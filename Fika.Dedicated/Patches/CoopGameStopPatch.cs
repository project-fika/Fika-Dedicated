using Fika.Core.Coop.GameMode;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    internal class CoopGameStopPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CoopGame).GetMethod(nameof(CoopGame.Extract));
        }

        [PatchPostfix]
        public static void Postfix()
        {
            if (FikaDedicatedPlugin.raidController != null)
            {
                FikaDedicatedPlugin.raidController.Dispose();
                FikaDedicatedPlugin.raidController = null;
            }
        }
    }
}
