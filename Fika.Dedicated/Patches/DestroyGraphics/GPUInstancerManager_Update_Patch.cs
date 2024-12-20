using GPUInstancer;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	public class GPUInstancerManager_Update_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(GPUInstancerManager).GetMethod(nameof(GPUInstancerManager.Update));
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
