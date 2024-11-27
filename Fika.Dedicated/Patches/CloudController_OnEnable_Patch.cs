using EFT.Rendering.Clouds;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fika.Dedicated.Patches
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
			GameObject.Destroy(__instance);
			return false;
		}
	}
}
