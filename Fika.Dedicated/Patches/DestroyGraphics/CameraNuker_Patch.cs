using SPT.Reflection.Patching;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	/// <summary>
	/// This patch deletes various effects out of the player's camera that the dedicated will never see.
	/// </summary>
	internal class CameraNuker : ModulePatch
	{
		// Whitelisted types we can delete out of the camera that will not cause it to throw exceptions.
		private static List<string> DeleteTypes = [
			"HBAO",
			"TOD_Scattering",
			"MBOIT_Scattering",
			"Undithering",
			"ContactShadows",
			"NightVision",
			"ThermalVision",
			"Bloom",
			"UltimateBloom",
			"AmbientOcclusion",
			"PrismEffects",
			"BloomAndFlares",
			"DesaturateEffect",
			"Antialiasing",
			"SceneCameraFollow",
			"ChromaticAberration",
			"FastBlur",
			"TextureMask",
			"Tonemapping",
			"RainScreenDrops",
			"ScreenWater",
			"SSAOMask",
			"DepthOfField",
			"InfectionEffect",
			"InventoryBlur",
			"CC_BleachBypass",
			"CC_ContrastVignette",
			"CC_DoubleVision",
			"CC_HueFocus",
			"CC_RadialBlur",
			"CC_Sharpen",
			"CC_Technicolor",
			"CC_FastVignette",
			"CC_Blend",
			"CC_Wiggle",
			"CC_Vintage",
			"CC_BrightnessContrastGamma",
			"PostprocessGrayscale",
			"MotionBlur",
			"HysteresisFilter",
			"DistortCameraFX",
			];

		protected override MethodBase GetTargetMethod()
		{
			return typeof(CameraClass).GetMethod(nameof(CameraClass.method_2));
		}

		[PatchPostfix]
		public static void Postfix(CameraClass __instance)
		{
			Component[] components = __instance.Camera.GetComponents(typeof(MonoBehaviour));

			foreach (Component component in components)
			{
				string type = component.GetType().Name;

				if (DeleteTypes.Contains(type))
				{
					Object.Destroy(component);
				}
			}
		}
	}
}
