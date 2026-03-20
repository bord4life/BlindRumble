using HarmonyLib;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Managers;
using RumbleModdingAPI.RMAPI;
using System;
using System.Linq;
using UnityEngine;

namespace BlindRumble
{
    internal class ShifstoneFix
    {
        // Keeps shiftstones from breaking the blindness
        [HarmonyPatch(typeof(Il2CppRUMBLE.Combat.ShiftStones.PlayerShiftstoneSystem), "AttachShiftStone", new Type[] { typeof(ShiftStone), typeof(int), typeof(bool), typeof(bool) })]
        public static class ShiftstonePatch
        {
            private static void Postfix(GameObject __instance, ShiftStone stone, int slotIndex, bool saveInSettings, bool syncWithOtherPlayers)
            {
                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is SonarMode) as SonarMode;
                if (modInstance == null && SonarMode.currentSceneName != "Loader")
                {
                    return;
                }

                Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;
                GameObject newParent = GameObjects.DDOL.GameInstance.GetGameObject();

                var playerManager = PlayerManager.instance;
                if (playerManager != null && playerManager.AllPlayers != null)
                {

                    for (int index = 0; index < playerManager.AllPlayers.Count; index++)
                    {

                        var player = playerManager.AllPlayers[index];
                        if (player != null && player.Controller != null && player.Controller.name == "Player Controller(Clone)" && SonarMode.modEnabled)
                        {
                            GameObject controller = player.Controller.gameObject;

                            var stoneName = stone.gameObject.name;

                            if (newParent.transform.FindChild(stoneName))
                            {
                                var shiftstone = newParent.transform.FindChild(stoneName);

                                stone.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = shiftstone.GetChild(0).GetComponent<Renderer>().material;
                            }

                            modInstance.ChangePlayerMaterial(controller, player.Controller.gameObject == playerManager.localPlayer.Controller.gameObject);

                        }
                    }
                }
            }
        }
    }
}

