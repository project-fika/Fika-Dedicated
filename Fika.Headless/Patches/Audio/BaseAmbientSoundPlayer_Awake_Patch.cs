using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class BaseAmbientSoundPlayer_Awake_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseAmbientSoundPlayer).GetMethod(nameof(BaseAmbientSoundPlayer.Awake));
        }

        [PatchPrefix]
        public static bool Prefix(BaseAmbientSoundPlayer __instance)
        {
            SoundPlayerRoomObserverComponent[] soundPlayers = __instance.gameObject.GetComponents<SoundPlayerRoomObserverComponent>();
            if (soundPlayers != null && soundPlayers.Length > 0)
            {
                Logger.LogInfo($"Destroying {soundPlayers.Length} SoundPlayerRoomObserverComponents");
                for (int i = 0; i < soundPlayers.Length; i++)
                {
                    GameObject.Destroy(soundPlayers[i]);
                }
            }
            GameObject.Destroy(__instance);
            return false;
        }
    }
}
