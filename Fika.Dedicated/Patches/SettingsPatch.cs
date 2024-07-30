using Comfort.Common;
using EFT;
using EFT.Settings.Graphics;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
    internal class SettingsPatch : ModulePatch
    {
        private static bool HasSet = false;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_23));
        }

        [PatchPostfix]
        public static void Postfix()
        {
            if (HasSet)
            {
                return;
            }

            SharedGameSettingsClass gameSettings = Singleton<SharedGameSettingsClass>.Instance;

            if (gameSettings == null)
            {
                return;
            }

            FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo("Setting graphics and volume");

            gameSettings.Sound.Settings.OverallVolume.SetValue(0);
            gameSettings.Sound.Settings.BinauralSound.SetValue(false);
            gameSettings.Sound.Settings.VoipEnabled.SetValue(false);

            gameSettings.Graphics.Settings.ShadowsQuality.SetValue(0);
            gameSettings.Graphics.Settings.TextureQuality.SetValue(0);
            gameSettings.Graphics.Settings.SuperSampling.SetValue(ESamplingMode.DownX05);
            gameSettings.Graphics.Settings.AnisotropicFiltering.SetValue(AnisotropicFiltering.Disable);
            gameSettings.Graphics.Settings.OverallVisibility.SetValue(400);
            gameSettings.Graphics.Settings.LodBias.SetValue(2);
            gameSettings.Graphics.Settings.Ssao.SetValue(ESSAOMode.Off);
            gameSettings.Graphics.Settings.SSR.SetValue(ESSRMode.Off);
            gameSettings.Graphics.Settings.AntiAliasing.SetValue(EAntialiasingMode.None);
            gameSettings.Graphics.Settings.NVidiaReflex.SetValue(ENvidiaReflexMode.Off);
            gameSettings.Graphics.Settings.GrassShadow.SetValue(false);
            gameSettings.Graphics.Settings.ChromaticAberrations.SetValue(false);
            gameSettings.Graphics.Settings.Noise.SetValue(false);
            gameSettings.Graphics.Settings.ZBlur.SetValue(false);
            gameSettings.Graphics.Settings.HighQualityColor.SetValue(false);
            gameSettings.Graphics.Settings.MipStreaming.SetValue(false);
            gameSettings.Graphics.Settings.SdTarkovStreets.SetValue(true);
            gameSettings.Graphics.Settings.DLSSMode.SetValue(EDLSSMode.Off);
            gameSettings.Graphics.Settings.FSRMode.SetValue(EFSRMode.Off);
            gameSettings.Graphics.Settings.FSR2Mode.SetValue(EFSR2Mode.Off);

            gameSettings.Graphics.Settings.LobbyFramerate.SetValue(30);
            gameSettings.Graphics.Settings.GameFramerate.SetValue(60);

            gameSettings.Game.Settings.EnableHideoutPreload.SetValue(false);

            int ratio = EftResolution.smethod_0(1024, 768);
            gameSettings.Graphics.Settings.DisplaySettings.SetValue(new()
            {
                AspectRatio = new(1024 / ratio, 768 / ratio),
                Display = 0,
                FullScreenMode = FullScreenMode.Windowed,
                Resolution = new(1024, 768)
            });

            gameSettings.Sound.Save();
            gameSettings.Graphics.Save();

            HasSet = true;
        }
    }
}
