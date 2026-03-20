using Il2CppRUMBLE.Players.Subsystems;
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.Playables;
using Il2CppRUMBLE.Managers;
using UnityEngine;

namespace BlindRumble
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


                var playerManager = PlayerManager.instance.LocalPlayer;
                Transform poolParent = PoolManager.Instance.GetPool("RockCube").poolParent;
                List<GameObject> overlappedObjects = new List<GameObject>();

                Class1.ActivateSonar(first.gameObject.transform.position, 5f, poolParent, overlappedObjects);
            }
        }
    }
}
