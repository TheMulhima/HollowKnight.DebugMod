using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using DebugMod.Hitbox;
using GlobalEnums;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    /// <summary>
    /// Handles struct SaveStateData and individual SaveState operations
    /// </summary>
    internal class SaveState
    {
        // Some mods (ItemChanger) check type to detect vanilla scene loads.
        private class DebugModSaveStateSceneLoadInfo : GameManager.SceneLoadInfo { }

        [Serializable]
        public class SaveStateData
        {
            public string saveStateIdentifier;
            public string saveScene;
            public PlayerData savedPd;
            public object lockArea;
            public SceneData savedSd;
            public Vector3 savePos;
            public FieldInfo cameraLockArea;
            public string filePath;
            public bool isKinematized;
            public string[] loadedScenes;
            public string[] loadedSceneActiveScenes;
            public int specialIndex;
            public bool isColoScene;
            public string coloWave;

            //not sure where to define this, maybe separate class and file, doesnt need to be in this class
            public readonly string[] specialScenes =
            {
            //Colos (0-2)
            "Room_Colosseum_Bronze",
            "Room_Colosseum_Silver",
            "Room_Colosseum_Gold"
            //Panths ()


            };




            internal SaveStateData() { }

            internal SaveStateData(SaveStateData _data)
            {
                saveStateIdentifier = _data.saveStateIdentifier;
                saveScene = _data.saveScene;

                //Special Case Variables
                specialIndex = _data.specialIndex;
                isColoScene = _data.isColoScene;
                coloWave = _data.coloWave;

                cameraLockArea = _data.cameraLockArea;
                savedPd = _data.savedPd;
                savedSd = _data.savedSd;
                savePos = _data.savePos;
                lockArea = _data.lockArea;
                isKinematized = _data.isKinematized;
                if (_data.loadedScenes is not null)
                {
                    loadedScenes = new string[_data.loadedScenes.Length];
                    Array.Copy(_data.loadedScenes, loadedScenes, _data.loadedScenes.Length);
                }
                else
                {
                    loadedScenes = new[] { saveScene };
                }

                loadedSceneActiveScenes = new string[loadedScenes.Length];
                if (_data.loadedSceneActiveScenes is not null)
                {
                    Array.Copy(_data.loadedSceneActiveScenes, loadedSceneActiveScenes, loadedSceneActiveScenes.Length);
                }
                else
                {
                    for (int i = 0; i < loadedScenes.Length; i++)
                    {
                        loadedSceneActiveScenes[i] = loadedScenes[i];
                    }
                }

            }
        }

        [SerializeField]
        public SaveStateData data;

        internal SaveState()
        {

            data = new SaveStateData();
        }


        #region Colosseums




        public string GrabSpecialSceneData(string curScene)
        {
            switch (curScene)

            {
                case "Room_Colosseum_Bronze": //0
                    data.isColoScene = true;
                    data.coloWave = GrabCurrentWave("Bronze");
                    break;

                case "Room_Colosseum_Silver": //1
                    data.isColoScene = true;
                    break;

                case "Room_Colosseum_Gold": //2
                    data.isColoScene = true;
                    break;


                default:
                    Console.AddLine("Saved Scene has no Special Definition");
                    break;


            }
            return curScene;
        }

        public string GrabCurrentWave(string coloLevel)
        {
            string wave = "2";
            return wave;
        }

        #endregion

        #region saving

        public void SaveTempState()
        {
            data.saveScene = GameManager.instance.GetSceneNameString();
            data.saveStateIdentifier = $"(tmp)_{data.saveScene}-{DateTime.Now.ToString("H:mm_d-MMM")}";
            data.savedPd = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(PlayerData.instance));
            data.savedSd = JsonUtility.FromJson<SceneData>(JsonUtility.ToJson(SceneData.instance));
            data.savePos = HeroController.instance.gameObject.transform.position;
            data.cameraLockArea = (data.cameraLockArea ?? typeof(CameraController).GetField("currentLockArea", BindingFlags.Instance | BindingFlags.NonPublic));
            data.lockArea = data.cameraLockArea.GetValue(GameManager.instance.cameraCtrl);
            data.isKinematized = HeroController.instance.GetComponent<Rigidbody2D>().isKinematic;
            var scenes = SceneWatcher.LoadedScenes;
            data.loadedScenes = scenes.Select(s => s.name).ToArray();
            data.loadedSceneActiveScenes = scenes.Select(s => s.activeSceneWhenLoaded).ToArray();
            //heres cc's stuff
            data.specialIndex = Array.IndexOf(data.specialScenes, data.saveScene);
            if (data.specialIndex > -1)
            {
                GrabSpecialSceneData(data.saveScene);
            }
        }

        public void NewSaveStateToFile(int paramSlot)
        {
            SaveTempState();
            SaveStateToFile(paramSlot);
        }

        public void SaveStateToFile(int paramSlot)
        {
            try
            {
                if (data.saveStateIdentifier.StartsWith("(tmp)_"))
                {
                    data.saveStateIdentifier = data.saveStateIdentifier.Substring(6);
                }
                else if (String.IsNullOrEmpty(data.saveStateIdentifier))
                {
                    throw new Exception("No temp save state set");
                }

                string saveStateFile = Path.Combine(SaveStateManager.path, $"savestate{paramSlot}.json");
                File.WriteAllText(saveStateFile,
                    JsonUtility.ToJson(data, prettyPrint: true)
                );
            }
            catch (Exception ex)
            {
                DebugMod.instance.LogDebug(ex.Message);
                throw ex;
            }
        }
        #endregion

        #region loading

        //loadDuped is used by external mods
        public void LoadTempState(bool loadDuped = false)
        {
            if (!PlayerDeathWatcher.playerDead && !HeroController.instance.cState.transitioning && (HeroController.instance.transform.parent == null))
            {
                GameManager.instance.StartCoroutine(LoadStateCoro(loadDuped));
            }
            else
            {
                Console.AddLine("SaveStates cannot be loaded when dead, transitioning, or on elevators");
            }
        }

        //loadDuped is used by external mods
        public void NewLoadStateFromFile(bool loadDuped = false)
        {
            LoadStateFromFile(SaveStateManager.currentStateSlot);
            LoadTempState(loadDuped);
        }

        public void LoadStateFromFile(int paramSlot)
        {
            try
            {
                data.filePath = Path.Combine(SaveStateManager.path, $"savestate{paramSlot}.json");

                if (File.Exists(data.filePath))
                {
                    //DebugMod.instance.Log("checked filepath: " + data.filePath);
                    SaveStateData tmpData = JsonUtility.FromJson<SaveStateData>(File.ReadAllText(data.filePath));
                    try
                    {
                        data = new SaveStateData(tmpData);
                        DebugMod.instance.Log("Load SaveState ready: " + data.saveStateIdentifier);
                    }
                    catch (Exception ex)
                    {
                        DebugMod.instance.LogError("Error applying save state data: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugMod.instance.LogDebug(ex);
                throw;
            }
        }

        //loadDuped is used by external mods
        private IEnumerator LoadStateCoro(bool loadDuped)
        {
            if (data.savedPd == null || string.IsNullOrEmpty(data.saveScene)) yield break;

            //remove dialogues if exists
            PlayMakerFSM.BroadcastEvent("BOX DOWN DREAM");
            PlayMakerFSM.BroadcastEvent("CONVO CANCEL");

            GameManager.instance.entryGateName = "dreamGate";
            GameManager.instance.startedOnThisScene = true;

            //Menderbug room loads faster (Thanks Magnetic Pizza)
            string dummySceneName = "Room_Mender_House";
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Room_Mender_House")
            {
                dummySceneName = "Room_Sly_Storeroom";
            }

            USceneManager.LoadScene(dummySceneName);

            yield return new WaitUntil(() => USceneManager.GetActiveScene().name == dummySceneName);

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.savedSd), SceneData.instance);
            GameManager.instance.ResetSemiPersistentItems();

            yield return null;

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.savedPd), PlayerData.instance);

            SceneWatcher.LoadedSceneInfo[] sceneData = data
                .loadedScenes
                .Zip(data.loadedSceneActiveScenes, (name, gameplay) => new SceneWatcher.LoadedSceneInfo(name, gameplay))
                .ToArray();

            sceneData[0].LoadHook();

            GameManager.instance.BeginSceneTransition
            (
                new DebugModSaveStateSceneLoadInfo
                {
                    SceneName = data.saveScene,
                    HeroLeaveDirection = GatePosition.unknown,
                    EntryGateName = "dreamGate",
                    EntryDelay = 0f,
                    WaitForSceneTransitionCameraFade = false,
                    Visualization = 0,
                    AlwaysUnloadUnusedAssets = true
                }
            );

            yield return new WaitUntil(() => USceneManager.GetActiveScene().name == data.saveScene);

            GameManager.instance.cameraCtrl.PositionToHero(false);

            ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);

            if (loadDuped)
            {
                yield return new WaitUntil(() => GameManager.instance.IsInSceneTransition == false);
                for (int i = 1; i < sceneData.Length; i++)
                {
                    On.GameManager.UpdateSceneName += sceneData[i].UpdateSceneNameOverride;
                    AsyncOperation loadop = USceneManager.LoadSceneAsync(sceneData[i].name, LoadSceneMode.Additive);
                    loadop.allowSceneActivation = true;
                    yield return loadop;
                    On.GameManager.UpdateSceneName -= sceneData[i].UpdateSceneNameOverride;
                    GameManager.instance.RefreshTilemapInfo(sceneData[i].name);
                    GameManager.instance.cameraCtrl.SceneInit();
                }
                GameManager.instance.BeginScene();
            }

            if (data.lockArea != null)
            {
                GameManager.instance.cameraCtrl.LockToArea(data.lockArea as CameraLockArea);
            }

            GameManager.instance.cameraCtrl.FadeSceneIn();

            HeroController.instance.TakeMP(1);
            HeroController.instance.AddMPChargeSpa(1);
            HeroController.instance.TakeHealth(1);
            HeroController.instance.AddHealth(1);

            HeroController.instance.geoCounter.geoTextMesh.text = data.savedPd.geo.ToString();

            GameCameras.instance.hudCanvas.gameObject.SetActive(true);

            FieldInfo cameraGameplayScene = typeof(CameraController).GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);

            cameraGameplayScene.SetValue(GameManager.instance.cameraCtrl, true);

            yield return null;

            HeroController.instance.gameObject.transform.position = data.savePos;
            HeroController.instance.transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
            HeroController.instance.GetComponent<Rigidbody2D>().isKinematic = data.isKinematized;

            if (loadDuped && DebugMod.settings.ShowHitBoxes > 0)
            {
                int cs = DebugMod.settings.ShowHitBoxes;
                DebugMod.settings.ShowHitBoxes = 0;
                yield return new WaitUntil(() => HitboxViewer.State == 0);
                DebugMod.settings.ShowHitBoxes = cs;
            }

            typeof(HeroController)
                .GetMethod("FinishedEnteringScene", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(HeroController.instance, new object[] { true, false });
            ReflectionHelper.CallMethod(GameManager.instance, "UpdateUIStateFromGameState");
        }
        #endregion

        #region helper functionality

        public bool IsSet()
        {
            bool isSet = !String.IsNullOrEmpty(data.saveStateIdentifier);
            return isSet;
        }

        public string GetSaveStateID()
        {
            return data.saveStateIdentifier;
        }

        public string[] GetSaveStateInfo()
        {
            return new string[]
            {
                data.saveStateIdentifier,
                data.saveScene
            };
        }
        public SaveState.SaveStateData DeepCopy()
        {
            return new SaveState.SaveStateData(this.data);
        }

        #endregion
    }
}