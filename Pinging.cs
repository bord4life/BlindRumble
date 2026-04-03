using HarmonyLib;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using UnityEngine;
using static RumbleModdingAPI.RMAPI.GameObjects.Gym.LOGIC;

namespace BlindRumble2
{
    [HarmonyPatch(typeof(Il2CppRUMBLE.MoveSystem.Stack), "Execute")]
    internal class Pinging
    {
        public static UnityEngine.Bounds bounds;
        // pings whenever structure spawns
        [HarmonyPostfix]
        public static void Postfix(Il2CppRUMBLE.MoveSystem.Stack __instance, Il2CppRUMBLE.MoveSystem.IProcessor processor)
        {
            // if not a player, skip
            Il2CppRUMBLE.MoveSystem.PlayerStackProcessor playerStackProcessor = null;
           

            try
            {
                playerStackProcessor = processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>();
            }
            catch (InvalidCastException)
            {
                return;
            }
            // if hold or flick, or mod is disabled, skip
            if (Core.modEnabled == false || __instance.cachedName == "HoldRight" || __instance.cachedName == "HoldLeft" || __instance.cachedName == "Flick")
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

                    Vector3 boxCenter = headsetTransform.position + headsetTransform.forward * 2.0f;
                    float boxRadius = 3.0f;
                    bounds = new UnityEngine.Bounds(boxCenter, Vector3.one * boxRadius);

                    List<Collider> overlappedObjects = new List<Collider>(Physics.OverlapBox(boxCenter, bounds.size / 2f));
                    break;
                }
            }

            var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is Core) as Core;
            var playerManager = PlayerManager.instance;

            if (modInstance != null)
            {
                GameObject controller = processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>().gameObject;

                if (controller == playerManager.localPlayer.Controller.gameObject)
                {
                    return;
                }


                GameObject visualsLocal = controller.transform.GetChild(1).gameObject;
                Transform headset = controller.transform.GetChild(2).GetChild(0).GetChild(0).transform;
                PlayerController playerController = controller.GetComponent<PlayerController>();

                // modInstance.RenderClone(visualsLocal.gameObject, poolParent, false);
                modInstance.CreatePlayerSnapshot(playerController);
            }
        }
    }
}
