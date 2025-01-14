using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch skips a bunch of unneccesary methods
    /// </summary>
    internal class MainMenuController_method_73_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_73));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
