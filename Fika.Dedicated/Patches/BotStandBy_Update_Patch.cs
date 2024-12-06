using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
	/// <summary>
	/// The purpose of this patch is to disable bot sleeping on the dedicated host
	/// </summary>
	internal class BotStandBy_Update_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(BotStandBy).GetMethod(nameof(BotStandBy.Update));
		}

		[PatchPrefix]
		public static bool Prefix()
		{
			return false;
		}
	}
}
