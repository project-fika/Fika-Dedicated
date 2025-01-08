using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
    public class DistortRenderer_Start_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DistortRenderer).GetMethod(nameof(DistortRenderer.Start));
        }

        [PatchPrefix]
        public static bool Prefix(DistortRenderer __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
