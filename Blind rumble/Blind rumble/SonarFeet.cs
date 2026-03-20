using BlindRumble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Il2CppRUMBLE.Managers;

namespace Blind_rumble
{
    internal class SonarFeet
    {
        [HarmonyPatch(typeof(Il2CppRUMBLE.Players.Subsystems.PlayerAudioPresence), "OnFootStepAudio", new Type[] { typeof(Il2CppRUMBLE.Players.FootStepAudioInvoker.FootStepData) })]
        public static class FootstepAudioPlay
        {
            private static void Postfix(Il2CppRUMBLE.Players.Subsystems.PlayerAudioPresence __instance, Il2CppRUMBLE.Players.FootStepAudioInvoker.FootStepData data)
            {

                if (Class1.modEnabled == false || Class1.midair)
                {
                    return;
                }

                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is Class1) as Class1;



                Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;



                List<GameObject> overlappedObjects = new List<GameObject>();
                Class1.ActivateSonar(__instance.parentController.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).position, 5f, poolParent, overlappedObjects); //data.FootPosition
            }
        }
    }
}
