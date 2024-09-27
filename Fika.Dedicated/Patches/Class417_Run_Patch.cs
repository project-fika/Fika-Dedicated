using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Dedicated.Patches
{
	/// <summary>
	/// THis prevents the season controller from running due to no graphics being used
	/// </summary>
	internal class Class428_Run_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Class428).GetMethod(nameof(Class428.Run));
		}

		[PatchPrefix]
		public static bool Prefix(Class428 __instance, ref Task __result, Class428.Interface3 ___interface3_0)
		{
			___interface3_0 = new Class428.Class436(__instance);
			__result = Task.CompletedTask;
			return false;
		}
	}
}
