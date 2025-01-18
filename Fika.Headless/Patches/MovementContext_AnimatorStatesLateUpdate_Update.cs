using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch aims to decrease the amount of CPU cycles spent updating data the headless does not see
    /// </summary>
    public class MovementContext_AnimatorStatesLateUpdate_Update : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MovementContext).GetMethod(nameof(MovementContext.AnimatorStatesLateUpdate));
        }

        [PatchPrefix]
        public static bool Prefix(Player ____player)
        {
            if (____player.IsYourPlayer)
            {
                return false; 
            }
            return true;
        }
    }
}
