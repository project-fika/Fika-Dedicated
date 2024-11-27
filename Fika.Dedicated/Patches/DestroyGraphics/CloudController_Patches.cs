using EFT.Rendering.Clouds;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class CloudController_OnEnable_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(CloudController).GetMethod(nameof(CloudController.OnEnable));
		}

		[PatchPrefix]
		public static bool Prefix(CloudController __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}

	internal class CloudController_UpdateAmbient_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(CloudController).GetMethod(nameof(CloudController.UpdateAmbient));
		}

		[PatchPrefix]
		public static bool Prefix()
		{
			return false;
		}
	}
}
