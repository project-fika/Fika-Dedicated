using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// THis prevents the season controller from running due to no graphics being used
    /// </summary>
    public class Class442_Run_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Class438).GetMethod(nameof(Class438.Run));
        }

        [PatchPrefix]
        public static bool Prefix(Class438 __instance, ref Task __result, Class438.Interface3 ___interface3_0)
        {
            ___interface3_0 = new Class438.Class448(__instance);
            __result = Task.CompletedTask;
            return false;
        }
    }
}
