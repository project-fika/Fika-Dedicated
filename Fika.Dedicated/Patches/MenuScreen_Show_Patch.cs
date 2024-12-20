using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
	public class MenuScreen_Show_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(MenuScreen).GetMethod(nameof(MenuScreen.Show),
				[
					typeof(Profile),
					typeof(MatchmakerPlayerControllerClass),
					typeof(ESessionMode)
				]);
		}

		[PatchPostfix]
		public static void PatchPostfix()
		{
			FikaDedicatedPlugin.Instance.StartSetDedicatedStatusReadyRoutine();
		}
	}
}