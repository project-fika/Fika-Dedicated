using EFT;
using Fika.Core.Coop.ObservedClasses;
using Fika.Core.Coop.Players;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	public class CoopPlayer_CreateMovementContext_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(CoopPlayer).GetMethod(nameof(CoopPlayer.CreateMovementContext));
		}

		[PatchPrefix]
		private static bool Prefix(Player __instance)
		{
			if (__instance.IsYourPlayer)
			{
				LayerMask movement_MASK = EFTHardSettings.Instance.MOVEMENT_MASK;
				__instance.MovementContext = DedicatedMovementContext.Create(__instance, new Func<IAnimator>(__instance.GetBodyAnimatorCommon),
					new Func<ICharacterController>(__instance.GetCharacterControllerCommon), movement_MASK);

				return false;
			}

			return true;
		}
	}
}
