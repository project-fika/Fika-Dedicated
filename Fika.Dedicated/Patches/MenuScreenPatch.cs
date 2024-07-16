using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    public class MenuScreenPatch : ModulePatch
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
        static void PatchPostfix()
        {
            FikaDedicatedPlugin.Instance.StartSetDedicatedStatusRoutine();
        }
    }
}