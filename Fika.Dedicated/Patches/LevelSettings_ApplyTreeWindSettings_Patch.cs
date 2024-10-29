using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
	public class LevelSettings_ApplyTreeWindSettings_Patch : ModulePatch
	{
		/// <summary>
		/// Prevents unneccesary code from running
		/// </summary>
		protected override MethodBase GetTargetMethod()
		{
			return typeof(LevelSettings).GetMethod(nameof(LevelSettings.ApplyTreeWindSettings));
		}

		[PatchPrefix]
		private static bool Prefix()
		{
			return false;
		}
	}
}
