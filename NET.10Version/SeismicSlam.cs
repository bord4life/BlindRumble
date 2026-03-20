using BlindRumble;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Il2CppRUMBLE.Managers;
using Il2CppRootMotion;


namespace BlindRumble
{
    internal class SeismicSlam
    {
        [HarmonyPatch(typeof(PlayerMovement), "OnBecameGrounded")]
        private static class PlayerMovement_OnBecameGrounded_Patch
        {
            private static void Postfix(PlayerMovement __instance)
            {
                RumbleModdingAPI.RMAPI.AudioManager.CreateAudioCall("C:\\Users\\johar\\source\\repos\\Blind rumble\\Blind rumble\\seismic_slam_buildup.wav", 50);
                
                if ((UnityEngine.Object)__instance != (UnityEngine.Object)Singleton<PlayerManager>.instance.localPlayer.Controller.GetSubsystem<PlayerMovement>())
                {
                    return;
                }

                if (!SonarMode.modEnabled || SonarMode.bodyMaterial == null || !SonarMode.midair)
                {
                    return;
                }

                SonarMode.midair = false;

                Vector3 playerPosition = PlayerManager.instance.LocalPlayer.Controller.transform.position;
                if (SonarMode.seismicSlam != null)
                {
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(SonarMode.seismicSlam, playerPosition);
                }

                var playerManager = PlayerManager.instance;
                Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;
                List<GameObject> overlappedObjects = new List<GameObject>();

                Transform newParent = GameObjects.DDOL.GameInstance.GetGameObject().transform;

                /* if (newParent.Find("HealthBarCamera") && newParent.Find("HealthBarCamera").gameObject.active)
                {
                    SonarMode.PlaySoundIfFileExists(@"\BlindRumble\seismic_slam_buildup_arcade.mp3");
                }
                else
                {
                    SonarMode.PlaySoundIfFileExists(@"\BlindRumble\seismic_slam_buildup.wav");
                 */

               
                SonarMode.ActivateSonar(PlayerManager.instance.LocalPlayer.Controller.transform.position, 1000f, poolParent, overlappedObjects);

                UnityEngine.Material glassMaterial1 = newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material;

                GameObject shockwaveSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(shockwaveSphere.GetComponent<Collider>());

                //FlipNormals(shockwaveSphere);

                shockwaveSphere.transform.localScale = Vector3.zero;
                shockwaveSphere.transform.position = playerManager.localPlayer.Controller.transform.GetChild(1).GetChild(0).GetChild(0).position + playerManager.localPlayer.Controller.transform.GetChild(1).GetChild(0).GetChild(0).TransformDirection(new Vector3(0, 0, 3f));

                Renderer sphereRenderer = shockwaveSphere.GetComponent<Renderer>();
                sphereRenderer.material = glassMaterial1;



                MelonCoroutines.Start(SonarMode.ScaleSphereOverTime(shockwaveSphere, new Vector3(50f, 50f, 50f), 2f));
            }
        }
    }
}
