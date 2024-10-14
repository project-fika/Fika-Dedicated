using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using Fika.Core;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.Patches;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using Fika.Core.UI.Custom;
using Fika.Core.UI.Patches;
using Fika.Dedicated.Classes;
using Fika.Dedicated.Patches;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace Fika.Dedicated
{
	[BepInPlugin("com.fika.dedicated", "Fika.Dedicated", "1.1.0")]
	[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("com.SPT.custom", BepInDependency.DependencyFlags.HardDependency)]
	public class FikaDedicatedPlugin : BaseUnityPlugin
	{
		public static FikaDedicatedPlugin Instance { get; private set; }
		public static ManualLogSource FikaDedicatedLogger;
		public static DedicatedRaidController raidController;
		public static int UpdateRate { get; internal set; }
		public DedicatedStatus Status { get; set; }

		private static DedicatedRaidWebSocketClient fikaDedicatedWebSocket;
		private float gcCounter;
		private Coroutine verifyConnectionsRoutine;

		private void Awake()
		{
			Instance = this;
			UpdateRate = 60;
			gcCounter = 0;

			FikaDedicatedLogger = Logger;

			FikaPlugin.AutoExtract.Value = true;
			FikaPlugin.QuestTypesToShareAndReceive.Value = 0;
			FikaPlugin.ConnectionTimeout.Value = 20;

			FikaPlugin.Instance.AllowFreeCam = true;
			FikaPlugin.Instance.AllowSpectateFreeCam = true;

			string[] commandLineArgs = Environment.GetCommandLineArgs();
			foreach (string arg in commandLineArgs)
			{
				if (arg.StartsWith("-updateRate="))
				{
					string trimmed = arg.Replace("-updateRate=", "");
					if (int.TryParse(trimmed, out int updateFreq))
					{
						UpdateRate = Mathf.Clamp(updateFreq, 30, 120);
						Application.targetFrameRate = UpdateRate;
						Logger.LogInfo("Setting UpdateRate to: " + UpdateRate);
					}

					continue;
				}

				if (arg.StartsWith("-sendRate="))
				{
					string trimmed = arg.Replace("-sendRate=", "");
					if (Enum.TryParse(trimmed, out FikaPlugin.ESendRate sendRate))
					{
						FikaPlugin.SendRate.Value = sendRate;
						Logger.LogInfo("Setting SendRate to: " + sendRate);
					}

					continue;
				}

				if (arg.StartsWith("-noDynamicAi"))
				{
					FikaPlugin.DynamicAI.Value = false;
					Logger.LogInfo("Disabling DynamicAI");

					continue;
				}
			}

			new DLSSPatch1().Enable();
			new DLSSPatch2().Enable();
			new DLSSPatch3().Enable();
			new DLSSPatch4().Enable();
			new VRAMPatch1().Enable();
			new VRAMPatch2().Enable();
			new VRAMPatch3().Enable();
			new VRAMPatch4().Enable();
			new SettingsPatch().Enable();
			new SessionResultExitStatusPatch().Enable();
			new MessageWindow_Show_Patch().Enable();
			new MenuScreen_Show_Patch().Enable();
			new HealthTreatmentScreen_IsAvailable_Patch().Enable();
			new CoopPlayer_CreateMovementContext_Patch().Enable();
			new Player_Init_Patch().Enable();
			new ValidateFormatPatch1().Enable();
			new ValidateFormatPatch2().Enable();
			new ValidateFormatPatch3().Enable();
			new GameWorld_OnGameStarted_Patch().Enable();
			new MainMenuController_method_46_Patch().Enable();
			new ConsoleScreen_OnProfileReceive_Patch().Enable();
			new Class428_Run_Patch().Enable();
			new Player_VisualPass_Patch().Enable();
			new IsReflexAvailablePatch().Enable();
			new AudioSourcePlayPatch().Enable();
			//InvokeRepeating("ClearRenderables", 1f, 1f);

			new TarkovApplication_method_18_Patch().Disable();
			new MenuScreen_Awake_Patch().Disable();

			Logger.LogInfo($"Fika.Dedicated loaded! OS: {SystemInfo.operatingSystem}");
			if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows)
			{
				Logger.LogWarning("You are not running an officially supported operating system by Fika. Minimal support will be given.");
			}

			EFTHardSettings hardSettings = EFTHardSettings.Instance;
			hardSettings.CULL_GROUNDER = 1000f;
			hardSettings.AnimatorCullDistance = 1000f;
			hardSettings.DEFERRED_DECALS_ENABLED = false;
			hardSettings.DRAW_DEFERRED_DECALS = false;
			hardSettings.PLAYER_HIT_DECALS_ENEBLED = false;
			hardSettings.STATIC_DEFERRED_DECALS_ENABLED = false;

			fikaDedicatedWebSocket = new DedicatedRaidWebSocketClient();
			fikaDedicatedWebSocket.Connect();

			StartCoroutine(RunPluginValidation());
		}

		protected void Update()
		{
			gcCounter += Time.deltaTime;

			if (gcCounter > 300)
			{
				Logger.LogInfo("Clearing memory");
				gcCounter = 0;
				GClass823.EmptyWorkingSet();
			}
		}

		// Done every second as a way to minimize processing time
		[Obsolete("Do not use", true)]
		private void ClearRenderables()
		{
			Stopwatch sw = Stopwatch.StartNew();
			Renderer[] renderers = FindObjectsOfType<Renderer>();
			foreach (Renderer renderer in renderers)
			{
				Destroy(renderer);
			}

			Logger.LogInfo($"ClearRenderables: ${sw.ElapsedMilliseconds}");
		}

		public void OnFikaStartRaid(StartDedicatedRequest request)
		{
			TarkovApplication tarkovApplication = (TarkovApplication)Singleton<ClientApplication<ISession>>.Instance;
			ISession session = tarkovApplication.GetClientBackEndSession();
			if (!session.LocationSettings.locations.TryGetValue(request.LocationId, out LocationSettingsClass.Location location))
			{
				Logger.LogError($"Failed to find location {request.LocationId}");
				return;
			}

			OfflineRaidSettingsMenuPatch_Override.UseCustomWeather = request.CustomWeather;

			Logger.LogInfo($"Starting on location {location.Name}");
			RaidSettings raidSettings = Traverse.Create(tarkovApplication).Field<RaidSettings>("_raidSettings").Value;
			Logger.LogInfo("Initialized raid settings");
			StartCoroutine(BeginFikaStartRaid(request, session, raidSettings, location));
		}

		private IEnumerator RunPluginValidation()
		{
			yield return new WaitForSeconds(5);
			VerifyPlugins();
		}

		private void VerifyPlugins()
		{
			List<string> invalidPluginList =
			[
				"com.Amanda.Graphics",
				"VIP.TommySoucy.MoreCheckmarks",
				"com.kmyuhkyuk.EFTApi",
				"com.mpstark.DynamicMaps",
				"IhanaMies.LootValue",
				"com.cactuspie.ramcleanerinterval",
				"com.TYR.DeClutter"
			];
			PluginInfo[] pluginInfos = [.. Chainloader.PluginInfos.Values];
			List<string> unsupportedMods = [];

			foreach (PluginInfo Info in pluginInfos)
			{
				if (invalidPluginList.Contains(Info.Metadata.GUID))
				{
					unsupportedMods.Add($"{Info.Metadata.Name}, GUID: {Info.Metadata.GUID}");
				}
			}

			if (unsupportedMods.Count > 0)
			{
				string modsString = string.Join("; ", unsupportedMods);
				Logger.LogFatal($"{unsupportedMods.Count} invalid plugins found, the game will be forcibly closed! Remove these mods: {modsString}");
				Application.Quit();
				return;
			}

			Logger.LogInfo("Plugins verified successfully");
		}

		private IEnumerator VerifyPlayersRoutine()
		{
			yield return new WaitForSeconds(300);
			if (Singleton<FikaServer>.Instance.NetServer.ConnectedPeersCount < 1)
			{
				int attempts = 0;
				while ((CoopGame)Singleton<IFikaGame>.Instance == null && attempts < 5)
				{
					yield return new WaitForSeconds(5);
					attempts++;
					if (attempts >= 5)
					{
						Logger.LogError("More than 5 attempts were required to get the CoopGame instance. Something is probably very wrong!");
					}
				}

				CoopGame coopGame = (CoopGame)Singleton<IFikaGame>.Instance;
				coopGame.StopFromCancel(FikaBackendUtils.Profile.ProfileId, ExitStatus.Runner);
				Logger.LogWarning("The were no connections after 5 minutes, terminating session...");
			}
		}

		private IEnumerator BeginFikaStartRaid(StartDedicatedRequest request, ISession session, RaidSettings raidSettings, LocationSettingsClass.Location location)
		{
			Status = DedicatedStatus.IN_RAID;

			Task.Run(async () =>
			{
				SetDedicatedStatusRequest setDedicatedStatusRequest = new(RequestHandler.SessionId, DedicatedStatus.IN_RAID);
				await FikaRequestHandler.SetDedicatedStatus(setDedicatedStatusRequest);
			});

			/*
             * Runs through the menus. Eventually this can be replaced
             * but it works for now and I was getting a CTD with other method
            */

			MenuScreen menuScreen;
			do
			{
				yield return StaticManager.Instance.WaitFrames(5, null);
				menuScreen = FindObjectOfType<MenuScreen>();
			} while (menuScreen == null);
			yield return null;

			menuScreen.method_9(); // main menu -> faction selection screen

			MatchMakerSideSelectionScreen sideSelectionScreen;
			do
			{
				yield return StaticManager.Instance.WaitFrames(5, null);
				sideSelectionScreen = FindObjectOfType<MatchMakerSideSelectionScreen>();
			} while (sideSelectionScreen == null);
			yield return null;

			Action<bool> targetFactionCallback = raidSettings.Side == ESideType.Pmc ?
				sideSelectionScreen.method_12 :
				sideSelectionScreen.method_13;
			targetFactionCallback(true); // select scav/pmc
			yield return null;

			sideSelectionScreen.method_11(request.Side); // select side

			yield return null;

			sideSelectionScreen.method_17(); // faction selection screen -> location selection screen
			yield return null;

			MatchMakerSelectionLocationScreen locationSelectionScreen;
			do
			{
				yield return StaticManager.Instance.WaitFrames(5, null);
				locationSelectionScreen = FindObjectOfType<MatchMakerSelectionLocationScreen>();
			} while (locationSelectionScreen == null);
			yield return null;

			locationSelectionScreen.Location_0 = session.LocationSettings.locations[request.LocationId];
			locationSelectionScreen.method_7(request.Time); // set time
			locationSelectionScreen.method_11(); // location selection screen -> offline raid screen

			MatchmakerOfflineRaidScreen offlineRaidScreen;
			do
			{
				yield return StaticManager.Instance.WaitFrames(5, null);
				offlineRaidScreen = FindObjectOfType<MatchmakerOfflineRaidScreen>();
			} while (offlineRaidScreen == null);
			yield return null;
			offlineRaidScreen.method_4(); // offline raid screen -> insurance screen

			if (raidSettings.Side != ESideType.Savage)
			{
				MatchmakerInsuranceScreen insuranceScreen;
				do
				{
					yield return StaticManager.Instance.WaitFrames(5, null);
					insuranceScreen = FindObjectOfType<MatchmakerInsuranceScreen>();
				} while (insuranceScreen == null);
				yield return null;
				insuranceScreen.method_8(); // insurance screen -> accept screen 
			}

			yield return null;

			raidSettings.PlayersSpawnPlace = request.SpawnPlace;
			raidSettings.MetabolismDisabled = request.MetabolismDisabled;
			raidSettings.BotSettings = request.BotSettings;
			raidSettings.WavesSettings = request.WavesSettings;
			raidSettings.TimeAndWeatherSettings = request.TimeAndWeatherSettings;

			MatchMakerAcceptScreen acceptScreen;
			do
			{
				yield return StaticManager.Instance.WaitFrames(5, null);
				acceptScreen = FindObjectOfType<MatchMakerAcceptScreen>();
			} while (acceptScreen == null);
			yield return null;

			yield return new WaitForSeconds(1f);
			MatchMakerUIScript fikaMatchMakerScript;
			do
			{
				yield return StaticManager.Instance.WaitFrames(5, null);
				fikaMatchMakerScript = FindObjectOfType<MatchMakerUIScript>();
			} while (fikaMatchMakerScript == null);
			yield return null;

			if (FikaPlugin.ForceIP.Value != "")
			{
				// We need to handle DNS entries as well
				string ip = FikaPlugin.ForceIP.Value;
				try
				{
					IPAddress[] dnsAddress = Dns.GetHostAddresses(FikaPlugin.ForceIP.Value);
					if (dnsAddress.Length > 0)
					{
						ip = dnsAddress[0].ToString();
					}
				}
				catch
				{

				}

				if (!IPAddress.TryParse(ip, out _))
				{
					Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("ERROR FORCING IP",
						$"'{ip}' is not a valid IP address to connect to! Check your 'Force IP' setting.",
						ErrorScreen.EButtonType.OkButton, 10f, null, null);
					yield break;
				}
			}

			if (FikaPlugin.ForceBindIP.Value != "Disabled")
			{
				if (!IPAddress.TryParse(FikaPlugin.ForceBindIP.Value, out _))
				{
					Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("ERROR BINDING",
						$"'{FikaPlugin.ForceBindIP.Value}' is not a valid IP address to bind to! Check your 'Force Bind IP' setting.",
						ErrorScreen.EButtonType.OkButton, 10f, null, null);
					yield break;
				}
			}

			Logger.LogInfo($"Starting with: {JsonConvert.SerializeObject(request)}");

			FikaBackendUtils.HostExpectedNumberOfPlayers = request.ExpectedNumPlayers + 1;
			Task createMatchTask = FikaBackendUtils.CreateMatch(session.Profile.ProfileId, session.Profile.Info.Nickname, raidSettings);
			while (!createMatchTask.IsCompleted)
			{
				yield return null;
			}
			FikaBackendUtils.IsDedicatedGame = true;

			verifyConnectionsRoutine = StartCoroutine(VerifyPlayersRoutine());

			fikaMatchMakerScript.AcceptButton.OnClick.Invoke();
		}

		public IEnumerator SetDedicatedStatusReady()
		{
			while (Status == DedicatedStatus.READY)
			{
				Task.Run(async () =>
				{
					SetDedicatedStatusRequest setDedicatedStatusRequest = new(RequestHandler.SessionId, DedicatedStatus.READY);
					await FikaRequestHandler.SetDedicatedStatus(setDedicatedStatusRequest);
				});

				yield return new WaitForSeconds(15.0f);
			}

			yield break;
		}

		public void StartSetDedicatedStatusReadyRoutine()
		{
			Status = DedicatedStatus.READY;
			StartCoroutine(SetDedicatedStatusReady());
		}
	}
}
