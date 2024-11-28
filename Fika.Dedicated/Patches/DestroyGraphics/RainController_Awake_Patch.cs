using SPT.Reflection.Patching;
using UnityEngine;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class RainController_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(RainController).GetMethod(nameof(RainController.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(RainController __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
}
