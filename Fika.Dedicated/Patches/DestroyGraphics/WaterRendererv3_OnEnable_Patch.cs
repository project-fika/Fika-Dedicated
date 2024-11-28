using SPT.Reflection.Patching;
using UnityEngine;
using System.Reflection;
using WaterSSR;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class WaterRendererv3_OnEnable_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(WaterRendererv3).GetMethod(nameof(WaterRendererv3.OnEnable));
		}

		[PatchPrefix]
		public static bool Prefix(ScopeMaskRenderer __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
}
