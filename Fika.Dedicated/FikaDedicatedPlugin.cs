using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
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
using Fika.Dedicated.Patches.DestroyGraphics;
using Fika.Dedicated.Patches.DLSS;
using Fika.Dedicated.Patches.TextureValidateFormat;
using Fika.Dedicated.Patches.VRAM;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Custom.Patches;
using SPT.Custom.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Fika.Dedicated
{
	[BepInPlugin("com.fika.dedicated", "Fika.Dedicated", DediVersion)]
	[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("com.SPT.custom", BepInDependency.DependencyFlags.HardDependency)]
	public class FikaDedicatedPlugin : BaseUnityPlugin
	{
		public const string DediVersion = "1.2.4";

        public static FikaDedicatedPlugin Instance { get; private set; }
        public static ManualLogSource FikaDedicatedLogger;
        public static DedicatedRaidController raidController;
        public static bool IsRunningWindows
        {
            get
            {
                return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
            }
        }
        public DedicatedStatus Status { get; set; }

        private static DedicatedRaidWebSocketClient fikaDedicatedWebSocket;
        private float gcCounter;
        private Coroutine verifyConnectionsRoutine;
        private bool invalidPluginsFound = false;


        public static ConfigEntry<int> UpdateRate { get; private set; }
        public static ConfigEntry<int> RAMCleanInterval { get; private set; }
        public static ConfigEntry<bool> ShouldBotsSleep { get; private set; }
        public static ConfigEntry<bool> ShouldDestroyGraphics { get; private set; }
        public static ConfigEntry<bool> DestroyRenderersOnSceneLoad { get; private set; }

        protected void Awake()
        {
            Instance = this;
            gcCounter = 0;

            FikaDedicatedLogger = Logger;

            SetupConfig();

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
            new MainMenuController_method_47_Patch().Enable();
            new ConsoleScreen_OnProfileReceive_Patch().Enable();
            new Class442_Run_Patch().Enable();
            new Player_VisualPass_Patch().Enable();
            new IsReflexAvailablePatch().Enable();
            new AudioSource_Play_Transpiler().Enable();
            new LevelSettings_ApplySettings_Transpiler().Enable();
            new LevelSettings_ApplyTreeWindSettings_Transpiler().Enable();
            new MainMenuController_method_44_Patch().Enable();
            new MainMenuController_method_45_Patch().Enable();
            new MainMenuController_method_72_Patch().Enable();
            new MainMenuController_method_73_Patch().Enable();
            new LocaleManagerClass_String_0_Patch().Enable();

            if (!ShouldBotsSleep.Value)
            {
                new BotStandBy_Update_Transpiler().Enable();
            }

            if (ShouldDestroyGraphics.Value)
            {
                DestroyGraphicsAutoloader.EnableDestroyGraphicsPatches();
            }

            //InvokeRepeating("ClearRenderables", 1f, 1f);

            new TarkovApplication_method_18_Patch().Disable();
            new MenuScreen_Awake_Patch().Disable();
            new MemoryCollectionPatch().Disable();
            new SetPreRaidSettingsScreenDefaultsPatch().Disable();

            Logger.LogInfo($"Fika.Dedicated loaded! OS: {SystemInfo.operatingSystem}");
            if (!IsRunningWindows)
            {
                Logger.LogWarning("You are not running an officially supported operating system by Fika. Minimal support will be given. Please cleanup your '/Logs' folder manually.");
            }
            else
            {
                CleanupLogFiles();
            }

            FikaBackendUtils.IsDedicated = true;

            fikaDedicatedWebSocket = new DedicatedRaidWebSocketClient();

            StartCoroutine(RunPluginValidation());
        }

        private void CleanupLogFiles()
        {
            string exePath = AppContext.BaseDirectory;
            string logsPath = Path.Combine(exePath, "Logs");
            if (!Directory.Exists(logsPath))
            {
                Logger.LogError("CleanupLogFiles: Could not finds '/Logs' folder!");
                return;
            }

            DirectoryInfo logsDir = new(logsPath);
            foreach (DirectoryInfo dir in logsDir.EnumerateDirectories())
            {
                try
                {
                    Logger.LogInfo($"CleanupLogFiles: Deleting {dir.Name}");
                    dir.Delete(true);
                }
                catch
                {
                    Logger.LogWarning($"CleanupLogFiles: Could not delete {dir.Name}, it's probably being used");
                }
            }
        }

        private void SetupConfig()
        {
            UpdateRate = Config.Bind("Dedicated", "Update Rate", 60,
                new ConfigDescription("How often the server should update (frame cap / tick rate). Only works if your machine can actually reach the selected setting",
                new AcceptableValueRange<int>(30, 120)));

            RAMCleanInterval = Config.Bind("Dedicated", "RAM Clean Interval", 5,
                new ConfigDescription("How often in minutes the RAM cleaner should run",
                new AcceptableValueRange<int>(1, 30)));

            ShouldBotsSleep = Config.Bind("Dedicated", "Bot sleeping", false,
                new ConfigDescription("Should the dedicated host allow bots to sleep? (BSG bot sleeping logic)"));

            ShouldDestroyGraphics = Config.Bind("Dedicated", "Destroy Graphics", true,
                new ConfigDescription("If the dedicated plugin should run patches to disable various graphical elements"));

            DestroyRenderersOnSceneLoad = Config.Bind("Dedicated", "Destroy Renderers", true,
                new ConfigDescription("If the dedicated plugin should hook scene loading to disable unnecessary renderers as well as unloading all materials (Requires 'Destroy Graphics' to be enabled)"));
        }

        protected void Update()
        {
            gcCounter += Time.deltaTime;

            if (gcCounter > (RAMCleanInterval.Value * 60))
            {
                Logger.LogDebug("Clearing memory");
                gcCounter = 0;
                MemoryControllerClass.EmptyWorkingSet();
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
            if (!TarkovApplication.Exist(out TarkovApplication tarkovApplication))
            {
                throw new NullReferenceException("OnFikaStartRaid: Could not find TarkovApplication!");
            }
            ISession session = tarkovApplication.GetClientBackEndSession();
            if (!session.LocationSettings.locations.TryGetValue(request.LocationId, out LocationSettingsClass.Location location))
            {
                Logger.LogError($"Failed to find location {request.LocationId}");
                return;
            }

            OfflineRaidSettingsMenuPatch_Override.UseCustomWeather = request.CustomWeather;

            Logger.LogInfo($"Starting on location {location.Name}");
            RaidSettings raidSettings = tarkovApplication.CurrentRaidSettings;
            Logger.LogInfo("Initialized raid settings");
            StartCoroutine(BeginFikaStartRaid(request, session, raidSettings, tarkovApplication));
        }

        private IEnumerator RunPluginValidation()
        {
            yield return new WaitForSeconds(5);
            VerifyPlugins();
            while (FikaPlugin.OfficialVersion == null)
            {
                yield return new WaitForSeconds(1);
            }

            FikaPlugin.AutoExtract.Value = true;
            FikaPlugin.QuestTypesToShareAndReceive.Value = 0;
            FikaPlugin.ConnectionTimeout.Value = 30;

            FikaPlugin.Instance.AllowFreeCam = true;
            FikaPlugin.Instance.AllowSpectateFreeCam = true;
        }

        private void VerifyPlugins()
        {
            List<string> invalidPluginList =
            [
                "com.Amanda.Graphics",
                "com.Amanda.Sense",
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
                Logger.LogFatal($"{unsupportedMods.Count} invalid plugins found, this dedicated host will not be available for hosting! Remove these mods: {modsString}");
                invalidPluginsFound = true;
                if (IsRunningWindows)
                {
                    MessageBoxHelper.Show($"{unsupportedMods.Count} invalid plugins found, this dedicated host will not be available for hosting! Check your log files for more information.",
                        "DEDICATED ERROR", MessageBoxHelper.MessageBoxType.OK);
                }
                Thread.Sleep(-1);
                return;
            }

            Logger.LogInfo("Plugins verified successfully");

            fikaDedicatedWebSocket.Connect();
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

        private IEnumerator BeginFikaStartRaid(StartDedicatedRequest request, ISession session, RaidSettings raidSettings, TarkovApplication tarkovApplication)
        {
            Status = DedicatedStatus.IN_RAID;

            SetDedicatedStatusRequest setDedicatedStatusRequest = new(RequestHandler.SessionId, DedicatedStatus.IN_RAID);
            Task statusTask = FikaRequestHandler.SetDedicatedStatus(setDedicatedStatusRequest);
            while (!statusTask.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }

            /*
             * Runs through the menus. Eventually this can be replaced
             * but it works for now and I was getting a CTD with other method
            */

            CommonUI commonUI = MonoBehaviourSingleton<CommonUI>.Instance;
            MenuScreen menuScreen = commonUI.MenuScreen;

            menuScreen.method_9(); // main menu -> faction selection screen

            MenuUI menuUI = MonoBehaviourSingleton<MenuUI>.Instance;

            MatchMakerSideSelectionScreen sideSelectionScreen = menuUI.MatchMakerSideSelectionScreen;
            do
            {
                yield return StaticManager.Instance.WaitFrames(5, null);
            } while (!sideSelectionScreen.isActiveAndEnabled);
            yield return null;

            Action<bool> targetFactionCallback = raidSettings.Side == ESideType.Pmc ?
                sideSelectionScreen.method_12 : sideSelectionScreen.method_13;
            targetFactionCallback(true); // select scav/pmc
            yield return null;

            sideSelectionScreen.method_11(request.Side); // select side

            yield return null;

            sideSelectionScreen.method_17(); // faction selection screen -> location selection screen
            yield return null;

            MatchMakerSelectionLocationScreen locationSelectionScreen = menuUI.MatchMakerSelectionLocationScreen;
            do
            {
                yield return StaticManager.Instance.WaitFrames(5, null);
            } while (!locationSelectionScreen.isActiveAndEnabled);
            yield return null;

            locationSelectionScreen.Location_0 = session.LocationSettings.locations[key: request.LocationId];
            locationSelectionScreen.method_7(request.Time); // set time
            locationSelectionScreen.method_11(); // location selection screen -> matchmaker accept screen (we skip with patches)

            raidSettings.PlayersSpawnPlace = request.SpawnPlace;
            raidSettings.MetabolismDisabled = request.MetabolismDisabled;
            raidSettings.BotSettings = request.BotSettings;
            raidSettings.WavesSettings = request.WavesSettings;
            raidSettings.TimeAndWeatherSettings = request.TimeAndWeatherSettings;
            raidSettings.isLocationTransition = false;
            raidSettings.isInTransition = false;
            raidSettings.BotSettings.BotAmount = request.WavesSettings.BotAmount;
            raidSettings.RaidMode = ERaidMode.Local;
            raidSettings.IsPveOffline = true;

            MainMenuController mmc = Traverse.Create(tarkovApplication).Field<MainMenuController>("mainMenuController").Value;
            Traverse mmcTraverse = Traverse.Create(mmc);
            mmcTraverse.Field<RaidSettings>("raidSettings_0").Value = raidSettings;
            mmcTraverse.Field<RaidSettings>("raidSettings_1").Value = raidSettings;

            yield return null;

            MatchMakerAcceptScreen acceptScreen = menuUI.MatchMakerAcceptScreen;
            do
            {
                yield return StaticManager.Instance.WaitFrames(5, null);
            } while (!acceptScreen.isActiveAndEnabled);
            yield return null;

            yield return new WaitForSeconds(1f);
            MatchMakerUIScript fikaMatchMakerScript;
            do
            {
                yield return StaticManager.Instance.WaitFrames(5, null);
                fikaMatchMakerScript = acceptScreen.gameObject.GetComponent<MatchMakerUIScript>();
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

            Logger.LogInfo($"Starting with: {JsonConvert.SerializeObject(raidSettings)}");

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
                Task.Run(SetStatusToReady);
                yield return new WaitForSeconds(15f);
            }

            yield break;
        }

        private async void SetStatusToReady()
        {
            SetDedicatedStatusRequest setDedicatedStatusRequest = new(RequestHandler.SessionId, DedicatedStatus.READY);
            await FikaRequestHandler.SetDedicatedStatus(setDedicatedStatusRequest);
        }

        public void StartSetDedicatedStatusReadyRoutine()
        {
            Status = invalidPluginsFound ? DedicatedStatus.IN_RAID : DedicatedStatus.READY;
            StartCoroutine(SetDedicatedStatusReady());
        }
    }
}
