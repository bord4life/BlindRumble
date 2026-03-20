using BlindRumble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Il2CppRUMBLE.Managers;

namespace BlindRumble
{
    internal class SonarFeet
    {
        [HarmonyPatch(typeof(Il2CppRUMBLE.Players.Subsystems.PlayerAudioPresence), "OnFootStepAudio", new Type[] { typeof(Il2CppRUMBLE.Players.FootStepAudioInvoker.FootStepData) })]
        public static class FootstepAudioPlay
        {
            private static void Postfix(Il2CppRUMBLE.Players.Subsystems.PlayerAudioPresence __instance, Il2CppRUMBLE.Players.FootStepAudioInvoker.FootStepData data)
            {

                if (SonarMode.modEnabled == false || SonarMode.midair)
                {
                    return;
                }

                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is SonarMode) as SonarMode;



                Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;



                List<GameObject> overlappedObjects = new List<GameObject>();
                SonarMode.ActivateSonar(__instance.parentController.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).position, 5f, poolParent, overlappedObjects); //data.FootPosition
            }
        }
    }
}
