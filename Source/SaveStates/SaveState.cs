﻿using System;
using System.Collections;
using System.IO;
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

            internal SaveStateData() { }
            
            
            internal SaveStateData(SaveStateData _data)
            {
                saveStateIdentifier = _data.saveStateIdentifier;
                saveScene = _data.saveScene;
                cameraLockArea = _data.cameraLockArea;
                savedPd = _data.savedPd;
                savedSd = _data.savedSd;
                savePos = _data.savePos;
                lockArea = _data.lockArea;
                isKinematized = _data.isKinematized;
                loadedScenes = new string[_data.loadedScenes.Length];
                Array.Copy(_data.loadedScenes, loadedScenes, _data.loadedScenes.Length);
            }
        }

        [SerializeField]
        public SaveStateData data;

        internal SaveState()
        {
            data = new SaveStateData();
        }

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
            int nLoadedScenes = USceneManager.sceneCount;
            data.loadedScenes = new string[nLoadedScenes];
            for (int i = 0; i < nLoadedScenes; i++)
            {
                data.loadedScenes[i] = USceneManager.GetSceneAt(i).name;
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
                File.WriteAllText (saveStateFile,
                    JsonUtility.ToJson( data, prettyPrint: true )
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

        public void LoadTempState(bool loadDuped)
        {
            GameManager.instance.StartCoroutine(LoadStateCoro(loadDuped));
        }

        public void NewLoadStateFromFile(bool loadDuped)
        {
            LoadStateFromFile(SaveStateManager.currentStateSlot);
            LoadTempState(loadDuped);
        }

        public void LoadStateFromFile(int paramSlot)
        {
            try
            {
                data.filePath = Path.Combine(SaveStateManager.path, $"savestate{paramSlot}.json");
                DebugMod.instance.Log("prep filepath: " + data.filePath);

                if (File.Exists(data.filePath))
                {
                    //DebugMod.instance.Log("checked filepath: " + data.filePath);
                    SaveStateData tmpData = JsonUtility.FromJson<SaveStateData>(File.ReadAllText(data.filePath));
                    try
                    {
                        data.saveStateIdentifier = tmpData.saveStateIdentifier;
                        data.cameraLockArea = tmpData.cameraLockArea;
                        data.savedPd = tmpData.savedPd;
                        data.savedSd = tmpData.savedSd;
                        data.savePos = tmpData.savePos;
                        data.saveScene = tmpData.saveScene;
                        data.lockArea = tmpData.lockArea;
                        data.isKinematized = tmpData.isKinematized;
                        data.loadedScenes = new string[tmpData.loadedScenes.Length];
                        Array.Copy(tmpData.loadedScenes, data.loadedScenes, tmpData.loadedScenes.Length);
                        DebugMod.instance.Log("Load SaveState ready: " + data.saveStateIdentifier);
                    }
                    catch (Exception ex)
                    {
                        DebugMod.instance.Log(string.Format(ex.Source, ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                DebugMod.instance.LogDebug(ex.Message);
                throw ex;
            }
        }

        private IEnumerator LoadStateCoro(bool loadDuped)
        {
            if (data.savedPd == null || string.IsNullOrEmpty(data.saveScene)) yield break;
            
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

            GameManager.instance.sceneData = SceneData.instance = JsonUtility.FromJson<SceneData>(JsonUtility.ToJson(data.savedSd));
            GameManager.instance.ResetSemiPersistentItems();

            yield return null;

            PlayerData.instance = GameManager.instance.playerData = HeroController.instance.playerData = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(data.savedPd));

            GameManager.instance.BeginSceneTransition
            (
                new GameManager.SceneLoadInfo
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
            
            if (loadDuped)
            {
                var GM = GameManager.instance;
                for (int i = 1; i < data.loadedScenes.Length; i++)
                {
                    AsyncOperation loadop = USceneManager.LoadSceneAsync(data.loadedScenes[i], LoadSceneMode.Additive);
                    loadop.allowSceneActivation = true;
                    yield return loadop;
                    GM.RefreshTilemapInfo(data.loadedScenes[i]);
                }
            }

            ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);
            
            GameManager.instance.cameraCtrl.PositionToHero(false);

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
                .Invoke(HeroController.instance, new object[] {true, false});
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