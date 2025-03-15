using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This prevents the season controller from running due to no graphics being used
    /// </summary>
    public class Class437_Run_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Class437).GetMethod(nameof(Class437.Run));
        }

        [PatchPrefix]
        public static bool Prefix(Class437 __instance, ref Task __result, ref Class437.Interface3 ___interface3_0)
        {
            ___interface3_0 = new Class437.Class447(__instance);
            __result = Task.CompletedTask;
            return false;
        }
    }
}
