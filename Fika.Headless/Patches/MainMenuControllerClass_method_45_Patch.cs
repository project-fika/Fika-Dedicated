using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch simulates clicking "Next" by calling the method bound to the event of the button
    /// </summary>
    internal class MainMenuControllerClass_method_45_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass).GetMethod(nameof(MainMenuControllerClass.method_45));
        }

        [PatchPrefix]
        public static bool Prefix(MainMenuControllerClass __instance)
        {
            __instance.method_77();
            return false;
        }
    }
}
