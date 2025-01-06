using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class IsReflexAvailablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3248).GetMethods().Where(x => x.ReturnType == typeof(bool)).FirstOrDefault();
        }

        [PatchPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
