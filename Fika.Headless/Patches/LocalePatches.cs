using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches
{
    internal class LocalePatches
    {
        public void Enable()
        {
            new LocaleManagerClass_UpdateMainMenuLocales_Patch().Enable();
            new LocaleManagerClass_UpdateLocales_Patch().Enable();
        }
    }

    internal class LocaleManagerClass_UpdateMainMenuLocales_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocaleManagerClass).GetMethod(nameof(LocaleManagerClass.UpdateMainMenuLocales));
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

    internal class LocaleManagerClass_UpdateLocales_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocaleManagerClass).GetMethod(nameof(LocaleManagerClass.UpdateLocales));
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
