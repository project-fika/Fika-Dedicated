using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
    public class GClass3324_SetResolution_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3324).GetMethod(nameof(GClass3324.SetResolution));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
