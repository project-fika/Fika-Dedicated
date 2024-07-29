using EFT;
using EFT.UI;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
    internal class ConsoleScreen_OnProfileReceive_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ConsoleScreen).GetMethod(nameof(ConsoleScreen.OnProfileReceive));
        }

        [PatchPostfix]
        public static void Prefix(Profile profile)
        {
            if (profile.Nickname.Contains("dedicated_"))
            {
                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
                {
                    MessageBoxHelper.Show($"You are not starting with a dedicated profile!\nRead the documentation again!", "FIKA ERROR", MessageBoxHelper.MessageBoxType.OK);
                }
                else
                {
                    FikaDedicatedPlugin.FikaDedicatedLogger.LogError("Not running a dedicated profile! Read the documentation again! Exiting...");
                }
                Application.Quit();
            }
            else
            {
                FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo("Profile verified");
            }
        }
    }
}
