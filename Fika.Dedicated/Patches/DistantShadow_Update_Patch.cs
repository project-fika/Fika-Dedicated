using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
	internal class DistantShadow_Update_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(DistantShadow).GetMethod(nameof(DistantShadow.Update));
		}

		[PatchPrefix]
		public static bool Prefix()
		{
			return false;
		}
	}
}
