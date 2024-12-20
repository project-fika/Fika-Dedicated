using EFT.Visual;
using SPT.Reflection.Patching;
using UnityEngine;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	public class Flicker_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Flicker).GetMethod(nameof(Flicker.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(Flicker __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
}
