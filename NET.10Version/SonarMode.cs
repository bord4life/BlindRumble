using Il2CppRootMotion.FinalIK;
using Il2CppRUMBLE.Audio;
using Il2CppRUMBLE.Managers;
// using UnityEngine.SocialPlatforms;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Pools;
using Il2CppRUMBLE.Utilities;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using MelonLoader;
// using UnityEngine.InputSystem.XR;
using MelonLoader.Utils;
using RumbleModdingAPI.RMAPI;
using RumbleModUI;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.VFX;
using static RumbleModdingAPI.RMAPI.Calls;
using AudioManager = RumbleModdingAPI.RMAPI.AudioManager;
using BuildInfo = BlindRumble.Information.BuildInfo;

[assembly: MelonInfo(typeof(BlindRumble.SonarMode), BuildInfo.ModName, BuildInfo.ModVersion, BuildInfo.Author)]
[assembly: MelonGame(null, null)]

// CHECK LINES 1157-1180 + 1261 + 1236

namespace BlindRumble
{

    public class SonarMode : MelonMod
    {
        public static string currentSceneName = "";
        public static UnityEngine.Material bodyMaterial;
        public static UnityEngine.Material structureMaterial;
        public static UnityEngine.Material mapMaterial;
        public static Shader poseGhostShaderRef;
        public static GameObject newParent;
        public static bool onCollisionEvent = false;
        public static bool matchFound = false;
        public static bool friendQueue = false;
        public static bool modEnabled = false;
        public static bool fistBumpPrevention = false;
        private static bool structuresCopied = false;
        public static bool initialVFXDisabled = false;
        public static Il2CppRUMBLE.Players.Player enemyPlayer;
        public static GameObject enemyHealth;
        public static GameObject playerHealth;
        public static short enemyHealthAmount;
        public static AudioCall seismicSlam;




        public static bool modEnabledAllTime = true; // MAKE SURE TO TURN OFF ON RELEASE -------------------------------------------------------------------------------------------------------






        private static int playerAmount = 0;
        public static bool midair = false;
        public static float fadeProgress = 0;


        public static GameObject mainStructure;
        public static Dictionary<GameObject, Coroutine> activeRenderCoroutines = new Dictionary<GameObject, Coroutine>();
        public static Dictionary<GameObject, List<GameObject>> cloneGroups = new Dictionary<GameObject, List<GameObject>>();

        public static Dictionary<GameObject, bool> constantCloneGroups = new Dictionary<GameObject, bool>();
        public static Dictionary<GameObject, bool> explodeGroups = new Dictionary<GameObject, bool>();
        public static Dictionary<GameObject, float> cloneCooldowns = new Dictionary<GameObject, float>();
        public static Dictionary<Il2CppRUMBLE.Players.Player, float> playerLastTimestamps = new Dictionary<Il2CppRUMBLE.Players.Player, float>();
        public static Dictionary<PlayerBoxInteractionTrigger, float> playerInteractionTimestamps = new Dictionary<PlayerBoxInteractionTrigger, float>();



        Mod Mod = new Mod();
        private ModSetting<bool> modEnabledUI;
        private ModSetting<bool> filledInCharacterMaterial;
        private ModSetting<bool> filledInOtherCharacterMaterial;
        private ModSetting<bool> audioEnabled;
        private ModSetting<string> soundFilePath;


        public override void OnEarlyInitializeMelon()
        {
            base.OnEarlyInitializeMelon();

            // This makes sure the audio can be played, and puts a warning in the log if file isnt there
            SoundCheck((string)soundFilePath.SavedValue);
        }

        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();

            // Ya gotta do this here else the event will be done
            // before you add the listener x_x 
            // The UI will initialize itself 3 seconds after
            // the Loader scene has been loaded
            UI.instance.UI_Initialized += OnUIInit;
        }


        public void OnUIInit()
        {
            Mod.ModName = BuildInfo.ModName;
            Mod.ModVersion = BuildInfo.ModVersion;
            Mod.SetFolder("BlindRumble");
            Mod.AddDescription("Description", "", BuildInfo.Description, new Tags { IsSummary = true });

            modEnabledUI = Mod.AddToList("Is Mod Enabled", true, 0, "Requires exiting into the Gym to take effect", new Tags());
            filledInCharacterMaterial = Mod.AddToList("Is Local Player Model Material White", false, 0, "HEAVILY ADVISED TO TURN ON IF RECORDING WITH LIV. Changes the material on YOUR player model to fully white. Fixes smearing in LIV. Requires exiting into the Gym to take effect", new Tags());
            filledInOtherCharacterMaterial = Mod.AddToList("Is Enemy Player Model Material White", false, 0, "Changes the material on ENEMY player models to fully white. Requires exiting into the Gym to take effect", new Tags());
            audioEnabled = Mod.AddToList("Is Seismic Slam audio enabled?", true, 0, "Enables the Seismic Slam audio.", new Tags());
            soundFilePath = Mod.AddToList("Seismic Slam File Path", @"\BlindRumble\seismic_slam_buildup.wav", "Path to Seismic Slam audio. You can use custom audio, but that isn't tested.", new Tags());

            Mod.GetFromFile();

            UI.instance.AddMod(Mod);
        }


