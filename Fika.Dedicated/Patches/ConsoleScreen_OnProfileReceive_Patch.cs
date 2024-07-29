using EFT;
using EFT.UI;
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
            if (!profile.Nickname.Contains("dedicated_"))
            {
                FikaDedicatedPlugin.FikaDedicatedLogger.LogError("Not running a dedicated profile! Exiting...");
                Application.Quit();
            }
        }
    }
}
