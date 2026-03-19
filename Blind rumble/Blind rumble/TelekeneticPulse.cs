using BlindRumble;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
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
    internal class TelekeneticPulse
    {
        // Makes hold and flick give off a pulse

        [HarmonyPatch(typeof(Il2CppRUMBLE.MoveSystem.FlickModifier), "Execute", new Type[] { typeof(Il2CppRUMBLE.MoveSystem.IProcessor), typeof(Il2CppRUMBLE.MoveSystem.StackConfiguration) })]
        public static class FlickStartEvent
        {
            private static void Postfix(Il2CppRUMBLE.MoveSystem.IProcessor processor, Il2CppRUMBLE.MoveSystem.StackConfiguration config)
            {
                if (Class1.modEnabled == false)
                {
                    return;
                }

                Transform poolParent = Pools.Structures.GetPoolCube().transform.parent;

                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is Class1) as Class1;
                if (modInstance != null)
                {

                    var localPlayer = Managers.GetPlayerManager().localPlayer;
                    bool isLocalPlayer = false;

                    if (localPlayer != null)
                    {
                        var playerPoseSystem = localPlayer.Controller.GetComponent<PlayerPoseSystem>();
                        if (playerPoseSystem != null)
                        {

                            if (processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>() == PlayerManager.instance.localPlayer.Controller.GetSubsystem<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>())
                            {
                                isLocalPlayer = true;
                            }
                        }
                    }

                    MelonCoroutines.Start(modInstance.HoldStart(poolParent, processor.Transform.position, 5f, isLocalPlayer, false));
                }
            }
        }

        public static class HoldStartEvent
        {
            private static void Postfix(Il2CppRUMBLE.MoveSystem.HoldModifier __instance, Il2CppRUMBLE.MoveSystem.IProcessor processor, Il2CppRUMBLE.MoveSystem.StackConfiguration config)// //Il2CppRUMBLE.MoveSystem.Stack __instance,
            {
                if (Class1.modEnabled == false)
                {
                    return;
                }

                Transform poolParent = Pools.Structures.GetPoolCube().transform.parent;

                var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is Class1) as Class1;
                if (modInstance != null)
                {

                    var localPlayer = Managers.GetPlayerManager().localPlayer;
                    bool isLocalPlayer = false;

                    if (localPlayer != null)
                    {
                        var playerPoseSystem = localPlayer.Controller.GetComponent<PlayerPoseSystem>();

                        if (playerPoseSystem != null)
                        {

                            if (processor.Cast<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>() == PlayerManager.instance.localPlayer.Controller.GetSubsystem<Il2CppRUMBLE.MoveSystem.PlayerStackProcessor>())
                            {
                                isLocalPlayer = true;
                            }
                        }
                    }

                    MelonCoroutines.Start(modInstance.HoldStart(poolParent, processor.Transform.position, 5f, isLocalPlayer, true));
                }
            }
        }
    }
}
