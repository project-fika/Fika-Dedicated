using EFT;
using EFT.HealthSystem;
using Fika.Core.Coop.Players;
using Fika.Dedicated.Classes;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
    public class HealthControllerPlayerAfterInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod(nameof(Player.Init));
        }

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            DedicatedRaidController raidController = FikaDedicatedPlugin.raidController;
            if (raidController == null)
            {
                FikaDedicatedPlugin.FikaDedicatedLogger.LogError("RaidController was null during player init!");
                FikaDedicatedPlugin.raidController = new();
                raidController = FikaDedicatedPlugin.raidController;
            }

            if (__instance.IsYourPlayer)
            {
                ActiveHealthController healthController = __instance.ActiveHealthController;
                if (healthController != null)
                {
                    healthController.SetDamageCoeff(0f);
                    healthController.DisableMetabolism();
                }

                Vector3 currentPosition = __instance.Position;
                __instance.Teleport(new(currentPosition.x, currentPosition.y - 50f, currentPosition.z));

                raidController.MainPlayer = (CoopPlayer)__instance;
            }

            if (__instance is ObservedCoopPlayer observedCoopPlayer)
            {
                if (raidController != null)
                {
                    if (raidController.TargetPlayer == null)
                    {
                        raidController.TargetPlayer = observedCoopPlayer;
                        FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo($"Setting {observedCoopPlayer.Profile.Info.MainProfileNickname} as TargetPlayer"); 
                    }
                }
            }
        }
    }
}
