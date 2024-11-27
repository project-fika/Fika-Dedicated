using GPUInstancer;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class GPUInstancerHiZOcclusionGenerator_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(GPUInstancerHiZOcclusionGenerator).GetMethod(nameof(GPUInstancerHiZOcclusionGenerator.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(GPUInstancerHiZOcclusionGenerator __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
}
