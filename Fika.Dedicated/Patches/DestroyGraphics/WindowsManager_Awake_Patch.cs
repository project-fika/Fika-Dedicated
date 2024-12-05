using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	/*
	internal class WindowsManager_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(WindowsManager).GetMethod(nameof(WindowsManager.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(WindowsManager __instance)
		{
			Object.Destroy(__instance);
			return false;
		}
	}
	*/
}
