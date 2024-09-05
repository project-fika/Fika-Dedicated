using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Dedicated.Patches
{
	internal class Class417_Run_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Class417).GetMethod(nameof(Class417.Run));
		}

		[PatchPrefix]
		public static bool Prefix(Class417 __instance, ref Task __result, Class417.Interface3 ___interface3_0)
		{
			___interface3_0 = new Class417.Class425(__instance);
			__result = Task.CompletedTask;
			return false;
		}
	}
}
