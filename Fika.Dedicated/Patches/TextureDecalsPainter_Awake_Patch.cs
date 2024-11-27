using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	internal class TextureDecalsPainter_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(TextureDecalsPainter).GetMethod(nameof(TextureDecalsPainter.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(TextureDecalsPainter __instance)
		{
			GameObject.Destroy(__instance);
			return false;
		}
	}
}
