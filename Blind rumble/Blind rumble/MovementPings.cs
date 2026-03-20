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
    internal class MovementPings
    {
        [HarmonyPatch(typeof(Il2CppRUMBLE.MoveSystem.Stack), "Execute")]
        private static class Stack_Execute_Patch
        {
            private static void Postfix(Il2CppRUMBLE.MoveSystem.Stack __instance, Il2CppRUMBLE.MoveSystem.IProcessor processor)
            {
                Il2CppRUMBLE.MoveSystem.PlayerStackProcessor playerStackProcessor = null;

                try
                {
                    playerStackProcessor = processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>();
                }
                catch (InvalidCastException)
                {
                    return;
                }

                if (playerStackProcessor == null)
                {
                    return;
                }


                if (Class1.modEnabled == false || __instance.cachedName == "HoldRight" || __instance.cachedName == "HoldLeft" || __instance.cachedName == "Flick")
                {
                    return;
                }

                try
                {
                    if (__instance.cachedName == "Jump" && Class1.midair == false && processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>() == PlayerManager.instance.localPlayer.Controller.GetSubsystem<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>())
                    {
                        Class1.midair = true;
                    }
                }
                catch
                {
                    return;
                }


                float checkRadius = 0.3f;
                Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

                Collider[] hitColliders = Physics.OverlapSphere(processor.Transform.position, checkRadius);

                foreach (Collider hitCollider in hitColliders)
                {
                    Transform visuals = hitCollider.transform.parent;
                    while (visuals != null && visuals.name != "Visuals")
                    {
                        visuals = visuals.parent;
                    }

                    if (visuals != null && visuals.name == "Visuals")
                    {
                        GameObject playerController = visuals.parent.gameObject;

                        Transform headsetTransform = playerController.transform.Find("VR/Headset Offset/Headset").transform;

                        if (headsetTransform == null)
                        {
                            return;
                        }

                        UnityEngine.Vector3 boxCenter = headsetTransform.position + headsetTransform.forward * 2.0f;

                        float boxRadius = 3.0f;

                        List<GameObject> overlappedObjects = Class1.PerformInitialOverlapCheck(boxCenter, boxRadius, headsetTransform.rotation, poolParent);

                        break;
                    }
                }

                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is Class1) as Class1;
                var playerManager = PlayerManager.instance;



                if (modInstance != null)
                {
                    GameObject controller = processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>().gameObject;

                    if (controller == playerManager.localPlayer.Controller.gameObject)
                    {
                        return;
                    }


                    GameObject visualsLocal = controller.transform.GetChild(0).gameObject;
                    Transform headset = controller.transform.GetChild(1).GetChild(0).GetChild(0).transform;

                    modInstance.RenderClone(visualsLocal.gameObject, poolParent, false);
                }

            }
        }
    }
}
