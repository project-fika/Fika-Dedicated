using SPT.Reflection.Patching;
using UnityEngine;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class StaticDeferredDecalRenderer_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(StaticDeferredDecalRenderer).GetMethod(nameof(StaticDeferredDecalRenderer.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(ScopeMaskRenderer __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
}
