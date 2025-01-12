using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
    public class TubeLight_Start_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TubeLight).GetMethod(nameof(TubeLight.Start));
        }

        [PatchPrefix]
        public static bool Prefix(TubeLight __instance)
        {
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
