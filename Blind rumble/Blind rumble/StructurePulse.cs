using BlindRumble;
using Il2CppRUMBLE.Environment;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using RumbleModdingAPI;

namespace Blind_rumble
{
    internal class StructurePulse
    {
        [HarmonyPatch(typeof(Il2CppRUMBLE.MoveSystem.Structure), "Start")]
        public static class StructureSpawn
        {
            private static void Postfix(ref Il2CppRUMBLE.MoveSystem.Structure __instance)
            {
                if (Class1.modEnabled == false)
                {
                    return;
                }

                MeshRenderer structureMeshRenderer;
                try
                {
                    if (__instance.processableComponent.gameObject.GetComponent<Tetherball>() != null)
                    {
                        var testMaterial = Class1.newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;

                        if (Class1.modEnabled)
                        {
                            structureMeshRenderer = __instance.processableComponent.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>();
                            structureMeshRenderer.material = testMaterial;
                            structureMeshRenderer.enabled = false;
                        }

                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e);
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(Il2CppRUMBLE.MoveSystem.Structure), "OnFetchFromPool")]
        public static class StructureRespawn
        {
            private static void Postfix(ref Il2CppRUMBLE.MoveSystem.Structure __instance)
            {
                if (Class1.modEnabled == false)
                {
                    return;
                }

                string name = __instance.processableComponent.gameObject.name;
                MeshRenderer structureMeshRenderer;
                try
                {
                    if (__instance.processableComponent.gameObject.GetComponent<Tetherball>() != null)
                    {

                        name = "BoulderBall";
                        structureMeshRenderer = __instance.processableComponent.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>();
                    }
                    else if (__instance.processableComponent.gameObject.GetComponent<MeshRenderer>() != null)
                    {

                        structureMeshRenderer = __instance.processableComponent.gameObject.GetComponent<MeshRenderer>();
                    }
                    else
                    {

                        structureMeshRenderer = __instance.processableComponent.gameObject.transform.GetComponentInChildren<MeshRenderer>();
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e);
                    return;
                }

                var testMaterial = Class1.newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;

                GameObject controller = Managers.GetPlayerManager().localPlayer.Controller.gameObject;
                Transform poolParent = Pools.Structures.GetPoolCube().transform.parent;

                switch (name)
                {
                    case "LargeRock":
                    case "SmallRock":
                        structureMeshRenderer.enabled = false;
                        break;

                    case "BoulderBall":
                        structureMeshRenderer.enabled = false;
                        structureMeshRenderer.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        break;

                    case "Pillar":
                    case "Disc":
                    case "Wall":
                    case "RockCube":
                    case "Ball":
                        structureMeshRenderer.enabled = false;

                        List<GameObject> overlappedObjects = new List<GameObject>();
                        overlappedObjects.Add(__instance.processableComponent.gameObject);
                        overlappedObjects.Add(__instance.processableComponent.transform.parent.gameObject);

                        var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is Class1) as Class1;

                        modInstance.RenderClone(__instance.processableComponent.gameObject, poolParent, false);

                        for (int i = 0; i < 3; i++)
                        {
                            MelonCoroutines.Start(modInstance.SonarEnumerator(__instance.processableComponent.gameObject.transform, 3f, poolParent, overlappedObjects, false, i));
                        }


                        break;
                }
            }
        }
    }
}
