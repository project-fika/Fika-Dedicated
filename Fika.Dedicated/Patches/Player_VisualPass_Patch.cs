using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	/// <summary>
	/// This patch aims to decrease the amount of CPU cycles spent updating data the dedicated does not see
	/// </summary>
	public class Player_VisualPass_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Player).GetMethod(nameof(Player.VisualPass));
		}

		[PatchPrefix]
		public static bool Prefix(Player __instance, bool ____bodyupdated, bool ____armsupdated)
		{
			__instance.BodyAnimatorCommon.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			__instance.ArmsAnimatorCommon.cullingMode = AnimatorCullingMode.AlwaysAnimate;

			if (____bodyupdated)
			{
				__instance.method_14();
				__instance.MouseLook(false);
			}

			if (____armsupdated || __instance.ArmsUpdateMode is Player.EUpdateMode.Auto)
			{
				__instance.ProceduralWeaponAnimation.LateTransformations(Time.deltaTime);
				if (__instance.HandsController != null)
				{
					__instance.HandsController.ManualLateUpdate(Time.deltaTime);
				}
			}

			return false;
		}
	}
}
