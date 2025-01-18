using EFT;
using EFT.Animations;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch aims to decrease the amount of CPU cycles spent updating data the headless does not see
    /// </summary>
    public class ProceduralWeaponAnimation_StartFovCoroutine_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ProceduralWeaponAnimation).GetMethod(nameof(ProceduralWeaponAnimation.StartFovCoroutine));
        }

        [PatchPrefix]
        public static bool Prefix(Player player)
        {
            return false;
            if (player.IsYourPlayer)
            {
                return false;
            }
            return true;
        }
    }
}
