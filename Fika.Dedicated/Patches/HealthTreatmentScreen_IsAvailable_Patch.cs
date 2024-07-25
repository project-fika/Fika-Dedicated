using EFT.UI.SessionEnd;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    public class HealthTreatmentScreen_IsAvailable_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HealthTreatmentScreen).GetMethod(nameof(HealthTreatmentScreen.IsAvailable), BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPrefix]
        static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
