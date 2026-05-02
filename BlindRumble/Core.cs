using Il2CppOculus.Platform;
using Il2CppPhoton.Pun;
using Il2CppPhoton.Voice.Unity;
using Il2CppRootMotion.FinalIK;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Scaling;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
using System.Collections;
using UnityEngine;
using UIFramework;

[assembly: MelonInfo(typeof(BlindRumble2.Core), BlindRumble2.BuildInfo.ModName, BlindRumble2.BuildInfo.ModVersion, BlindRumble2.BuildInfo.Author)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonColor(255, 140, 40, 220)]
[assembly: MelonAuthorColor(255, 140, 40, 220)]
[assembly: VerifyLoaderVersion(0, 7, 2, true)]
[assembly: MelonAdditionalDependencies("UIFramework")]


namespace BlindRumble2
{
    public class Core : MelonMod
    {

        public Core Instance;
        public bool IsShaderFound = false;
        public static string CurrentSceneName = "Loader";
        public static GameObject newParent;
        public static Material sonarMaterial;
        public static bool modEnabled;
        public static bool EIGym = true; // EI = EnableIn
        public static bool EIPark;
        public static bool EIMatch;
        public static Color MainSonar;
        public static Color SecondarySonar;
        public static MelonLogger.Instance loggerInstance;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            CurrentSceneName = sceneName;

            if (CurrentSceneName is "Loader" && modEnabled == true)
            {
                GetSonarShader();
            }
            else if (modEnabled == true && IsShaderFound == true)
            {
                SonarifyScene();
            }
            else
            {
                return;
            }
        }

        public void GetSonarShader()
        {
            sonarMaterial = new Material(Shader.Find("Shader Graphs/Pose Ghost Shader"))
            {
                hideFlags = HideFlags.DontUnloadUnusedAsset,
                color = new Color(0, 0, 0, 0)
            };

            IsShaderFound = true;
        }

        public void SonarifyScene()
        {
            // Makes everything have sonar shader
            if (CurrentSceneName == "Gym") // sonars gym
            {
                if (EIGym == false || IsShaderFound == false)
                {
                    return;
                }
                else
                {
                    LoggerInstance.Msg("Trying to put on shaders for floor");
                    foreach (MeshRenderer rend in GameObjects.Gym.SCENE.GYM.GetGameObject().GetComponentsInChildren<MeshRenderer>(true))
                    {
                        rend.material = sonarMaterial;
                        rend.material.color = SecondarySonar;
                    }
                    LoggerInstance.Msg("Tried + Trying Gondola.Cabin");
                    GameObjects.Gym.INTERACTABLES.Gondola.Cabin.GetGameObject().GetComponent<MeshRenderer>().material = sonarMaterial;
                    LoggerInstance.Msg("Tried");
                }
            }

            if (CurrentSceneName == "Park") // sonars park
            {
                if (EIPark == false || IsShaderFound == false)
                {
                    return;
                }
                else
                {
                    foreach (Renderer rend in GameObjects.Park.SCENE.PARK.GetGameObject().GetComponentsInChildren<Renderer>(true))
                    {
                        rend.material = sonarMaterial;
                        rend.material.color = SecondarySonar;
                    }
                }

            }
            if (CurrentSceneName.Contains("Map"))
            {
                if (EIMatch == false || IsShaderFound == false)
                {
                    return;
                }
                else if (CurrentSceneName == "Map0") // sonars ring
                {
                    foreach (Renderer rend in GameObjects.Map0.Scene.Map0.GetGameObject().GetComponentsInChildren<Renderer>(true))
                    {
                        rend.material = sonarMaterial;
                        rend.material.color = SecondarySonar;
                    }
                }
                else if (CurrentSceneName == "Map1") // sonars pit
                {
                    foreach (Renderer rend in GameObjects.Map1.Scene.MAP1.GetGameObject().GetComponentsInChildren<Renderer>(true))
                    {
                        rend.material = sonarMaterial;
                        rend.material.color = SecondarySonar;
                    }
                }

            }
        }

        public static IEnumerator CreateSnapshot(PlayerController player)
        {

            //Creates a temporary image of where the player used to be when a sound happened nearby.
            GameObject cloneVisuals = GameObject.Instantiate(player.PlayerVisuals.gameObject);
            cloneVisuals.GetComponent<Animator>().enabled = false;
            cloneVisuals.GetComponent<PlayerAnimator>().enabled = false;
            cloneVisuals.GetComponent<RigDefinition>().enabled = false;
            cloneVisuals.GetComponent<PlayerVisuals>().enabled = false;
            cloneVisuals.GetComponent<PlayerAudioPresence>().enabled = false;
            cloneVisuals.GetComponent<PlayerHandPresence>().enabled = false;
            cloneVisuals.GetComponent<PlayerScaling>().enabled = false;
            cloneVisuals.GetComponent<PhotonAnimatorView>().enabled = false;
            cloneVisuals.GetComponent<PlayerIK>().enabled = false;
            cloneVisuals.GetComponent<VRIK>().enabled = false;
            cloneVisuals.GetComponent<PhotonView>().enabled = false;

            foreach (var renderer in cloneVisuals.GetComponentsInChildren<Renderer>())
            {
                renderer.material = sonarMaterial;
                renderer.material.color = MainSonar;
            }

            cloneVisuals.GetComponent<Animator>().enabled = true;
            cloneVisuals.GetComponent<PlayerAnimator>().enabled = true;
            cloneVisuals.GetComponent<RigDefinition>().enabled = true;
            cloneVisuals.GetComponent<PlayerVisuals>().enabled = true;
            cloneVisuals.GetComponent<PlayerAudioPresence>().enabled = true;
            cloneVisuals.GetComponent<PlayerHandPresence>().enabled = true;
            cloneVisuals.GetComponent<PlayerScaling>().enabled = true;
            cloneVisuals.GetComponent<PhotonAnimatorView>().enabled = true;
            cloneVisuals.GetComponent<PlayerIK>().enabled = true;
            cloneVisuals.GetComponent<VRIK>().enabled = true;
            cloneVisuals.GetComponent<PhotonView>().enabled = true;

            yield return new WaitForSeconds(1.5f);

            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 originalScale = cloneVisuals.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                cloneVisuals.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }

            GameObject.Destroy(cloneVisuals);

        }



        public override void OnInitializeMelon()
        {
            UISetup.LoadPrefs();
            UI.Register((MelonBase)this, UISetup.category1, UISetup.category2);
        }

        public override void OnLateInitializeMelon()
        {
            Instance = this;

            if (modEnabled == false)
            {
                EIGym = false;
                EIPark = false;
                EIMatch = false;
            }
        }
    }
}
