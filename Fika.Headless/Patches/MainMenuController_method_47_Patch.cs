using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class MainMenuController_method_47_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_47));
        }

        [PatchPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
