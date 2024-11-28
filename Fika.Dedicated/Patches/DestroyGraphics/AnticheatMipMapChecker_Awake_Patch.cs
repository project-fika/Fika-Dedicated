using SPT.Reflection.Patching;
using UnityEngine;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class AnticheatMipMapChecker_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(AnticheatMipMapChecker).GetMethod(nameof(AnticheatMipMapChecker.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(AnticheatMipMapChecker __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
}

