using EFT;
using Fika.Core.Coop.Components;
using Fika.Core.Coop.Players;
using UnityEngine;

namespace Fika.Dedicated.Classes
{
    public class DedicatedRaidController
    {
        public CoopPlayer TargetPlayer
        {
            get
            {
                return targetPlayer;
            }
            set
            {
                targetPlayer = value;
                targetPlayer.HealthController.DiedEvent += HealthController_DiedEvent;
                MainPlayer.Transform.Original.SetParent(targetPlayer.Transform.Original);
                MainPlayer.Transform.localPosition = Vector3.down * 50;
            }
        }
        public CoopPlayer MainPlayer { get; set; }
        private CoopPlayer targetPlayer;

        private void HealthController_DiedEvent(EDamageType obj)
        {
            targetPlayer.HealthController.DiedEvent -= HealthController_DiedEvent;

            if (CoopHandler.TryGetCoopHandler(out CoopHandler coopHandler))
            {
                foreach (CoopPlayer player in coopHandler.HumanPlayers)
                {
                    if (player.HealthController.IsAlive && !player.IsYourPlayer)
                    {
                        TargetPlayer = player;
                        return;
                    }
                }
            }

            MainPlayer.Transform.Original.SetParent(null);
            FikaDedicatedPlugin.FikaDedicatedLogger.LogWarning("No more players found as targets");
        }

        public void Dispose()
        {
            targetPlayer.HealthController.DiedEvent -= HealthController_DiedEvent;
            targetPlayer = null;
            MainPlayer = null;
        }
    }
}
