using Il2CppRUMBLE.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace BlindRumble
{
    internal class GameDetector
    {
        [HarmonyPatch(typeof(Il2CppRUMBLE.Environment.Matchmaking.MatchmakeConsole), "MatchmakeStatusUpdated", new Type[] { typeof(MatchmakingHandler.MatchmakeStatus), typeof(bool) })]
        public static class Patch1
        {
            private static void Prefix(GameObject __instance, MatchmakingHandler.MatchmakeStatus status, bool instantLeverStep)
            {

                if (status == MatchmakingHandler.MatchmakeStatus.Success)
                {
                    SonarMode.matchFound = true;
                }
            }
        }
    }
}