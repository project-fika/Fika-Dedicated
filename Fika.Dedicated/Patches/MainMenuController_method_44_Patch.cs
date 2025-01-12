using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    internal class MainMenuController_method_44_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_44));
        }

        [PatchPrefix]
        public static bool Prefix(MainMenuController __instance)
        {
            __instance.method_76();
            return false;
        }
    }
}
