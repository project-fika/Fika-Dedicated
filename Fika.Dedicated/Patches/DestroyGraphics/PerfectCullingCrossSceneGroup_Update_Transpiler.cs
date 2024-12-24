using HarmonyLib;
using Koenigz.PerfectCulling.EFT;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class PerfectCullingCrossSceneGroup_Update_Transpiler : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(PerfectCullingCrossSceneGroup).GetMethod(nameof(PerfectCullingCrossSceneGroup.Update));
		}

		[PatchTranspiler]
		public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
		{
			// Create a new set of instructions
			List<CodeInstruction> instructionsList =
			[
				new CodeInstruction(OpCodes.Ret) // Return immediately
            ];

			return instructionsList;
		}
	}
}
