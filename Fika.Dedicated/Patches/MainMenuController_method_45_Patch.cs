using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
	internal class MainMenuController_method_45_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(MainMenuController).GetMethod(nameof(MainMenuController.method_45));
		}

		[PatchPrefix]
		public static bool Prefix(MainMenuController __instance)
		{
			__instance.method_46();
			return false;
		}
	}
}
