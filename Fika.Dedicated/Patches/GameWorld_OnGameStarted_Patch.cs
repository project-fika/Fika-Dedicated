using Comfort.Common;
using EFT;
using Fika.Core.Coop.Players;
using Fika.Dedicated.Classes;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
    public class GameWorld_OnGameStarted_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            DedicatedRaidController raidController = FikaDedicatedPlugin.raidController;
            if (raidController == null)
            {
                FikaDedicatedPlugin.raidController = Singleton<GameWorld>.Instance.gameObject.AddComponent<DedicatedRaidController>();
                raidController = FikaDedicatedPlugin.raidController;

                raidController.MainPlayer = (CoopPlayer)__instance.MainPlayer;
                raidController.MainPlayer.MovementContext.PitchLimit = new Vector2(-90, -90);
            }

			CameraClass.Instance.SetOcclusionCullingEnabled(false);
        }
    }
}
