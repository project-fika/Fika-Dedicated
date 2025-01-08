using GPUInstancer;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class GPUInstancerDetailManager_Awake_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GPUInstancerDetailManager).GetMethod(nameof(GPUInstancerDetailManager.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(GPUInstancerDetailManager __instance)
        {
            Object.Destroy(__instance);
            return false;
        }
    }
}
