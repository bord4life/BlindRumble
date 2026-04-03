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


namespace BlindRumble2
{
    public class Core : MelonMod
    {

        public static Core Instance;
        public static string CurrentSceneName = "";
        public static GameObject newParent;
        public static Material sonarMaterial;
        public static bool modEnabled;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            sceneName = CurrentSceneName;

            if (sceneName == "Loader")
            {
                MelonCoroutines.Start(GetSonarShader());
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

            Material sonarMaterial = new Material(Shader.Find("Shader Graphs/Pose Ghost Shader"));
            sonarMaterial.color = new Color(0, 0, 0, 0);

            //armThingy.SetActive(false);
            //armThingy.name = "InvisibleObject";
            //armThingy.GetComponent<Renderer>().material = sonarMaterial;
            //armThingy.transform.parent = newParent.transform;
        }

        internal void Sonarify()
        {

        }

        internal void CreatePlayerSnapshot(PlayerController player)
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
                renderer.material = sonarMaterial;

        }

        public override void OnLateInitializeMelon()
        {
            Instance = this;
        }
    }
}
