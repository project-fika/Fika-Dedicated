using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    /// <summary>
    /// This patch ensures that the raid settings are skipped if you are playing as a scav
    /// </summary>
    internal class MainMenuController_method_73_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_73));
        }

        [PatchPostfix]
        public static void Postfix(MainMenuController __instance, RaidSettings ___raidSettings_0)
        {
            if (___raidSettings_0.IsScav)
            {
                __instance.method_46();
            }
        }
    }
}