        public static void FlipNormals(GameObject obj)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null) return;

            Mesh mesh = meshFilter.mesh;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            mesh.normals = normals;

            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.triangles = triangles;
        }

        public static void SoundCheck(string soundFilePath)
        {

            if (System.IO.File.Exists(MelonEnvironment.UserDataDirectory + @soundFilePath))
            {

                seismicSlam = AudioManager.CreateAudioCall(@soundFilePath, 50);

            }
            else
            {

                MelonLogger.Warning("YOU ARE MISSING THE AUDIO FILE!!! AUDIO WILL NOT BE PLAYED!!!");

            }
        }


        public static void PlaySound(string soundFilePath)
        {
           
        }

        public override void OnLateUpdate()
        {


            if (enemyHealth != null && enemyPlayer != null && enemyHealthAmount != null && currentSceneName != "Gym" && currentSceneName != "Loader" && playerHealth != null)
            {

                if (enemyPlayer.Data.HealthPoints != enemyHealthAmount)
                {

                    enemyHealth.transform.GetComponent<PlayerHealth>().SetHealthBarPercentage(enemyPlayer.Data.HealthPoints, enemyHealthAmount, true);
                    enemyHealth.transform.GetComponent<PlayerHealth>().SetHealth(enemyPlayer.Data.HealthPoints, enemyHealthAmount, true);
                    enemyHealth.transform.GetComponent<PlayerHealth>().SetHealthBarPercentage(enemyPlayer.Data.HealthPoints, enemyHealthAmount, true);
                    try
                    {
                        // enemyHealth.transform.GetComponent<PlayerHealth>().UpdateLocalHealthBar();
                    }
                    catch
                    {
                    }

                    if (enemyPlayer.Data.HealthPoints < enemyHealthAmount)
                    {
                        Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

                        List<GameObject> overlappedObjects = new List<GameObject>();
                        SonarMode.ActivateSonar(enemyPlayer.Controller.transform.GetChild(1).GetChild(0).GetChild(0).position, 5f, poolParent, overlappedObjects, false);
                    }

                    enemyHealthAmount = enemyPlayer.Data.HealthPoints;
                }

                enemyHealth.transform.position = playerHealth.transform.position;
                enemyHealth.transform.localPosition = playerHealth.transform.localPosition;
                enemyHealth.transform.rotation = playerHealth.transform.rotation;

                enemyHealth.transform.GetChild(1).position = playerHealth.transform.GetChild(1).position;
                enemyHealth.transform.GetChild(1).localPosition = playerHealth.transform.GetChild(1).localPosition + new Vector3(0, 0.05f, 0);
                enemyHealth.transform.GetChild(1).rotation = playerHealth.transform.GetChild(1).rotation;
            }

            if (modEnabled)
            {
                var controller = PlayerManager.instance.LocalPlayer.Controller.gameObject;
                var fadeScreen = controller.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1);
                var fadeScreenInstance = fadeScreen.GetComponent<Il2CppRUMBLE.Players.SimpleScreenFadeInstance>();
                var progressField = fadeScreenInstance.GetType().GetProperty("Progress");

                if (progressField != null)
                {

                    float progress = (float)progressField.GetValue(fadeScreenInstance);

                    if (progress >= fadeProgress - 0.01f && progress <= fadeProgress + 0.01f && progress > 0)
                    {
                        progressField.SetValue(fadeScreenInstance, 0f);
                    }

                    fadeProgress = progress;
                }



                var playerManager1 = PlayerManager.instance;

                if (playerManager1 != null && playerManager1.AllPlayers != null)
                {
                    for (int index = 0; index < playerManager1.AllPlayers.Count; index++)
                    {
                        var player = playerManager1.AllPlayers[index];
                        if (player != null && player.Controller != null && player.Controller.name == "Player Controller(Clone)")
                        {
                            GameObject controller1 = player.Controller.gameObject;


                            if (controller1 == playerManager1.localPlayer.Controller.gameObject)
                            {
                                continue;
                            }

                            var boneChest = controller1.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(4).GetChild(0);

                            try
                            {
                                var visualEffect = boneChest.GetChild(3).GetComponent<VisualEffect>();
                                if (visualEffect != null)
                                {
                                    visualEffect.enabled = false;
                                }

                                var pooledVisualEffect = boneChest.GetChild(3).GetComponent<PooledVisualEffect>();
                                if (pooledVisualEffect != null)
                                {
                                    pooledVisualEffect.playOnFetchPool = false;
                                    pooledVisualEffect.enableOnFetchPool = false;
                                }
                            }
                            catch
                            { 
                            }

                            try
                            {
                                var visualEffect = boneChest.GetChild(4).GetComponent<VisualEffect>();
                                if (visualEffect != null)
                                {
                                    visualEffect.enabled = false;
                                }

                                var pooledVisualEffect = boneChest.GetChild(4).GetComponent<PooledVisualEffect>();
                                if (pooledVisualEffect != null)
                                {
                                    pooledVisualEffect.playOnFetchPool = false;
                                    pooledVisualEffect.enableOnFetchPool = false;
                                }
                            }
                            catch
                            {
                            }

                        }
                    }
                }

            }

            base.OnLateUpdate();
        }

        private IEnumerator DisableVFX(string currentScene, bool enabled)
        {
            if (currentScene == "Gym")
            {
                while (GameObject.Find("Hand_L_Poseghost") == null)
                {
                    yield return null;
                }
            }
            else
            {
                while (GameObjects.DDOL.GameInstance.GetGameObject() == null)
                {
                    yield return null;
                }
            }

                yield return new WaitForSeconds(2f);

            var poolsParent = PoolManager.instance.GetPool("RockCube").poolParent;
            PoolManager poolManager = poolsParent.GetComponent<PoolManager>();


            var poolSettingsArray = poolManager.resourcesToPool;

            int poolChildren = poolsParent.transform.childCount;
            for (int i = 0; i < poolChildren; ++i)
            {
   
                if (i == 22 || i == 23 || i == 24 || i == 25 || i == 35)
                {
                    continue;
                }

                var child = poolsParent.transform.GetChild(i).gameObject;

  
                if (child.name.Contains("VFX"))
                {
                    var visualEffect = child.GetComponent<VisualEffect>();
                    if (visualEffect != null)
                    {
                        visualEffect.enabled = enabled;
                    }

                    var pooledVisualEffect = child.GetComponent<PooledVisualEffect>();
                    if (pooledVisualEffect != null)
                    {
                        pooledVisualEffect.playOnFetchPool = enabled;
                        pooledVisualEffect.enableOnFetchPool = enabled;
                    }

                    if (i < poolSettingsArray.Length)
                    {
                        var poolSetting = poolSettingsArray[i];
                        if (poolSetting != null)
                        {
                            var resource = poolSetting.Resource;

                            if (resource != null)
                            {
                                var resourceVisualEffect = resource.GetComponent<VisualEffect>();
                                if (resourceVisualEffect != null)
                                {
                                    resourceVisualEffect.enabled = enabled;
                                }

                                var resourcePooledVisualEffect = resource.GetComponent<PooledVisualEffect>();
                                if (resourcePooledVisualEffect != null)
                                {
                                    resourcePooledVisualEffect.playOnFetchPool = enabled;
                                    resourcePooledVisualEffect.enableOnFetchPool = true;
                                }
                            }
                        }
                    }
                }

                int vfxChildren = child.transform.childCount;
                for (int g = 0; g < vfxChildren; ++g)
                {
                    var grandChild = child.transform.GetChild(g).gameObject;

                    var grandChildVisualEffect = grandChild.GetComponent<VisualEffect>();
                    if (grandChildVisualEffect != null)
                    {
                        grandChildVisualEffect.enabled = enabled;
                    }

                    var grandChildPooledVisualEffect = grandChild.GetComponent<PooledVisualEffect>();
                    if (grandChildPooledVisualEffect != null)
                    {
                        grandChildPooledVisualEffect.playOnFetchPool = enabled;
                        grandChildPooledVisualEffect.enableOnFetchPool = true;
                    }
                }

                if (child.name.Contains("VFX"))
                {
                    child.SetActive(enabled);
                }
            }


        }

        private void EnablePlayerCloneRendering(GameObject clone)
        {

            UnityEngine.Material finalMaterial = bodyMaterial;

            Transform visualsTransform = clone.transform;

            //if ((bool)((ModSetting)filledInOtherCharacterMaterial).SavedValue == true)
            //{

            //    Color customColor = new Color(0.8235f, 0.8f, 0.698f, 1f);
            //    Texture2D customTexture = new Texture2D(1, 1);

            //    customTexture.SetPixel(0, 0, customColor);
            //    customTexture.Apply();

            //    Texture2D whiteImage = customTexture;
            //    whiteMaterial = new UnityEngine.Material(Shader.Find("UI/Default"));
            //    whiteMaterial.mainTexture = whiteImage;

            //    finalMaterial = whiteMaterial;
            //}

            visualsTransform.GetChild(0).GetComponent<Renderer>().material = finalMaterial;
            visualsTransform.GetChild(0).GetComponent<Renderer>().enabled = true;

            visualsTransform.parent.GetChild(2).GetChild(0).GetComponent<Renderer>().enabled = false;
            visualsTransform.parent.GetChild(3).GetChild(0).GetComponent<Renderer>().enabled = false;

            visualsTransform.GetChild(1).GetChild(0).GetChild(2).GetChild(1).GetComponent<Renderer>().enabled = false;
            visualsTransform.GetChild(1).GetChild(0).GetChild(3).GetChild(1).GetComponent<Renderer>().enabled = false;

            Transform lowerArmLeft = visualsTransform.GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(1).GetChild(0).GetChild(0);
            Transform lowerArmRight = visualsTransform.GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(2).GetChild(0).GetChild(0);

            Transform? snapTransformLeft = lowerArmLeft.Find("SnapTransform");
            Transform? snapTransformRight = lowerArmRight.Find("SnapTransform");

            if (!snapTransformLeft)
            {
                snapTransformLeft = lowerArmLeft.GetChild(0).Find("Shiftsocket_L").GetChild(0);
            }

            if (!snapTransformRight)
            {
                snapTransformRight = lowerArmRight.GetChild(0).Find("Shiftsocket_R").GetChild(0);
            }

            if (snapTransformLeft.childCount > 0 && snapTransformLeft.GetChild(0).childCount > 0)
            {

                snapTransformLeft.GetChild(0).GetChild(0).GetComponent<Renderer>().material = finalMaterial;
            }

            if (snapTransformRight.childCount > 0 && snapTransformRight.GetChild(0).childCount > 0)
            {

                snapTransformRight.GetChild(0).GetChild(0).GetComponent<Renderer>().material = finalMaterial;
            }

        }

        public void ChangePlayerMaterial(GameObject modelTransform, bool IsLocalPlayer)
        {


            bodyMaterial = newParent.transform.Find("WhiteGhostPoseObject").gameObject.GetComponent<Renderer>().material;


            UnityEngine.Material finalMaterial = bodyMaterial;

            if (IsLocalPlayer)
            {
                modelTransform.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Camera>().nearClipPlane = 0.1f;
                modelTransform.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }




            if (!IsLocalPlayer)
            {

                finalMaterial = newParent.transform.Find("InvisibleObject").gameObject.GetComponent<Renderer>().material;
                modelTransform.transform.Find("NameTag").gameObject.SetActive(false);
                modelTransform.transform.Find("Park").gameObject.SetActive(false);
                modelTransform.transform.Find("Health").GetChild(0).gameObject.SetActive(false);

            }
            //else if ((bool)((ModSetting)filledInCharacterMaterial).SavedValue == true)
            //{
            //    Color customColor = new Color(0.8235f, 0.8f, 0.698f, 1f);
            //    Texture2D customTexture = new Texture2D(1, 1);

            //    customTexture.SetPixel(0, 0, customColor);
            //    customTexture.Apply();

            //    Texture2D whiteImage = customTexture;
            //    whiteMaterial = new UnityEngine.Material(Shader.Find("UI/Default"));
            //    whiteMaterial.mainTexture = whiteImage;

            //    finalMaterial = whiteMaterial;
            //}

            Transform visualsTransform = modelTransform.transform.GetChild(0);
            

            Transform lowerArmLeft = visualsTransform.GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(1).GetChild(0).GetChild(0);
            Transform lowerArmRight = visualsTransform.GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(2).GetChild(0).GetChild(0);

            Transform? snapTransformLeft = lowerArmLeft.Find("SnapTransform");
            Transform? snapTransformRight = lowerArmRight.Find("SnapTransform");

            Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

            visualsTransform.GetChild(0).GetComponent<Renderer>().material = finalMaterial;

            if (!snapTransformLeft)
            {
                snapTransformLeft = lowerArmLeft.GetChild(0).Find("Shiftsocket_L").GetChild(0);
            }

            if (!snapTransformRight)
            {
                snapTransformRight = lowerArmRight.GetChild(0).Find("Shiftsocket_R").GetChild(0);
            }

            if (snapTransformLeft.childCount > 0 && snapTransformLeft.GetChild(0).childCount > 0)
            {
                var stoneName = snapTransformLeft.GetChild(0).gameObject.name;

                if (!newParent.transform.FindChild(stoneName) && poolParent.FindChildRecursive(stoneName))
                {
                    var stoneClone = GameObject.Instantiate(poolParent.FindChildRecursive(stoneName).gameObject);
                    stoneClone.transform.parent = newParent.transform;
                    stoneClone.name = stoneName;
                    stoneClone.SetActive(false);
                }

                snapTransformLeft.GetChild(0).GetChild(0).GetComponent<Renderer>().material = finalMaterial;
            }

            if (snapTransformRight.childCount > 0 && snapTransformRight.GetChild(0).childCount > 0)
            {
                var stoneName = snapTransformRight.GetChild(0).gameObject.name;

                if (!newParent.transform.FindChild(stoneName) && poolParent.FindChildRecursive(stoneName))
                {
                    var stoneClone = GameObject.Instantiate(poolParent.FindChildRecursive(stoneName).gameObject);
                    stoneClone.transform.parent = newParent.transform;
                    stoneClone.name = stoneName;
                    stoneClone.SetActive(false);
                }

                snapTransformRight.GetChild(0).GetChild(0).GetComponent<Renderer>().material = finalMaterial;
            }
        }


        private void DisableRenderersWithSwitch(Transform parent, bool isGym)
        {
            if (parent == null || newParent == null)
            {
                return;
            }

            GameObject poolManagerObject = GameObjects.DDOL.GameInstance.PreInitializable.GetGameObject().transform.GetChild(1).gameObject;



            for (int i = 0; i < poolManagerObject.transform.childCount; i++)
            {
                Transform copiedStructure = poolManagerObject.transform.GetChild(i);

                if (copiedStructure.name.Contains("Shift"))
                {
                    continue;
                }


                for (int j = 0; j < copiedStructure.childCount; j++)
                {
                    Transform firstChild = copiedStructure.GetChild(j);



                    if (isGym)
                    {
                        Renderer targetRenderer = firstChild.GetComponent<Renderer>();
                        if (targetRenderer != null)
                        {
                            targetRenderer.enabled = true;
                        }

                    }
                    else
                    {
                        DisableRenderers(firstChild);
                    }

                    for (int k = 0; k < firstChild.childCount; k++)
                    {
                        Transform grandChild = firstChild.GetChild(k);

                        if (isGym)
                        {
                            Renderer targetRenderer = grandChild.GetComponent<Renderer>();
                            if (targetRenderer != null)
                            {
                                targetRenderer.enabled = true;
                            }
                        }
                        else
                        {
                            DisableRenderers(grandChild);
                        }
                    }
                }
            }
        }

        private void CopyStructures(Transform parent, Transform newParent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                if (IsStructure(child.name))
                {
                    GameObject copiedStructure = GameObject.Instantiate(child.gameObject, newParent);
                    copiedStructure.name = child.name;
                }
            }
        }


        private bool IsStructure(string name)
        {
            return name.Contains("BoulderBall") || name.Contains("SmallRock") || name.Contains("LargeRock") ||
                   name.Contains("Pillar") || name.Contains("Disc") || name.Contains("Wall") ||
                   name.Contains("RockCube") || name.Contains("Ball");
        }

        private void ApplyMaterialFromCopy(Transform target, Transform newParent)
        {

            if (target == null || newParent == null) return;


            Transform copiedStructure = newParent.Find(target.name);

            if (copiedStructure != null)
            {


                for (int i = 0; i < target.childCount; i++)
                {

                    Transform targetChild = target.GetChild(i);
                    Transform copiedChild = copiedStructure.childCount > i ? copiedStructure.GetChild(i) : null;


                    if (targetChild != null && copiedChild != null)
                    {
                        Renderer targetChildRenderer = targetChild.GetComponent<Renderer>();
                        Renderer copiedChildRenderer = copiedChild.GetComponent<Renderer>();

                        if (targetChildRenderer != null && copiedChildRenderer != null)
                        {

                            targetChildRenderer.material = copiedChildRenderer.material;
                            targetChildRenderer.enabled = true;
                        }


                        for (int g = 0; g < targetChild.childCount; g++)
                        {

                            Transform targetGrandchild = targetChild.GetChild(g);
                            Transform copiedGrandchild = copiedChild.childCount > g ? copiedChild.GetChild(g) : null;

                            if (targetGrandchild != null && copiedGrandchild != null)
                            {
                                Renderer targetGrandchildRenderer = targetGrandchild.GetComponent<Renderer>();
                                Renderer copiedGrandchildRenderer = copiedGrandchild.GetComponent<Renderer>();

                                if (targetGrandchildRenderer != null && copiedGrandchildRenderer != null)
                                {

                                    targetGrandchildRenderer.material = copiedGrandchildRenderer.material;
                                    targetGrandchildRenderer.enabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }


        private void DisableRenderers(Transform target)
        {
            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer != null && !target.Find("ExplodeStatus_VFX"))
            {
                targetRenderer.enabled = false;
            }
        }

        private IEnumerator DelayCharacterTransformation(string currentScene, bool IsLocalPlayer = true)
        {
            if (currentScene != "Loader" && currentScene != "")
            {
                if (currentScene == "Gym")
                {
                    while (GameObject.Find("Hand_L_Poseghost") == null)
                    {
                        yield return null;
                    }
                }
                else
                {

                    while (GameObject.Find("Player Controller(Clone)") == null)
                    {
                        yield return null;
                    }

                }
                yield return new WaitForSeconds(3f);






                if (GameObject.Find("MatchInfoMod") && currentScene.Contains("Map"))
                {
                    Renderer[] renderers3 = GameObject.Find("MatchInfoMod").transform.GetComponentsInChildren<Renderer>(true);

                    foreach (var rend in renderers3)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            if (rend.gameObject.GetComponent<TextMeshPro>() != null)
                            {
                                materials[g].renderQueue = 5000;

                                var textMesh = rend.gameObject.GetComponent<TextMeshPro>();

                                rend.gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                                continue;
                            }

                        }

                        rend.materials = materials;


                    }
                }

                if (currentScene == "Gym")
                {

                    Transform visualsTransform = PlayerManager.instance.LocalPlayer.Controller.transform.GetChild(0);


                    Transform lowerArmLeft = visualsTransform.GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(1).GetChild(0).GetChild(0);
                    Transform lowerArmRight = visualsTransform.GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(2).GetChild(0).GetChild(0);

                    Transform? snapTransformLeft = lowerArmLeft.Find("SnapTransform");
                    Transform? snapTransformRight = lowerArmRight.Find("SnapTransform");

                    Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

                    if (!snapTransformLeft)
                    {
                        snapTransformLeft = lowerArmLeft.GetChild(0).Find("Shiftsocket_L").GetChild(0);
                    }

                    if (!snapTransformRight)
                    {
                        snapTransformRight = lowerArmRight.GetChild(0).Find("Shiftsocket_R").GetChild(0);
                    }

                    if (snapTransformLeft.childCount > 0 && snapTransformLeft.GetChild(0).childCount > 0)
                    {
                        var stoneName = snapTransformLeft.GetChild(0).gameObject.name;

                        if (!newParent.transform.FindChild(stoneName) && poolParent.FindChildRecursive(stoneName))
                        {
                            var stoneClone = GameObject.Instantiate(poolParent.FindChildRecursive(stoneName).gameObject);
                            stoneClone.transform.parent = newParent.transform;
                            stoneClone.name = stoneName;
                            stoneClone.SetActive(false);
                        }

                        var shiftstone = newParent.transform.FindChild(stoneName);

                        snapTransformLeft.GetChild(0).GetChild(0).GetComponent<Renderer>().material = shiftstone.GetChild(0).GetComponent<Renderer>().material;
                    }

                    if (snapTransformRight.childCount > 0 && snapTransformRight.GetChild(0).childCount > 0)
                    {
                        var stoneName = snapTransformRight.GetChild(0).gameObject.name;

                        if (!newParent.transform.FindChild(stoneName) && poolParent.FindChildRecursive(stoneName))
                        {
                            var stoneClone = GameObject.Instantiate(poolParent.FindChildRecursive(stoneName).gameObject);
                            stoneClone.transform.parent = newParent.transform;
                            stoneClone.name = stoneName;
                            stoneClone.SetActive(false);
                        }

                        var shiftstone = newParent.transform.FindChild(stoneName);

                        snapTransformRight.GetChild(0).GetChild(0).GetComponent<Renderer>().material = shiftstone.GetChild(0).GetComponent<Renderer>().material;
                    }

                }

                if ((bool)((ModSetting)modEnabledUI).SavedValue && IsLocalPlayer)
                {

                    if (currentScene == "Park" || (friendQueue && currentScene.Contains("Map")) || (currentScene.Contains("Map") && Mods.doesOpponentHaveMod("BlindRumble", BuildInfo.ModVersion, false)) || (currentScene.Contains("Map") && modEnabledAllTime))
                    {

                        Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;
                        DisableRenderersWithSwitch(poolParent, false);

                        modEnabled = true;

                        newParent.transform.Find("InvertedSphereClone").gameObject.SetActive(true);

                        MelonCoroutines.Start(DisableVFX(currentScene, false));
                        RenderSettings.fog = false;


                        if (currentScene != "Park")
                        {
                            var playerManager1 = PlayerManager.instance;

                            if (playerManager1 != null && playerManager1.AllPlayers != null)
                            {
                                for (int index = 0; index < playerManager1.AllPlayers.Count; index++)
                                {
                                    var player = playerManager1.AllPlayers[index];
                                    if (player != null && player.Controller != null && player.Controller.name == "Player Controller(Clone)")
                                    {
                                        GameObject controller = player.Controller.gameObject;


                                        if (controller == playerManager1.localPlayer.Controller.gameObject)
                                        {
                                            continue;
                                        }

                                        enemyHealthAmount = player.Data.HealthPoints;




                                        playerHealth = GameObject.Find("/Health").gameObject;

                                        GameObject dummyPlayer = GameObject.Instantiate(PlayerManager.instance.playerControllerPrefab.gameObject);
   
                                        dummyPlayer.name = "FakePlayer";
                                        dummyPlayer.transform.GetChild(0).gameObject.SetActive(false);
                                        dummyPlayer.transform.GetChild(1).gameObject.SetActive(false);
      
                                        dummyPlayer.transform.GetChild(3).gameObject.SetActive(false);
                                        dummyPlayer.transform.GetChild(4).gameObject.SetActive(false);
                                        dummyPlayer.transform.GetChild(5).gameObject.SetActive(false);

                                        dummyPlayer.transform.GetChild(6).gameObject.SetActive(false);

                                        dummyPlayer.transform.GetChild(7).gameObject.SetActive(false);
                                        dummyPlayer.transform.GetChild(8).gameObject.SetActive(false);
                                        dummyPlayer.transform.GetChild(9).gameObject.SetActive(false);
                                        dummyPlayer.transform.GetChild(10).gameObject.SetActive(false);

                                        dummyPlayer.transform.Find("Health").gameObject.SetActive(true);

                                        enemyHealth = dummyPlayer.transform.Find("Health").gameObject;

                                        GameObject.Destroy(dummyPlayer.transform.Find("LIV").gameObject);


                                        enemyHealth.transform.GetComponent<PlayerHealth>().Initialize(player.Controller.Cast<Il2CppRUMBLE.Players.PlayerController>());

                                        enemyHealth.transform.GetComponent<PlayerHealth>().SetHealthBarPercentage(enemyHealthAmount, 20, true);
                                        enemyHealth.transform.GetComponent<PlayerHealth>().SetHealth(enemyHealthAmount, 20, true);
                                        enemyHealth.transform.GetComponent<PlayerHealth>().SetHealthBarPercentage(enemyHealthAmount, 20, true);
                                        try
                                        {
                                            enemyHealth.transform.GetComponent<PlayerHealth>().SetHealthBarPercentage(enemyHealthAmount, 20, true);
                                        }
                                        catch
                                        {
                                        }

                                        enemyPlayer = player;



                                        break;
                                    }
                                }
                            }
                        }
                    }
                }



                if (currentScene == "Gym")
                {

                    GameObject armMesh = GameObjects.Gym.TUTORIAL.Worldtutorials.ToolTips.TooltipMatchmaker.ToolTipHand.Hand.GetGameObject();
                    GameObject regionEurope = GameObjects.Gym.INTERACTABLES.RegionSelector.GetGameObject().transform.Find("Model/WorldMap/Regions_Effectmesh/Region_Europe.002").gameObject;
                    
                    GameObject glassPane = GameObjects.Gym.INTERACTABLES.Shiftstones.ShiftstoneCabinet.Glassplane.GetGameObject();

                    if (armMesh != null && regionEurope != null && !newParent.transform.Find("WhiteGhostPoseObject"))
                    {
                        GameObject newParent = GameObjects.DDOL.GameInstance.GetGameObject();
                        if (newParent != null)
                        {

                            GameObject armMeshClone = GameObject.Instantiate(armMesh).gameObject;
                            armMeshClone.SetActive(false);
                            armMeshClone.name = "WhiteGhostPoseObject";
                            armMeshClone.transform.parent = newParent.transform;

                            GameObject regionGlow = GameObject.Instantiate(regionEurope).gameObject;
                            regionGlow.SetActive(false);
                            regionGlow.name = "YellowRegionGlowObject";
                            regionGlow.transform.parent = newParent.transform;

                            GameObject totemDomeGlass = GameObject.Instantiate(glassPane).gameObject;
                            totemDomeGlass.SetActive(false);
                            totemDomeGlass.name = "GlassObject";
                            totemDomeGlass.transform.parent = newParent.transform;



                            totemDomeGlass.GetComponent<Renderer>().material = new Material(armMeshClone.GetComponent<Renderer>().material);

                            Material material = totemDomeGlass.GetComponent<Renderer>().material;
                            Shader shader = totemDomeGlass.GetComponent<Renderer>().material.shader;
                            for (int i = 0; i < shader.GetPropertyCount(); i++)
                            {

                                if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
                                {
                                    string propertyName = shader.GetPropertyName(i);
                                    material.SetColor(propertyName, new Color(0.21f, 0.7f, 0.81f, 1f));
                                }
                            }


                            Material material1 = armMeshClone.GetComponent<Renderer>().material;
                            Shader shader1 = armMeshClone.GetComponent<Renderer>().material.shader;
                            for (int i = 0; i < shader1.GetPropertyCount(); i++)
                            {
                                if (shader1.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
                                {
                                    string propertyName = shader1.GetPropertyName(i);
                                    material1.SetColor(propertyName, new Color(0.8f, 0.8f, 0.8f, 1));
                                }
                            }

                            GameObject poolManagerObject = GameObjects.DDOL.GameInstance.PreInitializable.GetGameObject().transform.GetChild(1).gameObject;

                        }
                        else
                        {
                            MelonLogger.Msg("Failed to find new parent GameObject");
                        }


                    }

                    newParent.transform.Find("InvertedSphereClone").gameObject.SetActive(false);
                }
                else if (currentScene == "Park" && modEnabled)
                {

                    // Disables all visuals in Park
                    GameObjects.Park.LIGHTING.GetGameObject().SetActive(false);

                    GameObjects.Park.SCENE.ParkVIsta.GetGameObject().SetActive(false);
                    GameObjects.Park.SCENE.GYMWater.GetGameObject().SetActive(false);
                    GameObjects.Park.SCENE.PARK.GetGameObject().SetActive(false);
                    GameObjects.Park.SCENE.PARKMos.GetGameObject().SetActive(false);
                    
                    GameObjects.Park.INTERACTABLES.Fruit.GetGameObject().SetActive(false);
                    GameObjects.Park.INTERACTABLES.Toys.GetGameObject().SetActive(false);
                    GameObjects.Park.INTERACTABLES.Shiftstones.GetGameObject().SetActive(false);
                    GameObjects.Park.INTERACTABLES.Gondola.GetGameObject().SetActive(false);
                    GameObjects.Park.INTERACTABLES.Telephone20REDUXspecialedition.GetGameObject().SetActive(false);
                    GameObjects.Park.INTERACTABLES.ParkboardPark.GetGameObject().SetActive(false);

                    mapMaterial = new UnityEngine.Material(Shader.Find(newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material.shader.name));
                    mapMaterial.renderQueue = 3000;

                    UnityEngine.Material transparentMaterial = newParent.transform.Find("InvisibleObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material yellowGhostPoseMaterial = newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material regionMaterial = newParent.transform.Find("YellowRegionGlowObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material glassMaterial = newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material;

                    UnityEngine.Material glassObjectMaterial = new UnityEngine.Material(Shader.Find(newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material.shader.name));
                    glassObjectMaterial.renderQueue = 3000;

                    Renderer[] renderers = GameObjects.Park.SCENE.PARK.GetGameObject().transform.parent.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            if (rend.gameObject.name.Contains("Arena") && g == 0)
                            {
                                materials[g] = transparentMaterial;
                                continue;
                            }
                            materials[g] = mapMaterial;
                        }

                        rend.materials = materials;
                    }

                    Renderer[] renderers1 = GameObjects.Park.LOGIC.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers1)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            if (rend.gameObject.GetComponent<TextMeshPro>() != null)
                            {
                                materials[g].renderQueue = 5000;

                                var textMesh = rend.gameObject.GetComponent<TextMeshPro>();

                                rend.gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                                continue;
                            }


                            if (rend.gameObject.GetComponent<Image>() != null)
                            {
                                rend.gameObject.GetComponent<Image>().material.renderQueue = 5000;
                                continue;
                            }

                            if (rend.gameObject.transform.parent.gameObject.name == "ShiftstoneQuickswapper" || rend.gameObject.transform.parent.gameObject.name == "FloatingButton" || rend.gameObject.transform.parent.gameObject.name == "InteractionButton Toggle Variant" || rend.gameObject.transform.parent.parent.gameObject.name == "InteractionButton Toggle Variant")
                            {
                                materials[g] = glassMaterial;
                            }
                            else
                            {
                                materials[g] = yellowGhostPoseMaterial;
                            }

                            if (rend.gameObject.transform.parent.gameObject.name == "Thetherball" || rend.gameObject.transform.parent.parent.gameObject.name == "Thetherball" || rend.gameObject.name == "Thetherball")
                            {
                                materials[g] = yellowGhostPoseMaterial;
                                rend.enabled = true;
                            }

                        }

                        rend.materials = materials;


                    }


                    Renderer[] renderers2 = GameObjects.Park.LOGIC.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers2)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            if (rend.gameObject.GetComponent<TextMeshPro>() != null)
                            {
                                materials[g].renderQueue = 5000;

                                var textMesh = rend.gameObject.GetComponent<TextMeshPro>();

                                rend.gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                                continue;
                            }


                            if (rend.gameObject.GetComponent<Image>() != null)
                            {
                                rend.gameObject.GetComponent<Image>().material.renderQueue = 5000;
                                continue;
                            }

                            materials[g] = yellowGhostPoseMaterial;

                        }

                        rend.materials = materials;


                    }


                    GameObjects.Park.INTERACTABLES.Shiftstones.ShiftstoneQuickswapper.LeftHandSlab.GetGameObject().transform.GetComponent<Renderer>().material = regionMaterial;
                    GameObjects.Park.INTERACTABLES.Shiftstones.ShiftstoneQuickswapper.RighthandSlab.GetGameObject().transform.GetComponent<Renderer>().material = regionMaterial;

                    GameObjects.Park.INTERACTABLES.Toys.MatchCounter.Scoreboard.FirstPlayerNameUI.Text.GetGameObject().transform.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    GameObjects.Park.INTERACTABLES.Toys.MatchCounter.Scoreboard.SecondPlayerNameUI.Text.GetGameObject().transform.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    GameObjects.Park.INTERACTABLES.Toys.MatchCounter.Scoreboard.FirstPlayerScoreUI.Text.GetGameObject().transform.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    GameObjects.Park.INTERACTABLES.Toys.MatchCounter.Scoreboard.SecondPlayerScoreUI.Text.GetGameObject().transform.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Park.INTERACTABLES.Toys.MatchCounter.Scoreboard.RestScoreUI.Text.GetGameObject().transform.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else if (currentScene == "Map0" && GameObjects.Map0.Scene.Map0Vista.GetGameObject().active == true && modEnabled)
                {

                    GameObjects.Map0.LightingEffects.GetGameObject().SetActive(false);
                    GameObjects.Map0.Scene.Map0Vista.GetGameObject().SetActive(false);
                    GameObjects.Map0.Logic.Pedestals.GetGameObject().SetActive(false);
                    GameObjects.Map0.Scene.RINGBanner.GetGameObject().SetActive(false);
                    GameObjects.Map0.Scene.Map0Leaves.GetGameObject().SetActive(false);
                    GameObjects.Map0.Scene.GYMWater.GetGameObject().SetActive(false);
                    GameObjects.Map0.Scene.Station.GetGameObject().SetActive(false);
                    GameObjects.Map0.Scene.Map0.GetGameObject().SetActive(false);

                    GameObjects.Map0.Scene.GetGameObject().transform.GetChild(0).GetChild(2).GetChild(1).gameObject.GetComponent<Renderer>().enabled = false;

                    mapMaterial = new UnityEngine.Material(Shader.Find(newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material.shader.name));
                    mapMaterial.renderQueue = 3000;

                    UnityEngine.Material transparentMaterial = newParent.transform.Find("InvisibleObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material yellowGhostPoseMaterial = newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material regionMaterial = newParent.transform.Find("YellowRegionGlowObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material glassMaterial = newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material;

                    UnityEngine.Material glassObjectMaterial = new UnityEngine.Material(Shader.Find(newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material.shader.name));
                    glassObjectMaterial.renderQueue = 3000;



                    Renderer[] renderers = GameObjects.Map0.Scene.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            if (rend.gameObject.name == "Gutter")
                            {
                                materials[g] = mapMaterial;
                            }
                            else
                            {
                                materials[g] = mapMaterial;
                            }
                        }

                        rend.materials = materials;
                    }



                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalLift.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalRelocate.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalSink.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalSpawn.GetGameObject().GetComponent<VisualEffect>().enabled = false;

                    Renderer[] renderers1 = GameObjects.Map0.Logic.Pedestals.MatchpedestalP1.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers1)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = yellowGhostPoseMaterial;
                        }

                        rend.materials = materials;
                    }

                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalLift.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalRelocate.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalSink.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map0.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalSpawn.GetGameObject().GetComponent<VisualEffect>().enabled = false;

                    Renderer[] renderers2 = GameObjects.Map0.Logic.Pedestals.MatchpedestalP2.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers2)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = yellowGhostPoseMaterial;
                        }

                        rend.materials = materials;
                    }

                    //MATCH SLAB ONE
                    //GRAPHICS SLAB
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock11.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    //MIGHT BE WRONG^^^^---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    var materials1 = GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().materials;

                    materials1[0] = yellowGhostPoseMaterial;
                    materials1[1] = yellowGhostPoseMaterial;

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().materials = materials1;

                    //REPLAY BUTTON
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    //RE-QUEUE BUTTON
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);


                    //EXIT MATCH BUTTON
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);



                    //SHIFTSTONE SWAPPER
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(1).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.LeftHandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;
                    GameObjects.Map0.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.RighthandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;






                    //MATCH SLAB TWO
                    //GRAPHICS SLAB
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    //MIGHT BE WRONG^^^^---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    var materials2 = GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().GetComponent<Renderer>().materials;

                    materials2[0] = yellowGhostPoseMaterial;
                    materials2[1] = yellowGhostPoseMaterial;

                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().materials = materials2;

                    //REPLAY BUTTON
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Replaytext.GetGameObject().GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    //RE-QUEUE BUTTON
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.Requeuetext.GetGameObject().GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);


                    //EXIT MATCH BUTTON
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.GetGameObject().GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);



                    //SHIFTSTONE SWAPPER
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(1).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.LeftHandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;
                    GameObjects.Map0.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.RighthandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;





                }
                else if (currentScene == "Map1" && GameObjects.Map1.Scene.MAP1.GetGameObject().active == true && modEnabled)
                {
                    GameObjects.Map1.LightingEffects.GetGameObject().SetActive(false);


                    /* GameObjects.Map1.Map1Production.MainStaticGroup.CombatFloor.GetGameObject().SetActive(false);
                    GameObjects.Map1.Map1Production.MainStaticGroup.DeathDirt.GetGameObject().SetActive(false);
                    GameObjects.Map1.Map1Production.MainStaticGroup.Leaves.GetGameObject().SetActive(false);
                    GameObjects.Map1.Map1Production.MainStaticGroup.OuterBoundry.GetGameObject().SetActive(false); */

                    GameObjects.Map1.Scene.MAP1.GetGameObject().SetActive(false);
                    GameObjects.Map1.Scene.MAP1.GetGameObject().SetActive(false);

                    mapMaterial = new UnityEngine.Material(Shader.Find(newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material.shader.name));
                    mapMaterial.renderQueue = 3000;

                    UnityEngine.Material transparentMaterial = newParent.transform.Find("InvisibleObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material yellowGhostPoseMaterial = newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material regionMaterial = newParent.transform.Find("YellowRegionGlowObject").gameObject.GetComponent<Renderer>().material;
                    UnityEngine.Material glassMaterial = newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material;

                    UnityEngine.Material glassObjectMaterial = new UnityEngine.Material(Shader.Find(newParent.transform.Find("GlassObject").gameObject.GetComponent<Renderer>().material.shader.name));
                    glassObjectMaterial.renderQueue = 3000;



                    Renderer[] renderers = GameObjects.Map1.Scene.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = mapMaterial;
                        }

                        rend.materials = materials;
                    }



                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalLift.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalRelocate.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalSink.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP1.VFX.DustPedestalSpawn.GetGameObject().GetComponent<VisualEffect>().enabled = false;

                    Renderer[] renderers1 = GameObjects.Map1.Logic.Pedestals.MatchpedestalP1.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers1)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = yellowGhostPoseMaterial;
                        }

                        rend.materials = materials;
                    }

                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalLift.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalRelocate.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalSink.GetGameObject().GetComponent<VisualEffect>().enabled = false;
                    GameObjects.Map1.Logic.Pedestals.MatchpedestalP2.VFX.DustPedestalSpawn.GetGameObject().GetComponent<VisualEffect>().enabled = false;

                    Renderer[] renderers2 = GameObjects.Map1.Logic.Pedestals.MatchpedestalP2.GetGameObject().transform.GetComponentsInChildren<Renderer>();

                    foreach (var rend in renderers2)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = yellowGhostPoseMaterial;
                        }

                        rend.materials = materials;
                    }

                    //MATCH SLAB ONE
                    //GRAPHICS SLAB
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    //MIGHT BE WRONG^^^^---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    var materials1 = GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().materials;

                    materials1[0] = yellowGhostPoseMaterial;
                    materials1[1] = yellowGhostPoseMaterial;

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().materials = materials1;

                    //REPLAY BUTTON
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    //RE-QUEUE BUTTON
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);


                    //EXIT MATCH BUTTON
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);



                    //SHIFTSTONE SWAPPER
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(1).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.LeftHandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;
                    GameObjects.Map1.Logic.MatchSlabOne.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.RighthandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;






                    //MATCH SLAB TWO
                    //GRAPHICS SLAB
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    //MIGHT BE WRONG^^^^---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.Floatrock1.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    var materials2 = GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().GetComponent<Renderer>().materials;

                    materials2[0] = yellowGhostPoseMaterial;
                    materials2[1] = yellowGhostPoseMaterial;

                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.GraphicsSlab.Mesh.MeshGraphicsslab.GetGameObject().GetComponent<Renderer>().materials = materials2;

                    //REPLAY BUTTON
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Replaytext.GetGameObject().GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightlocal.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;

                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Light.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Replaybutton.Interactionlightnetworked.Lightplug.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    //RE-QUEUE BUTTON
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Requeuebutton.Requeuetext.GetGameObject().GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);


                    //EXIT MATCH BUTTON
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.Slabrock1float.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.InteractionButton.Button.Spring.GetGameObject().GetComponent<Renderer>().material = yellowGhostPoseMaterial;


                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.Exitmatchbutton.GetGameObject().GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);



                    //SHIFTSTONE SWAPPER
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(1).gameObject.GetComponent<TextMeshPro>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.FloatingButton.GetGameObject().transform.GetChild(2).GetChild(0).GetChild(2).gameObject.GetComponent<Renderer>().material = glassMaterial;

                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.LeftHandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;
                    GameObjects.Map1.Logic.MatchSlabTwo.MatchSlab.Slabbuddymatchvariant.MatchForm.ShiftstoneQuickswapper.RighthandSlab.GetGameObject().gameObject.GetComponent<Renderer>().material = regionMaterial;

                }


                var playerManager = PlayerManager.instance;
                if (playerManager != null && playerManager.AllPlayers != null)
                {

                    for (int index = 0; index < playerManager.AllPlayers.Count; index++)
                    {

                        var player = playerManager.AllPlayers[index];
                        if (player != null && player.Controller != null && player.Controller.name == "Player Controller(Clone)" && modEnabled)
                        {

                            GameObject controller = player.Controller.gameObject;

                            ChangePlayerMaterial(controller, player.Controller.gameObject == playerManager.localPlayer.Controller.gameObject);

                        }
                    }
                }

            }
        }

        private IEnumerator YellowGhostPoseCloningCoroutine()
        {
            while (!GameObject.Find("BootLoaderPlayer/Visuals/Left"))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            GameObject armMesh = GameObject.Find("BootLoaderPlayer/Visuals/Left").gameObject;
            GameObject invertedSphere = GameObject.Find("________________SCENE_________________/Text/Iverted sphere").gameObject;
            newParent = GameObjects.DDOL.GameInstance.GetGameObject();

            GameObject armMeshClone = GameObject.Instantiate(armMesh).gameObject;
            GameObject armMeshClone1 = GameObject.Instantiate(armMesh).gameObject;
            GameObject invertedSphereClone = GameObject.Instantiate(invertedSphere).gameObject;

            Material invisibleMaterial = new Material(Shader.Find("Shader Graphs/RUMBLE_Transperent"));
            invisibleMaterial.color = new Color(0, 0, 0, 0);

            armMeshClone.SetActive(false);
            armMeshClone.name = "YellowGhostPoseObject";
            armMeshClone.transform.parent = newParent.transform;

            armMeshClone1.SetActive(false);
            armMeshClone1.name = "InvisibleObject";
            armMeshClone1.GetComponent<Renderer>().material = invisibleMaterial;
            armMeshClone1.transform.parent = newParent.transform;

            invertedSphereClone.transform.localScale = new UnityEngine.Vector3(1000, 1000, 1000);
            invertedSphereClone.SetActive(false);
            invertedSphereClone.name = "InvertedSphereClone";
            invertedSphereClone.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 1);
            invertedSphereClone.transform.parent = newParent.transform;
        }



        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

            currentSceneName = sceneName;

            if (sceneName == "Loader")
            {
                MelonCoroutines.Start(YellowGhostPoseCloningCoroutine());
            }
            else
            {
                if (sceneName == "Gym")
                {
                    Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

                    playerInteractionTimestamps.Clear();
                    friendQueue = false;
                    modEnabled = false;
                    initialVFXDisabled = false;

                    DisableRenderersWithSwitch(poolParent, true);
                    MelonCoroutines.Start(DisableVFX(sceneName, true));
                    MelonCoroutines.Start(DelayCharacterTransformation(sceneName));
                    RenderSettings.fog = true;

                }
                else
                {
                    MelonCoroutines.Start(DelayCharacterTransformation(sceneName));
                }





            }
        }

        public static List<GameObject> PerformInitialOverlapCheck(UnityEngine.Vector3 startPosition, float boxSize, Quaternion rotation, Transform poolParent)
        {


            List<GameObject> processedVisuals = new List<GameObject>();

            var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is SonarMode) as SonarMode;


            Collider[] hitColliders = Physics.OverlapBox(startPosition, new UnityEngine.Vector3(boxSize, boxSize, boxSize) / 2, rotation);

            List<GameObject> overlappedObjects = new List<GameObject>();

            foreach (var hitCollider in hitColliders)
            {
                var poolParentResult = IsDescendantOf(hitCollider.transform, poolParent, true, false);


                if (poolParentResult != null || IsDescendantOf(hitCollider.transform, poolParent, false, true, processedVisuals))
                {

                    if (poolParentResult != null && poolParentResult.gameObject.name == "BoulderBall")
                    {

                        if (processedVisuals.Contains(poolParentResult.gameObject))
                        {
                            continue;
                        }

                        if (!processedVisuals.Contains(poolParentResult.gameObject))
                        {

                            processedVisuals.Add(poolParentResult.gameObject);
                        }

                        overlappedObjects.Add(poolParentResult.gameObject);
                    }
                    else
                    {
                        overlappedObjects.Add(hitCollider.gameObject);
                    }



                    if (modInstance != null)
                    {
                        if (poolParentResult != null && poolParentResult.gameObject.name == "BoulderBall")
                        {
                            modInstance.RenderClone(poolParentResult.gameObject, poolParent);
                        }
                        else
                        {
                            modInstance.RenderClone(hitCollider.gameObject, poolParent);
                        }
                    }

                }
            }

            return overlappedObjects;
        }



        public IEnumerator SonarEnumerator(Transform structureTransform, float targetSize, Transform poolParent, List<GameObject> initialOverlaps, bool collidedWithMap = false, int delay = 0)
        {

            if (delay == 0)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(delay / 10);
            }

            UnityEngine.Vector3 startPosition = structureTransform.position;

            List<GameObject> processedVisuals = new List<GameObject>();

            ActivateSonar(startPosition, targetSize, poolParent, initialOverlaps, collidedWithMap, processedVisuals);
        }





        public static void ActivateSonar(UnityEngine.Vector3 startPosition, float targetSize, Transform poolParent, List<GameObject> initialOverlaps, bool collidedWithMap = false, List<GameObject> processedVisuals = null)
        {
            if (modEnabled == false)
            {
                return;
            }

            if (processedVisuals == null)
            {
                processedVisuals = new List<GameObject>();
            }

            var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is SonarMode) as SonarMode;
            Transform visuals;

            GameObject poolManagerObject = GameObjects.DDOL.GameInstance.PreInitializable.GetGameObject().transform.GetChild(1).gameObject;

            Collider[] poolColliders = poolManagerObject.GetComponentsInChildren<Collider>();

            foreach (var poolCollider in poolColliders)
            {
                if (UnityEngine.Vector3.Distance(poolCollider.gameObject.transform.position, startPosition) > targetSize / 2)// / 2
                {
                    continue;
                }

                if (!initialOverlaps.Contains(poolCollider.gameObject))
                {

                    visuals = IsDescendantOf(poolCollider.transform, poolParent, false, true, processedVisuals, true);
                    var currentParent = IsDescendantOf(poolCollider.transform, poolParent, true, false);

                    if (currentParent != null || visuals != null)
                    {
                        if (visuals && processedVisuals.Contains(visuals.gameObject))
                        {
                            continue;
                        }

                        if (visuals && !processedVisuals.Contains(visuals.gameObject))
                        {
                            processedVisuals.Add(visuals.gameObject);
                        }

                        if (poolCollider && processedVisuals.Contains(poolCollider.gameObject))
                        {
                            continue;
                        }

                        if (poolCollider && !processedVisuals.Contains(poolCollider.gameObject))
                        {
                            processedVisuals.Add(poolCollider.gameObject);
                        }

                        if (currentParent != null && currentParent.parent != null && currentParent.parent.gameObject.name == "BoulderBall" && processedVisuals.Contains(currentParent.parent.gameObject))
                        {
                            continue;
                        }

                        if (currentParent != null && currentParent.parent != null && currentParent.parent.gameObject.name == "BoulderBall" && !processedVisuals.Contains(currentParent.parent.gameObject))
                        {
                            processedVisuals.Add(currentParent.gameObject);
                        }

                        if (modInstance != null)
                        {
                            if (currentParent != null && currentParent.parent != null && currentParent.parent.gameObject.name == "BoulderBall")
                            {
                                modInstance.RenderClone(currentParent.parent.gameObject, poolParent, collidedWithMap);
                            }
                            else if (currentParent != null)
                            {

                                modInstance.RenderClone(poolCollider.gameObject, poolParent, collidedWithMap);
                            }
                        }
                    }
                }
            }

            var playerManager = PlayerManager.instance;

            if (playerManager != null && playerManager.AllPlayers != null)
            {
                for (int index = 0; index < playerManager.AllPlayers.Count; index++)
                {
                    var player = playerManager.AllPlayers[index];
                    if (player != null && player.Controller != null && player.Controller.name == "Player Controller(Clone)" && modEnabled)
                    {
                        GameObject controller = player.Controller.gameObject;


                        GameObject visualsLocal = controller.transform.GetChild(0).gameObject;
                        Transform headset = controller.transform.GetChild(1).GetChild(0).GetChild(0).transform;

                        if (processedVisuals.Contains(visualsLocal.gameObject) || UnityEngine.Vector3.Distance(headset.position, startPosition) > targetSize)
                        {
                            continue;
                        }



                        processedVisuals.Add(visualsLocal.gameObject);

                        modInstance.RenderClone(visualsLocal.gameObject, poolParent, collidedWithMap);
                    }
                }
            }
        }






        public IEnumerator CheckForExplode(GameObject original, Transform poolParent)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (original == null || !original.active)
                {
                    yield break;
                }

                var isDescendant = IsDescendantOf(original.transform, poolParent, true, false);

                if (isDescendant && original.transform.parent.Find("ExplodeStatus_VFX") || isDescendant && original.transform.Find("ExplodeStatus_VFX"))
                {
                    if (explodeGroups.ContainsKey(original) || original.transform.parent.Find("Hold_VFX") || original.transform.parent.Find("Flick_VFX") || original.transform.Find("Hold_VFX") || original.transform.Find("Flick_VFX") || constantCloneGroups.ContainsKey(original))
                    {
                        yield break;
                    }

                    explodeGroups[original] = true;
                    RenderConstantClone(original, poolParent, true, original.transform, true);
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }



        public void RenderClone(GameObject original, Transform poolParent, bool collidedWithMap = false, bool ignoreRendererEnabled = false, bool addToCloneList = true, bool playerCollisionClone = false)
        {
            if (modEnabled == false)
            {
                return;
            }

            List<GameObject> processedVisuals = new List<GameObject>();

            Transform visuals = IsDescendantOf(original.transform, poolParent, false, true, processedVisuals, true);

            GameObject localController = PlayerManager.instance.localPlayer.Controller.gameObject;
            GameObject localVisuals = localController.transform.GetChild(0).gameObject;
            float cooldownTime = 0.5f;

            if (visuals)
            {
                original = visuals.transform.parent.gameObject;
            }

            if (original == localController)
            {
                return;
            }


            if (!visuals && !original.transform.parent.name.Contains("Structure") || visuals || original.transform.name == "SmallRock" || original.transform.name == "LargeRock" || original.transform.name == "BoulderBall")
            {

                MelonCoroutines.Start(CheckForExplode(original, poolParent));

                if (IsDescendantOf(original.transform, poolParent, true, false) && original.GetComponent<Renderer>().enabled == true && ignoreRendererEnabled == false)
                {
                    return;
                }

               

                if (cloneCooldowns.ContainsKey(original) && cloneCooldowns[original] > 0f && original.transform.name != "LargeRock" || constantCloneGroups.ContainsKey(original) && !playerCollisionClone)
                {
                    return;
                }

                GameObject clone = GameObject.Instantiate(original);

                if (visuals)
                {
                    clone.GetComponent<PlayerController>().assignedPlayer = null;
                }

                clone.name = "ObjectGhostClone";

                UnityEngine.Vector3 oldSize;

                if (visuals)
                {

                    GameObject cloneVisuals;
                    GameObject playerVisuals;
                    if (original == localController)
                    {
                        cloneVisuals = clone.transform.GetChild(0).gameObject;
                        playerVisuals = original.transform.GetChild(0).gameObject;
                    }
                    else
                    {
                        cloneVisuals = clone.transform.GetChild(0).gameObject;
                        playerVisuals = original.transform.GetChild(0).gameObject;
                    }

                    Transform VR = clone.transform.GetChild(1).transform;
                    Transform leftController = VR.GetChild(1).transform;
                    Transform rightController = VR.GetChild(2).transform;
                    Transform pillBody = clone.transform.GetChild(4).GetChild(0).transform;
                    Transform headset = VR.GetChild(0).GetChild(0).transform;

                    Transform VROriginal = original.transform.GetChild(1).transform;
                    Transform leftControllerOriginal = VROriginal.GetChild(1).transform;
                    Transform rightControllerOriginal = VROriginal.GetChild(2).transform;
                    Transform pillBodyOriginal = original.transform.GetChild(4).GetChild(0).transform;
                    Transform headsetOriginal = VROriginal.GetChild(0).GetChild(0).transform;



                    cloneVisuals.GetComponent<VRIK>().enabled = true;

                    leftController.gameObject.GetComponent<TrackedPoseDriver>().enabled = false;
                    rightController.gameObject.GetComponent<TrackedPoseDriver>().enabled = false;


                    pillBody.position = pillBodyOriginal.position;
                    headset.position = headsetOriginal.position;
                    leftController.position = leftControllerOriginal.position;
                    rightController.position = rightControllerOriginal.position;

                    pillBody.rotation = pillBodyOriginal.rotation;
                    headset.rotation = headsetOriginal.rotation;
                    leftController.rotation = leftControllerOriginal.rotation;
                    rightController.rotation = rightControllerOriginal.rotation;


                    oldSize = playerVisuals.transform.localScale;
                }
                else
                {

                    oldSize = original.transform.localScale;

                    clone.transform.localScale = UnityEngine.Vector3.zero;


                    if (playerCollisionClone)
                    {

                        clone.SetActive(false);
                    }
                }


                Rigidbody[] rigidbodies = clone.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.isKinematic = true;
                }

                Renderer cloneRenderer = clone.GetComponent<Renderer>();
                if (cloneRenderer != null)
                {
                    cloneRenderer.enabled = true;
                }

                Renderer[] cloneRenderers = clone.GetComponentsInChildren<Renderer>();
                foreach (var rend in cloneRenderers)
                {
                    rend.enabled = true;
                }
                Collider cloneCollider = clone.GetComponent<Collider>();
                MeshCollider meshCloneCollider = clone.GetComponent<MeshCollider>();

                Collider[] cloneColliders = clone.GetComponentsInChildren<Collider>();
                MeshCollider[] meshCloneColliders = clone.GetComponentsInChildren<MeshCollider>();

                foreach (var rend in cloneColliders)
                {
                    rend.enabled = false;
                }

                foreach (var rend in meshCloneColliders)
                {
                    rend.enabled = false;
                }

                if (cloneCollider != null)
                {
                    cloneCollider.enabled = false;
                }
                if (meshCloneCollider != null)
                {
                    meshCloneCollider.enabled = false;
                }

                float delay;
                float timeToDestroy;

                if (addToCloneList == true)
                {
                    if (!cloneGroups.ContainsKey(original))
                    {
                        cloneGroups[original] = new List<GameObject>();
                    }

                    if (collidedWithMap == true)
                    {
                        cooldownTime = 0.1f;
                    }

                    cloneGroups[original].Add(clone);
                    cloneCooldowns[original] = cooldownTime;

                    delay = 0;
                    timeToDestroy = 1f;
                }
                else
                {
                    if (!cloneGroups.ContainsKey(original))
                    {
                        cloneGroups[original] = new List<GameObject>();
                    }

                    delay = (cloneGroups[original].Count - 1) * 0.1f;
                    timeToDestroy = 1f + (cloneGroups[original].Count - 1) * 0.1f;
                }



                if (IsDescendantOf(original.transform, poolParent, true, false))
                {

                    Renderer[] renderers = clone.GetComponentsInChildren<Renderer>();
                    var yellowGhostMaterial = newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;

                    foreach (var rend in renderers)
                    {
                        var materials = rend.materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = yellowGhostMaterial;
                        }
                        rend.materials = materials;
                    }


                    if (original.transform.name == "BoulderBall")
                    {
                        var materials = clone.GetComponent<Renderer>().materials;

                        for (var g = 0; g < materials.Length; g++)
                        {
                            materials[g] = yellowGhostMaterial;
                        }
                        clone.GetComponent<Renderer>().materials = materials;
                    }

                    if (original.transform.name == "BoulderBall")
                    {
                        UnityEngine.Vector3 oldSize1 = original.transform.GetChild(0).transform.localScale;

                        clone.transform.GetChild(0).transform.position = original.transform.GetChild(0).transform.position;
                        clone.transform.GetChild(0).transform.rotation = original.transform.GetChild(0).transform.rotation;
                        clone.transform.GetChild(0).transform.localScale = UnityEngine.Vector3.zero;

                        MelonCoroutines.Start(FollowOriginalWithDelay(clone, original, delay, 0.5f, collidedWithMap, false, playerCollisionClone));
                        MelonCoroutines.Start(FollowOriginalWithDelay(clone.transform.GetChild(0).gameObject, original.transform.GetChild(0).gameObject, delay, 0.5f, collidedWithMap, false, playerCollisionClone));





                        MelonCoroutines.Start(ScaleClone(clone.transform.GetChild(0).gameObject, oldSize1, 0.05f));
                    }
                    else
                    {

                        MelonCoroutines.Start(FollowOriginalWithDelay(clone, original, delay, 0.5f, collidedWithMap, false, playerCollisionClone));
                    }

                }

                if (visuals)
                {

                    GameObject cloneVisuals;
                    if (original == localController)
                    {
                        cloneVisuals = clone.transform.GetChild(0).gameObject;
                    }
                    else
                    {
                        cloneVisuals = clone.transform.GetChild(0).gameObject;
                    }


                    clone.transform.Find("PlayerOnPlayerColliders").gameObject.SetActive(false);
                    clone.transform.Find("Hitboxes").gameObject.SetActive(false);
                    clone.transform.Find("Park").gameObject.SetActive(false);
                    clone.transform.Find("NameTag").gameObject.SetActive(false);

                    clone.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(false);
                    GameObject.Destroy(clone.transform.Find("LIV").gameObject);

                    EnablePlayerCloneRendering(cloneVisuals);

                    MelonCoroutines.Start(DestroyCloneAfterDelay(clone, 0.5f, original, cloneVisuals));
                }
                else
                {
                    MelonCoroutines.Start(ScaleClone(clone, oldSize, 0.05f));

                    MelonCoroutines.Start(DestroyCloneAfterDelay(clone, 1f, original));
                }



            }
        }


        public static IEnumerator ScaleSphereOverTime(GameObject sphere, Vector3 targetScale, float duration)
        {
            Vector3 initialScale = sphere.transform.localScale;
            float elapsedTime = 0f;
            

            while (elapsedTime < duration)
            {
                sphere.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            sphere.transform.localScale = targetScale;

            GameObject.Destroy(sphere, 0f);
        }

        private IEnumerator ScaleClone(GameObject clone, UnityEngine.Vector3 targetScale, float duration)
        {
            if (clone == null)
            {
                yield break;
            }

            UnityEngine.Vector3 initialScale = clone.transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if (clone == null)
                {
                    yield break;
                }

                clone.transform.localScale = UnityEngine.Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (clone == null)
            {
                yield break;
            }

            clone.transform.localScale = targetScale;
        }

        private IEnumerator FollowOriginalWithDelay(GameObject clone, GameObject original, float delay, float followTime, bool collidedWithMap = false, bool NeedsHoldOrFlick = false, bool playerCollisionClone = false)
        {
            Queue<UnityEngine.Vector3> positionHistory = new Queue<UnityEngine.Vector3>();
            Queue<Quaternion> rotationHistory = new Queue<Quaternion>();

            float elapsedTime = 0f;
            int framesPassed = 0;
            bool destroyPositionSet = false;
            bool holdOrFlickReleased = false;
            UnityEngine.Vector3 destroyPosition = Vector3.zero;

            float minDelay = (float)Math.Max(delay, 0.1);

            while (followTime < 0 || elapsedTime < followTime + delay)
            {
 
                if (original != null && original.active)
                {
                    positionHistory.Enqueue(original.transform.position);
                    rotationHistory.Enqueue(original.transform.rotation);
                }


                if (original != null && original.active && positionHistory.Count > minDelay / Time.deltaTime)
                {
                    positionHistory.Dequeue();
                    rotationHistory.Dequeue();
                }


                if (original != null && clone != null && original.active && elapsedTime >= delay && positionHistory.Count > 0 && delay > 0)
                {
                    clone.transform.position = positionHistory.Peek();
                    clone.transform.rotation = rotationHistory.Peek();
                }
                else if(original != null && clone != null && original.active && delay == 0)
                {
                    clone.transform.position = original.transform.position;
                    clone.transform.rotation = original.transform.rotation;
                }


                if ((original == null || !original.active) && clone != null && collidedWithMap == true || (original == null || !original.active) && clone != null && playerCollisionClone == true)
                {

                    if (!destroyPositionSet)
                    {
                        destroyPosition = clone.transform.position;
                        destroyPositionSet = true;

                        if (playerCollisionClone)
                        {
                            Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;
                            List<GameObject> overlappedObjects = new List<GameObject>();
                            if (original != null)
                            {
                                overlappedObjects.Add(original.gameObject);

                            }

                            if (original != null && original.transform.parent.gameObject != null)
                            {
                                overlappedObjects.Add(original.transform.parent.gameObject);

                            }

                            clone.SetActive(true);
                            SonarMode.ActivateSonar(clone.transform.position, 5f, poolParent, overlappedObjects, false);
                        }
                    }

                    float randomRange = 0.05f;
                    clone.transform.position = destroyPosition + new UnityEngine.Vector3(
                        UnityEngine.Random.Range(-randomRange, randomRange),
                        UnityEngine.Random.Range(-randomRange, randomRange),
                        UnityEngine.Random.Range(-randomRange, randomRange)
                    );

                    
                }


                if (followTime < 0 && (original == null || !original.active) || followTime < 0 && NeedsHoldOrFlick && !explodeGroups.ContainsKey(original) && !original.transform.parent.Find("ExplodeStatus_VFX") && !original.transform.Find("ExplodeStatus_VFX") && !original.transform.parent.Find("Hold_VFX") && !original.transform.parent.Find("Flick_VFX") && !original.transform.Find("Hold_VFX") && !original.transform.Find("Flick_VFX"))
                {

                    followTime = 0.5f;
                    elapsedTime = 0;
                    holdOrFlickReleased = true;

                }

                elapsedTime += Time.deltaTime;
                framesPassed += 1;

                yield return null;
            }

            if (holdOrFlickReleased)
            {

                if (explodeGroups.ContainsKey(original))
                {
                    explodeGroups.Remove(original);
                }

                if (constantCloneGroups.ContainsKey(original))
                {
                    constantCloneGroups.Remove(original);
                }

                DisableVFX(currentSceneName, false);

                MelonCoroutines.Start(DestroyCloneAfterDelay(clone, 0f, original));
            }


            if ((original == null || !original.active) && followTime > 0)
            {

                if (clone != null)
                {

                   

                    if (cloneGroups.ContainsKey(original))
                    {
                        cloneGroups[original].Remove(clone);
                        if (cloneGroups[original].Count == 0)
                        {
                            cloneGroups.Remove(original);
                        }
                    }

                    if (explodeGroups.ContainsKey(original))
                    {
                        explodeGroups.Remove(original);
                    }

                    if (constantCloneGroups.ContainsKey(original))
                    {
                        constantCloneGroups.Remove(original);
                    }

                    GameObject.Destroy(clone);
                }
            }
        }

        


        private IEnumerator DestroyCloneAfterDelay(GameObject clone, float delay, GameObject original, GameObject cloneVisuals = null)
        {
            float elapsedTime = 0f;

            while (clone != null)
            {
                if (cloneVisuals == null)
                {
                    var holdVFX = clone.transform.Find("Hold_VFX");
                    var flickVFX = clone.transform.Find("Flick_VFX");

                    if (holdVFX != null || flickVFX != null)
                    {
                        yield return ScaleClone(clone, UnityEngine.Vector3.zero, 0.05f);
                        break;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(delay);

                    yield return ScaleClone(cloneVisuals, UnityEngine.Vector3.zero, 0.05f);
                    break;
                }


                elapsedTime += Time.deltaTime;
                if (elapsedTime >= delay)
                {
                    yield return ScaleClone(clone, UnityEngine.Vector3.zero, 0.05f);
                    break;
                }

                yield return null;
            }


            if (clone != null)
            {


                if (cloneGroups.ContainsKey(original))
                {
                    cloneGroups[original].Remove(clone);
                    if (cloneGroups[original].Count == 0)
                    {
                        cloneGroups.Remove(original);
                    }
                }

                if (clone.transform.Find("Visuals") && clone.transform.Find("Visuals").Find("Renderer"))
                {
                    GameObject.Destroy(clone.transform.GetChild(0).GetChild(0).gameObject);

                    GameObject.Destroy(clone.transform.GetChild(0).gameObject);
                }
                

                GameObject.Destroy(clone);
            }
        }

        private IEnumerator RenderObjectForDuration(GameObject obj, float duration)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }

            yield return new WaitForSeconds(duration);

            if (renderer != null)
            {
                renderer.enabled = false;
            }

            activeRenderCoroutines.Remove(obj);
        }

        public static Transform IsDescendantOf(Transform child, Transform poolParent, bool checkingForStructure, bool checkingForVisuals, List<GameObject> processedVisuals = null, bool notInstafillProcessedVisuals = false, bool collisionCheck = false)
        {
            Transform current = child;
            bool isStructure = false;

            while (current != null)
            {

                
                if (checkingForStructure == true && current.parent != null && current.parent.name.Contains("Structure"))
                {
                    isStructure = true;
                }
                
                if (checkingForStructure == true && current == poolParent && isStructure == true || checkingForStructure == true && current.name == "BoulderBall")
                {
                    return current;
                }

                if (checkingForVisuals == true && current != null && current.name == "Player Controller(Clone)" && processedVisuals != null || checkingForVisuals == true && collisionCheck == true && current != null && current.name == "Player Controller(Clone)" && processedVisuals != null)
                {

                    current = current.GetChild(0);

                    if (!notInstafillProcessedVisuals && !processedVisuals.Contains(current.gameObject))
                    {
                        processedVisuals.Add(current.gameObject);
                    }


                    return current;
                }



                current = current.parent;
            }

            return null;
        }

        private GameObject FindClosestDescendant(List<GameObject> gos, Transform poolParent, UnityEngine.Vector3 pos)
        {
            GameObject closest = null;
            float minDistance = float.MaxValue;

            foreach (var go in gos)
            {
                if (IsDescendantOf(go.transform, poolParent, true, false))
                {
                    float distance = UnityEngine.Vector3.Distance(go.transform.position, pos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = go;
                    }
                }
            }

            return closest;
        }





        private void CleanupCooldowns()
        {
            var keys = cloneCooldowns.Keys.ToList();

            foreach (var key in keys)
            {
                cloneCooldowns[key] -= Time.fixedDeltaTime;

                if (cloneCooldowns[key] <= 0f)
                {
                    cloneCooldowns.Remove(key);
                }
            }
        }
        public override void OnFixedUpdate()
        {

            if (matchFound)
            {

                int stepIndex = GameObjects.Gym.INTERACTABLES.MatchConsole.GetGameObject().transform.GetChild(8).gameObject.GetComponent<Il2CppRUMBLE.Interactions.InteractionBase.InteractionSlider>().snappedStep;
                if (stepIndex == 5)
                {
                    friendQueue = true;
                }
                else
                {
                    friendQueue = false;
                }
                matchFound = false;
            }

            if (modEnabled == false)
            {
                return;
            }

            CleanupCooldowns();

            var playerManager = PlayerManager.instance;
            if (playerManager == null || playerManager.AllPlayers == null)
            {
                return;
            }

            int currentPlayerCount = playerManager.AllPlayers.Count;

            if (currentPlayerCount > playerAmount && currentPlayerCount > 1)
            {
                MelonCoroutines.Start(DelayCharacterTransformation(currentSceneName, false));
            }

            playerAmount = currentPlayerCount;






        }

        private void ExecutePoseEffects(PlayerPoseSystem playerPoseSystem, Il2CppRUMBLE.Players.Player player)
        {

            GameObject controller = player.Controller.gameObject;
            GameObject visuals;

            visuals = controller.transform.GetChild(0).gameObject;


            Transform headsetTransform = player.Controller.transform.Find("VR/Headset Offset/Headset").transform;
            if (headsetTransform == null)
            {
                return;
            }

            UnityEngine.Vector3 boxCenter = headsetTransform.position + headsetTransform.forward * 2.0f;
            float boxRadius = 3.0f;
            UnityEngine.Vector3 playerPosition = headsetTransform.position;
            Transform poolParent = PoolManager.instance.GetPool("RockCube").poolParent;

            var modInstance = MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m is SonarMode) as SonarMode;
            if (modInstance != null)
            {

                List<GameObject> overlappedObjects = SonarMode.PerformInitialOverlapCheck(boxCenter, boxRadius, headsetTransform.rotation, poolParent);

            }
        }


        private void RenderConstantClone(GameObject original, Transform poolParent, bool NeedsHoldOrFlick, Transform vfxAsset, bool isExplode = false)
        {

            if (!modEnabled || original == null || original.active == false)
            {
                return;
            }

            if (constantCloneGroups.ContainsKey(original))
            {
                return;
            }

            if (vfxAsset != null && vfxAsset.gameObject != original)
            {
                vfxAsset.GetComponent<VisualEffect>().enabled = true;
                vfxAsset.gameObject.SetActive(true);
            }
            

            GameObject clone = GameObject.Instantiate(original);
            clone.name = "ObjectGhostClone";

            UnityEngine.Vector3 oldSize = original.transform.localScale;

            clone.transform.localScale = UnityEngine.Vector3.zero;

            MelonCoroutines.Start(ScaleClone(clone, oldSize, 0.05f));


            Renderer[] renderers = clone.GetComponentsInChildren<Renderer>();
            var yellowGhostMaterial = newParent.transform.Find("YellowGhostPoseObject").gameObject.GetComponent<Renderer>().material;

            foreach (var rend in renderers)
            {
                var materials = rend.materials;
                for (var g = 0; g < materials.Length; g++)
                {
                    materials[g] = yellowGhostMaterial;
                }
                rend.materials = materials;
                rend.enabled = true; 
            }

 
            Rigidbody[] rigidbodies = clone.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }

            Collider cloneCollider = clone.GetComponent<Collider>();
            MeshCollider meshCloneCollider = clone.GetComponent<MeshCollider>();

            Collider[] cloneColliders = clone.GetComponentsInChildren<Collider>();
            MeshCollider[] meshCloneColliders = clone.GetComponentsInChildren<MeshCollider>();

            foreach (var collider in cloneColliders)
            {
                collider.enabled = false;
            }

            foreach (var meshCollider in meshCloneColliders)
            {
                meshCollider.enabled = false;
            }

            if (cloneCollider != null)
            {
                cloneCollider.enabled = false;
            }

            if (meshCloneCollider != null)
            {
                meshCloneCollider.enabled = false;
            }

            

            constantCloneGroups.Add(original, true);

            MelonCoroutines.Start(FollowOriginalWithDelay(clone, original, 0, -1, false, NeedsHoldOrFlick));
        }





        public IEnumerator HoldStart(Transform poolParent, UnityEngine.Vector3 startPosition, float targetSize, bool isLocalPlayer, bool isHold)
        {
            if (!modEnabled)
            {
                yield break;
            }

            Collider[] hitColliders = Physics.OverlapSphere(startPosition, targetSize);
            List<GameObject> processedVisuals = new List<GameObject>();

            foreach (var hitCollider in hitColliders)
            {
                if (IsDescendantOf(hitCollider.transform, poolParent, true, false))
                {
                    if (!processedVisuals.Contains(hitCollider.gameObject))
                    {
                        processedVisuals.Add(hitCollider.gameObject);
                    }
                }
            }

            GameObject closest = FindClosestDescendant(processedVisuals, poolParent, startPosition);

            if (closest == null)
            {
                yield break;
            }

            string vfxType = isHold ? "Hold_VFX" : "Flick_VFX";
            var closestParent = closest.transform?.parent;
            bool returnImmediately = false;

            if (closestParent == null)
            {
                yield break;
            }

            bool isGrounded = isHold && closestParent.gameObject.GetComponent<Il2CppRUMBLE.MoveSystem.Structure>() && closestParent.gameObject.GetComponent<Il2CppRUMBLE.MoveSystem.Structure>()?.IsGrounded == true || isHold && closest.gameObject.GetComponent<Il2CppRUMBLE.MoveSystem.Structure>() && closest.gameObject.GetComponent<Il2CppRUMBLE.MoveSystem.Structure>()?.IsGrounded == true;
            var vfxAsset = closestParent.Find(vfxType);

            if (vfxAsset == null)
            {
                vfxAsset = closest.transform.Find(vfxType);
            }

            if (!isGrounded && !isLocalPlayer)
            {
                if (vfxAsset != null)
                {
                    vfxAsset.GetComponent<VisualEffect>().enabled = false;
                    vfxAsset.gameObject.SetActive(false);
                }


                DisableVFX(currentSceneName, false);

                returnImmediately = true;
            }

            yield return null;
            yield return null;
            yield return null;

            if (closestParent == null || closestParent.Find(vfxType) == null && closest.transform.Find(vfxType) == null)
            {
                yield break;
            }

            vfxAsset = closestParent.Find(vfxType);

            if (vfxAsset == null)
            {
                vfxAsset = closest.transform.Find(vfxType);
            }

            if (!isGrounded && !isLocalPlayer)
            {
                if (vfxAsset != null)
                {
                    vfxAsset.GetComponent<VisualEffect>().enabled = false;
                    vfxAsset.gameObject.SetActive(false);
                }
                

                DisableVFX(currentSceneName, false);

                yield break;
            }

            if (returnImmediately == true)
            {
                if (vfxAsset != null)
                {
                    vfxAsset.GetComponent<VisualEffect>().enabled = false;
                    vfxAsset.gameObject.SetActive(false);
                }

                yield break;
            }

            RenderConstantClone(closest, poolParent, true, vfxAsset);
        }

        
    }
}