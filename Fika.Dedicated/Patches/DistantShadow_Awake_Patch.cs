using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	internal class DistantShadow_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(DistantShadow).GetMethod(nameof(DistantShadow.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(DistantShadow __instance, RenderTexture[] ___renderTexture_10)
		{
			___renderTexture_10 = [];
			GameObject.Destroy(__instance);
			return false;
		}
	}
}
