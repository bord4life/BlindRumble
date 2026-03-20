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
                if (SonarMode.fistBumpPrevention == false)
                {
                    SonarMode.fistBumpPrevention = true;
                }
                else
                {
                    SonarMode.fistBumpPrevention = false;
                    return;
                }

                if (SonarMode.modEnabled == false || SonarMode.bodyMaterial == null)
                {
                    return;
                }

                if (SonarMode.playerInteractionTimestamps.ContainsKey(first))
                {
                    float previousTimestamp = SonarMode.playerInteractionTimestamps[first];
                    float currentTimestamp = (float)first.GetPreviousTime();

                    if (currentTimestamp - previousTimestamp <= 0.5f)
                    {
                        return;
                    }

                    SonarMode.playerInteractionTimestamps[first] = currentTimestamp;
                }
                else if (first.parentSystem.previousBoxTimestamp > 0.1f)
                {
                    SonarMode.playerInteractionTimestamps.Add(first, (float)first.GetPreviousTime());
                }
                else
                {
                    return;
                }

                if (SonarMode.playerInteractionTimestamps.ContainsKey(second))
                {
                    float previousTimestamp = SonarMode.playerInteractionTimestamps[second];
                    float currentTimestamp = second.parentSystem.previousBoxTimestamp;

                    if (currentTimestamp - previousTimestamp <= 0.5f)
                    {
                        return;
                    }

                    SonarMode.playerInteractionTimestamps[second] = currentTimestamp;
                }
                else if (second.parentSystem.previousBoxTimestamp > 0.1f)
                {
                    SonarMode.playerInteractionTimestamps.Add(second, second.parentSystem.previousBoxTimestamp);
                }
                else
                {
                    return;
                }


                var playerManager = PlayerManager.instance.LocalPlayer;
                Transform poolParent = PoolManager.Instance.GetPool("RockCube").poolParent;
                List<GameObject> overlappedObjects = new List<GameObject>();

                SonarMode.ActivateSonar(first.gameObject.transform.position, 5f, poolParent, overlappedObjects);
            }
        }
    }
}
