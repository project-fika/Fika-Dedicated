using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	internal class VolumetricLightRenderer_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(VolumetricLightRenderer).GetMethod(nameof(VolumetricLightRenderer.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(VolumetricLightRenderer __instance)
		{
			GameObject.Destroy(__instance);
			return false;
		}
	}
}
