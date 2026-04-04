using Il2CppOculus.Platform;
using Il2CppPhoton.Pun;
using Il2CppRootMotion.FinalIK;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Scaling;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(BlindRumble2.Core), BlindRumble2.BuildInfo.ModName, BlindRumble2.BuildInfo.ModVersion, BlindRumble2.BuildInfo.Author)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonColor(255, 255, 255, 255)]
[assembly: MelonAuthorColor(255, 255, 255, 255)]
[assembly: VerifyLoaderVersion(0, 7, 2, true)]
[assembly: MelonAdditionalDependencies("UIFramework")]


namespace BlindRumble2
{
    public partial class Core : MelonMod
    {

        public static Core Instance;
        public static string CurrentSceneName = "";
        public static GameObject newParent;
        public static Material sonarMaterial;
        public static bool modEnabled;
        public static bool EIGym; // EI = EnableIn
        public static bool EIPark;
        public static bool EIMatch;
        public static Color MainSonar;
        public static Color SecondarySonar;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            sceneName = CurrentSceneName;

            if (sceneName == "Loader" && modEnabled == true)
            {
                MelonCoroutines.Start(GetSonarShader());
            }
            else
            {
                Sonarify();
            }
        }

        public IEnumerator GetSonarShader()
        {
            while (!GameObject.Find("BootLoaderPlayer/Visuals/Left"))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            GameObject armThing = GameObject.Find("BootLoaderPlayer/Visuals/Left").gameObject;
            GameObject armThingy = GameObject.Instantiate(armThing).gameObject;

            newParent = GameObjects.DDOL.GameInstance.GetGameObject();

            while (!GameObject.Find("Shader Graphs/Pose Ghost Shader"))
            {
                yield return null;
            }

            Material sonarMaterial = new Material(Shader.Find("Shader Graphs/Pose Ghost Shader"));
            sonarMaterial.color = new Color(0, 0, 0, 0);

            //armThingy.SetActive(false);
            //armThingy.name = "InvisibleObject";
            //armThingy.GetComponent<Renderer>().material = sonarMaterial;
            //armThingy.transform.parent = newParent.transform;
        }

        internal void Sonarify()
        {
            // Makes everything have sonar shader
            if (CurrentSceneName == "Gym") // sonars gym
            {
                if (EIGym == false)
                {
                    return;
                }
                foreach (Renderer rend in GameObjects.Gym.SCENE.GYM.GetGameObject().GetComponentsInChildren<Renderer>(true))
                {
                    rend.material = sonarMaterial;
                    rend.material.color = SecondarySonar;
                }
            }

            if (CurrentSceneName == "Park") // sonars park
            {
                if (EIPark == false)
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
                if (EIMatch == false)
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

        public IEnumerator CreatePlayerSnapshot(PlayerController player)
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
                renderer.material.color = SecondarySonar;
            }

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

        public override void OnLateInitializeMelon()
        {
            Instance = this;
        }

        public static Color StringToColor(string colorString) // seperates the values into smth useable by color system
        {
            string[] parts = colorString.Split(',');
            float r = float.Parse(parts[0].Trim());
            float g = float.Parse(parts[1].Trim());
            float b = float.Parse(parts[2].Trim());
            float a = float.Parse(parts[3].Trim());
            return new Color(r, g, b, a);
        }
    }
}
