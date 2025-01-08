using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    internal class MainMenuController_method_72_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_72));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
