using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch simulates clicking "Next" by calling the method bound to the event of the button
    /// </summary>
    internal class MainMenuControllerClass_method_46_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass).GetMethod(nameof(MainMenuControllerClass.method_46));
        }

        [PatchPrefix]
        public static bool Prefix(MainMenuControllerClass __instance)
        {
            __instance.method_78();
            return false;
        }
    }
}
