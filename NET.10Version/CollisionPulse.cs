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
    internal class CollisionPulse
    {
        [HarmonyPatch(typeof(Il2CppRUMBLE.MoveSystem.Structure), "OnCollisionEnter", new Type[] { typeof(Collision) })]
        public static class StructureCollisionEvent
        {
            private static void Prefix(Collision collision)
            {
                if (SonarMode.modEnabled == false)
                {
                    return;
                }

                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is SonarMode) as SonarMode;

                GameObject controller = PlayerManager.instance.LocalPlayer.Controller.gameObject;
                Transform visuals = controller.transform.GetChild(0);
                Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

                List<GameObject> overlappedObjects = new List<GameObject>();

                List<GameObject> processedVisuals = new List<GameObject>();

                if (SonarMode.IsDescendantOf(collision.GetContact(0).thisCollider.gameObject.transform, poolParent, false, true, processedVisuals, false, true) && collision.contactCount == 1 || SonarMode.IsDescendantOf(collision.GetContact(0).otherCollider.gameObject.transform, poolParent, false, true, processedVisuals, false, true) && collision.contactCount == 1)
                {
                    modInstance.RenderClone(collision.GetContact(0).thisCollider.gameObject, poolParent, false, false, true, true);
                    return;
                }

                SonarMode.ActivateSonar(collision.GetContact(0).point, 5f, poolParent, overlappedObjects, true);


            }
        }
    }
}
