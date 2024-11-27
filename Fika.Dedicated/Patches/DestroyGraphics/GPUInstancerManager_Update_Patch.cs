using GPUInstancer;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class GPUInstancerManager_Update_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(GPUInstancerManager).GetMethod(nameof(GPUInstancerManager.Update));
		}

		[PatchPrefix]
		public static bool Prefix()
		{
			return false;
		}
	}
}
