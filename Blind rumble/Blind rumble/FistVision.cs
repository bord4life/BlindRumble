using BlindRumble;
using Il2CppRUMBLE.Players.Subsystems;
using RumbleModdingAPI.RMAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Il2CppRUMBLE.Players.Subsystems.PlayerVR;
using HarmonyLib;
using UnityEngine.Playables;

namespace Blind_rumble
{
    internal class FistVision
    {
        //Allows fistbumps to give sonar pulses
        [HarmonyPatch(typeof(Il2CppRUMBLE.Players.Subsystems.PlayerBoxInteractionSystem), "OnPlayerBoxInteraction", new Type[] { typeof(PlayerBoxInteractionTrigger), typeof(PlayerBoxInteractionTrigger) })]
        public static class FistbumpPatch
        {
            private static void Postfix(PlayerBoxInteractionTrigger first, PlayerBoxInteractionTrigger second)
            {
                if (Class1.fistBumpPrevention == false)
                {
                    Class1.fistBumpPrevention = true;
                }
                else
                {
                    Class1.fistBumpPrevention = false;
                    return;
                }

                if (Class1.modEnabled == false || Class1.bodyMaterial == null)
                {
                    return;
                }

                if (Class1.playerInteractionTimestamps.ContainsKey(first))
                {
                    float previousTimestamp = Class1.playerInteractionTimestamps[first];
                    float currentTimestamp = (float)first.GetPreviousTime();

                    if (currentTimestamp - previousTimestamp <= 0.5f)
                    {
                        return;
                    }

                    Class1.playerInteractionTimestamps[first] = currentTimestamp;
                }
                else if (first.parentSystem.previousBoxTimestamp > 0.1f)
                {
                    Class1.playerInteractionTimestamps.Add(first, (float)first.GetPreviousTime());
                }
                else
                {
                    return;
                }

                if (Class1.playerInteractionTimestamps.ContainsKey(second))
                {
                    float previousTimestamp = Class1.playerInteractionTimestamps[second];
                    float currentTimestamp = second.parentSystem.previousBoxTimestamp;

                    if (currentTimestamp - previousTimestamp <= 0.5f)
                    {
                        return;
                    }

                    Class1.playerInteractionTimestamps[second] = currentTimestamp;
                }
                else if (second.parentSystem.previousBoxTimestamp > 0.1f)
                {
                    Class1.playerInteractionTimestamps.Add(second, second.parentSystem.previousBoxTimestamp);
                }
                else
                {
                    return;
                }


                var playerManager = Managers.GetPlayerManager();
                Transform poolParent = Pools.Structures.GetPoolCube().transform.parent;
                List<GameObjects> overlappedObjects = new List<GameObjects>();

                Class1.ActivateSonar(first.gameObject.transform.position, 5f, poolParent, overlappedObjects);
            }
        }
    }
}
