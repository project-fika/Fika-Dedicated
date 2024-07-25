using Fika.Core.Coop.Components;
using Fika.Core.Coop.Players;
using UnityEngine;

namespace Fika.Dedicated.Classes
{
    public class DedicatedRaidController : MonoBehaviour
    {
        public CoopPlayer MainPlayer { get; set; }
        private CoopPlayer targetPlayer;
        private int counter;

        private void Start()
        {
            counter = 0;
        }

        private void Update()
        {
            counter++;

            if (counter > 300)
            {
                counter = 0;

                if (targetPlayer != null)
                {
                    if (!targetPlayer.HealthController.IsAlive)
                    {
                        FindNewPlayer();
                        return;
                    }

                    Vector3 currentPosition = targetPlayer.Position;
                    MainPlayer.Teleport(new(currentPosition.x, currentPosition.y - 50, currentPosition.z));
                }
                else
                {
                    FindNewPlayer();
                }
            }
        }

        public void FindNewPlayer()
        {
            if (CoopHandler.TryGetCoopHandler(out CoopHandler coopHandler))
            {
                foreach (CoopPlayer player in coopHandler.HumanPlayers)
                {
                    if (player.HealthController.IsAlive && !player.IsYourPlayer)
                    {
                        targetPlayer = player;
                        FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo("DedicatedRaidController: New player: " + player.Profile.Info.MainProfileNickname);
                        return;
                    }
                }
            }

            targetPlayer = null;
            FikaDedicatedPlugin.FikaDedicatedLogger.LogWarning("No more players found as targets");
            Destroy(this);
        }
    }
}
