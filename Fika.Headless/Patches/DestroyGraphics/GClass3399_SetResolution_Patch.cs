using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class GClass3399_SetResolution_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3399).GetMethod(nameof(GClass3399.SetResolution));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
