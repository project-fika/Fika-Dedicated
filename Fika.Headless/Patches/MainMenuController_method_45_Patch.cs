﻿using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch simulates clicking "Next" by calling the method bound to the event of the button
    /// </summary>
    internal class MainMenuController_method_45_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_45));
        }

        [PatchPrefix]
        public static bool Prefix(MainMenuController __instance)
        {
            __instance.method_77();
            return false;
        }
    }
}
