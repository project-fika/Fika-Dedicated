using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	internal class AmbientLight_Start_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(AmbientLight).GetMethod(nameof(AmbientLight.Start));
		}

		[PatchPrefix]
		public static bool Prefix(AmbientLight __instance)
		{
			GameObject.Destroy(__instance);
			return false;
		}
	}
}
