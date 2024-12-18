﻿using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class LocalDustParticlesParent_Awake_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(LocalDustParticlesParent).GetMethod(nameof(LocalDustParticlesParent.Awake));
		}

		[PatchPrefix]
		public static bool Prefix(LocalDustParticlesParent __instance)
		{
			GameObject.Destroy(__instance);
			return false;
		}
	}
}
