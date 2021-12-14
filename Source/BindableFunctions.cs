using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using Random = System.Random;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using On;

namespace DebugMod
{
    public static class BindableFunctions
    {
        private static readonly FieldInfo TimeSlowed = typeof(GameManager).GetField("timeSlowed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo IgnoreUnpause = typeof(UIManager).GetField("ignoreUnpause", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        
        internal static readonly FieldInfo cameraGameplayScene = typeof(CameraController).GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);
        
        private static FieldInfo cameraLockArea = typeof(CameraController).GetField("currentLockArea", BindingFlags.Instance | BindingFlags.NonPublic);

        #region Misc

        [BindableMethod(name = "Nail Damage +4", category = "Misc")]
        public static void IncreaseNailDamage()
        {
            int num = 4;
            if (PlayerData.instance.nailDamage == 0)
            {
                num = 5;
            }
            PlayerData.instance.nailDamage += num;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            Console.AddLine("Increased base nailDamage by " + num);
        }

        [BindableMethod(name = "Nail Damage -4", category = "Misc")]
        public static void DecreaseNailDamage()
        {
            int num2 = PlayerData.instance.nailDamage - 4;
            if (num2 >= 0)
            {
                PlayerData.instance.nailDamage = num2;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                Console.AddLine("Decreased base nailDamage by 4");
            }
            else
            {
                Console.AddLine("Cannot set base nailDamage less than 0 therefore forcing 0 value");
                PlayerData.instance.nailDamage = 0;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
        }

        [BindableMethod(name = "Force Pause", category = "Misc")]
        public static void ForcePause()
        {
            try
            {
                if ((PlayerData.instance.disablePause || (bool)TimeSlowed.GetValue(GameManager.instance) || (bool)IgnoreUnpause.GetValue(UIManager.instance)) && DebugMod.GetSceneName() != "Menu_Title" && DebugMod.GM.IsGameplayScene())
                {
                    TimeSlowed.SetValue(GameManager.instance, false);
                    IgnoreUnpause.SetValue(UIManager.instance, false);
                    PlayerData.instance.disablePause = false;
                    UIManager.instance.TogglePauseGame();
                    Console.AddLine("Forcing Pause Menu because pause is disabled");
                }
                else
                {
                    Console.AddLine("Game does not report that Pause is disabled, requesting it normally.");
                    UIManager.instance.TogglePauseGame();
                }
            }
            catch (Exception e)
            {
                Console.AddLine("Error while attempting to pause, check ModLog.txt");
                DebugMod.instance.Log("Error while attempting force pause:\n" + e);
            }
        }

        [BindableMethod(name = "Hazard Respawn", category = "Misc")]
        public static void Respawn()
        {
            if (GameManager.instance.IsGameplayScene() && !HeroController.instance.cState.dead && PlayerData.instance.health > 0)
            {
                if (UIManager.instance.uiState.ToString() == "PAUSED")
                {
                    UIManager.instance.TogglePauseGame();
                    GameManager.instance.HazardRespawn();
                    Console.AddLine("Closing Pause Menu and respawning...");
                    return;
                }
                if (UIManager.instance.uiState.ToString() == "PLAYING")
                {
                    HeroController.instance.RelinquishControl();
                    GameManager.instance.HazardRespawn();
                    HeroController.instance.RegainControl();
                    Console.AddLine("Respawn signal sent");
                    return;
                }
                Console.AddLine("Respawn requested in some weird conditions, abort, ABORT");
            }
        }

        [BindableMethod(name = "Set Respawn", category = "Misc")]
        public static void SetHazardRespawn()
        {
            Vector3 manualRespawn = DebugMod.RefKnight.transform.position;
            HeroController.instance.SetHazardRespawn(manualRespawn, false);
            Console.AddLine("Manual respawn point on this map set to" + manualRespawn);
        }

        [BindableMethod(name = "Force Camera Follow", category = "Misc")]
        public static void ForceCameraFollow()
        {
            if (!DebugMod.cameraFollow)
            {
                Console.AddLine("Forcing camera follow");
                DebugMod.cameraFollow = true;
            }
            else
            {
                DebugMod.cameraFollow = false;
                cameraGameplayScene.SetValue(DebugMod.RefCamera, true);
                Console.AddLine("Returning camera to normal settings");
            }
        }
        
        [BindableMethod(name = "Decrease Timescale", category = "Misc")]
        public static void TimescaleDown()
        {
            //This needs to be added because the game sets timescale to 0 when paused to pause the game if this is changed to a 
            //non-zero value, the game continues to play even tho the pause menu is up which is scuffed so this makes it less skuffed
            if (DebugMod.GM.IsGamePaused())
            {
                Console.AddLine("Cannot change timescale when paused");
                return;
            }
            float oldScale = Time.timeScale;
            bool wasTimeScaleActive = DebugMod.TimeScaleActive;
            Time.timeScale -= 0.1f;
            DebugMod.CurrentTimeScale = Time.timeScale;

            DebugMod.TimeScaleActive = DebugMod.CurrentTimeScale != 1f;

            switch (DebugMod.TimeScaleActive)
            {
                case true when wasTimeScaleActive == false:
                    EnableTimeScale();
                    break;
                case false when wasTimeScaleActive == true:
                    DisableTimeScale();
                    break;
            }
            Console.AddLine("New TimeScale value: " + oldScale + " Old value: " + DebugMod.CurrentTimeScale);

        }

        [BindableMethod(name = "Increase Timescale", category = "Misc")]
        public static void TimescaleUp()
        {
            if (DebugMod.GM.IsGamePaused())
            {
                Console.AddLine("Cannot change timescale when paused");
                return;
            }
            float oldScale = Time.timeScale;
            bool wasTimeScaleActive = DebugMod.TimeScaleActive;
            Time.timeScale += 0.1f;
            DebugMod.CurrentTimeScale = Time.timeScale;

            DebugMod.TimeScaleActive = DebugMod.CurrentTimeScale != 1f;

            switch (DebugMod.TimeScaleActive)
            {
                case true when wasTimeScaleActive == false:
                    EnableTimeScale();
                    break;
                case false when wasTimeScaleActive == true:
                    DisableTimeScale();
                    break;
            }
            Console.AddLine("New TimeScale value: " + oldScale + " Old value: " + DebugMod.CurrentTimeScale);
        }

        private static void EnableTimeScale()
        {
            DebugMod.TimeScaleActive = true;
            On.GameManager.SetTimeScale_float += GameManager_SetTimeScale_1;
            On.GameManager.FreezeMoment_float_float_float_float += GameManager_FreezeMoment_1;
            On.QuitToMenu.Start += Quit_To_Menu;
        }
        
        private static void DisableTimeScale()
        {
            Time.timeScale = 1f;
            DebugMod.CurrentTimeScale = 1f;
            DebugMod.TimeScaleActive = false;
            On.GameManager.SetTimeScale_float -= GameManager_SetTimeScale_1;
            On.GameManager.FreezeMoment_float_float_float_float -= GameManager_FreezeMoment_1;
            On.QuitToMenu.Start -= Quit_To_Menu;
        }
        private static IEnumerator Quit_To_Menu(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            yield return null;
            UIManager ui = UIManager.instance;
            bool flag = ui != null;
            if (flag)
            {
                UIManager.instance.AudioGoToGameplay(0f);
                UnityEngine.Object.Destroy(ui.gameObject);
            }
            HeroController heroController = HeroController.instance;
            bool flag2 = heroController != null;
            if (flag2)
            {
                UnityEngine.Object.Destroy(heroController.gameObject);
            }
            GameCameras gameCameras = GameCameras.instance;
            bool flag3 = gameCameras != null;
            if (flag3)
            {
                UnityEngine.Object.Destroy(gameCameras.gameObject);
            }
            GameManager gameManager = GameManager.instance;
            bool flag4 = gameManager != null;
            if (flag4)
            {
                try
                {
                    ObjectPool.RecycleAll();
                }
                catch (Exception ex)
                {
                    Exception exception = ex;
                    Debug.LogErrorFormat("Error while recycling all as part of quit, attempting to continue regardless.", new object[0]);
                    Debug.LogException(exception);
                }
                gameManager.playerData.Reset();
                gameManager.sceneData.Reset();
                UnityEngine.Object.Destroy(gameManager.gameObject);
            }
            Time.timeScale = DebugMod.CurrentTimeScale;
            yield return null;
            GCManager.Collect();
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu_Title", 0);
            DisableTimeScale();
        }
        private static IEnumerator GameManager_FreezeMoment_1(On.GameManager.orig_FreezeMoment_float_float_float_float orig, GameManager self, float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
        {
            FieldInfo timeSlowedField = typeof(GameManager).GetField("TimeSlowed", BindingFlags.Instance | BindingFlags.Public);
            bool flag = self.TimeSlowed;
            if (flag)
            {
                yield break;
            }
            timeSlowedField.SetValue(self,true);
            yield return self.StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
            for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime * DebugMod.CurrentTimeScale)
            {
                yield return null;
            }
            yield return self.StartCoroutine(SetTimeScale(1f, rampUpTime));
            timeSlowedField.SetValue(self,false);
            yield break;
        }

        private static IEnumerator SetTimeScale(float newTimeScale, float duration)
        {
            float lastTimeScale = Time.timeScale;
            for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
            {
                float val = Mathf.Clamp01(timer / duration);
                SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
                yield return null;
            }
            SetTimeScale(newTimeScale);
            yield break;
        }

        private static void SetTimeScale(float newTimeScale)
        {
            Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale) * DebugMod.CurrentTimeScale;
        }

        private static void GameManager_SetTimeScale_1(On.GameManager.orig_SetTimeScale_float orig, GameManager self, float newTimeScale)
        {
            Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale) * DebugMod.CurrentTimeScale;
        }

        [BindableMethod(name = "Reset settings", category = "Misc")]
        public static void Reset()
        {
            var pd = PlayerData.instance;
            var HC = HeroController.instance;
            var GC = GameCameras.instance;
            
            //nail damage
            pd.nailDamage = 5 + pd.nailSmithUpgrades * 4;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");

            //Hero Light
            GameObject gameObject = DebugMod.RefKnight.transform.Find("HeroLight").gameObject;
            Color color = gameObject.GetComponent<SpriteRenderer>().color;
            color.a = 0.7f;
            gameObject.GetComponent<SpriteRenderer>().color = color;
            
            //HUD
            //if (!GC.hudCanvas.gameObject.activeInHierarchy) 
                //GC.hudCanvas.gameObject.SetActive(true);
            
            //Hide Hero
            tk2dSprite component = DebugMod.RefKnight.GetComponent<tk2dSprite>();
            color = component.color;  color.a = 1f;
            component.color = color;

            //rest all is self explanatory
            DisableTimeScale();
            GC.tk2dCam.ZoomFactor = 1f;
            HC.vignette.enabled = false;
            EnemiesPanel.hitboxes = false;
            //EnemiesPanel.hpBars = false;
            EnemiesPanel.autoUpdate = false;
            pd.infiniteAirJump=false;
            DebugMod.infiniteSoul = false;
            DebugMod.infiniteHP = false; 
            pd.isInvincible=false; 
            DebugMod.noclip=false;
        }

        #endregion
        
        #region SaveStates 

        [BindableMethod(name = "Quickslot (save)", category = "Savestates")]
        public static void SaveState()
        {
            DebugMod.saveStateManager.SaveState(SaveStateType.Memory);
        }

        [BindableMethod(name = "Quickslot (load)", category = "Savestates")]
        public static void LoadState()
        {
            DebugMod.saveStateManager.LoadState(SaveStateType.Memory);
        }

        [BindableMethod(name = "Quickslot save to file", category = "Savestates")]
        public static void CurrentSaveStateToFile()
        {
            DebugMod.saveStateManager.SaveState(SaveStateType.File);
        }

        [BindableMethod(name = "Load file to quickslot", category = "Savestates")]
        public static void CurrentSlotToSaveMemory()
        {
            DebugMod.saveStateManager.LoadState(SaveStateType.File);
        }

        [BindableMethod(name = "Save new state to file", category = "Savestates")]
        public static void NewSaveStateToFile()
        {
            DebugMod.saveStateManager.SaveState(SaveStateType.SkipOne);

        }
        [BindableMethod(name = "Load new state from file", category = "Savestates")]
        public static void LoadFromFile()
        {
            DebugMod.saveStateManager.LoadState(SaveStateType.SkipOne);
        }

        /*
        [BindableMethod(name = "Toggle auto slot", category = "Savestates")]
        public static void ToggleAutoSlot()
        {
            DebugMod.saveStateManager.ToggleAutoSlot();
        }
        
        
        [BindableMethod(name = "Refresh state menu", category = "Savestates")]
        public static void RefreshSaveStates()
        {
            DebugMod.saveStateManager.RefreshStateMenu();
        }
        */

        #endregion
        
        #region Visual

        [BindableMethod(name = "Show Hitboxes", category = "Visual")]
        public static void ShowHitboxes()
        {
            if (++DebugMod.settings.ShowHitBoxes > 2) DebugMod.settings.ShowHitBoxes = 0;
            Console.AddLine("Toggled show hitboxes: " + DebugMod.settings.ShowHitBoxes);
        }
        
        [BindableMethod(name = "Toggle Vignette", category = "Visual")]
        public static void ToggleVignette()
        {
            HeroController.instance.vignette.enabled = !HeroController.instance.vignette.enabled;
        }
        //
        
        [BindableMethod(name = "Deactivate Visual Masks", category = "Visual")]
        public static void DeactivateVisualMasks()
        {
            int ctr = 0;

            void disableMask(GameObject go)
            {
                foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                {
                    if (r.enabled)
                    {
                        ctr++;
                        r.enabled = false;
                    }
                }
            }

            float knightZ = HeroController.instance.transform.position.z;
            foreach (GameObject go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (go.transform.position.z > knightZ) continue;

                // A collection of ways to identify masks. It's possible some slip through the cracks I guess
                if (go.name.StartsWith("msk_"))
                    disableMask(go);
                else if (go.name.StartsWith("Tut_msk"))
                    disableMask(go);
                else if (go.name.StartsWith("black_solid"))
                    disableMask(go);
                else if (go.name.ToLower().Contains("vignette"))
                    disableMask(go);
                else if (go.LocateMyFSM("unmasker") is PlayMakerFSM)
                    disableMask(go);
                else if (go.LocateMyFSM("remasker_inverse") is PlayMakerFSM)
                    disableMask(go);
                else if (go.LocateMyFSM("remasker") is PlayMakerFSM)
                    disableMask(go);
            }

            Console.AddLine($"Deactivated {ctr} masks" + (HeroController.instance.vignette.enabled ? " and toggling vignette off" : string.Empty));

            // The vignette counts as a visual mask :)
            HeroController.instance.vignette.enabled = false;
        }
        
        [BindableMethod(name = "Toggle Hero Light", category = "Visual")]
        public static void ToggleHeroLight()
        {
            GameObject gameObject = DebugMod.RefKnight.transform.Find("HeroLight").gameObject;
            Color color = gameObject.GetComponent<SpriteRenderer>().color;
            if (Math.Abs(color.a) > 0f)
            {
                color.a = 0f;
                gameObject.GetComponent<SpriteRenderer>().color = color;
                Console.AddLine("Rendering HeroLight invisible...");
            }
            else
            {
                color.a = 0.7f;
                gameObject.GetComponent<SpriteRenderer>().color = color;
                Console.AddLine("Rendering HeroLight visible...");
            }
        }

        [BindableMethod(name = "Toggle HUD", category = "Visual")]
        public static void ToggleHUD()
        {
            if (GameCameras.instance.hudCanvas.gameObject.activeInHierarchy)
            {
                GameCameras.instance.hudCanvas.gameObject.SetActive(false);
                Console.AddLine("Disabling HUD...");
            }
            else
            {
                GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                Console.AddLine("Enabling HUD...");
            }
        }

        [BindableMethod(name = "Reset Camera Zoom", category = "Visual")]
        public static void ResetZoom()
        {
            GameCameras.instance.tk2dCam.ZoomFactor = 1f;
            Console.AddLine("Zoom factor was reset");
        }

        [BindableMethod(name = "Zoom In", category = "Visual")]
        public static void ZoomIn()
        {
            float zoomFactor = GameCameras.instance.tk2dCam.ZoomFactor;
            GameCameras.instance.tk2dCam.ZoomFactor = zoomFactor + zoomFactor * 0.05f;
            Console.AddLine("Zoom level increased to: " + GameCameras.instance.tk2dCam.ZoomFactor);
        }

        [BindableMethod(name = "Zoom Out", category = "Visual")]
        public static void ZoomOut()
        {
            float zoomFactor2 = GameCameras.instance.tk2dCam.ZoomFactor;
            GameCameras.instance.tk2dCam.ZoomFactor = zoomFactor2 - zoomFactor2 * 0.05f;
            Console.AddLine("Zoom level decreased to: " + GameCameras.instance.tk2dCam.ZoomFactor);
        }

        [BindableMethod(name = "Hide Hero", category = "Visual")]
        public static void HideHero()
        {
            tk2dSprite component = DebugMod.RefKnight.GetComponent<tk2dSprite>();
            Color color = component.color;
            if (Math.Abs(color.a) > 0f)
            {
                color.a = 0f;
                component.color = color;
                Console.AddLine("Rendering Hero sprite invisible...");
            }
            else
            {
                color.a = 1f;
                component.color = color;
                Console.AddLine("Rendering Hero sprite visible...");
            }
        }
        [BindableMethod(name = "Toggle Camera Shake", category = "Visual")]
        public static void ToggleCameraShake()
        {
            bool newValue = !GameCameras.instance.cameraShakeFSM.enabled;
            GameCameras.instance.cameraShakeFSM.enabled = newValue;
            Console.AddLine($"{(newValue ? "Enabling" : "Disabling")} Camera Shake...");
        }

        #endregion

        #region Panels

        [BindableMethod(name = "Toggle All UI", category = "Mod UI")]
        public static void ToggleAllPanels()
        {
            bool active = !(
                DebugMod.settings.HelpPanelVisible ||
                DebugMod.settings.InfoPanelVisible ||
                DebugMod.settings.EnemiesPanelVisible ||
                DebugMod.settings.TopMenuVisible ||
                DebugMod.settings.ConsoleVisible ||
                DebugMod.settings.MinInfoPanelVisible
                );

            if (MinimalInfoPanel.minInfo)
            {
                DebugMod.settings.InfoPanelVisible = false;
                DebugMod.settings.MinInfoPanelVisible = active;
            }
            else
            {
                DebugMod.settings.InfoPanelVisible = active;
                DebugMod.settings.MinInfoPanelVisible = false;
            }
            DebugMod.settings.TopMenuVisible = active;
            DebugMod.settings.EnemiesPanelVisible = active;
            DebugMod.settings.ConsoleVisible = active;
            DebugMod.settings.HelpPanelVisible = active;

            if (DebugMod.settings.EnemiesPanelVisible)
            {
                EnemiesPanel.RefreshEnemyList();
            }
        }

        [BindableMethod(name = "Toggle Binds", category = "Mod UI")]
        public static void ToggleHelpPanel()
        {
            DebugMod.settings.HelpPanelVisible = !DebugMod.settings.HelpPanelVisible;
        }

        [BindableMethod(name = "Toggle Info", category = "Mod UI")]
        public static void ToggleInfoPanel()
        {
            if (MinimalInfoPanel.minInfo)
            {
                DebugMod.settings.InfoPanelVisible = false;
                DebugMod.settings.MinInfoPanelVisible = !DebugMod.settings.MinInfoPanelVisible;
            }
            else
            {
                DebugMod.settings.InfoPanelVisible = !DebugMod.settings.InfoPanelVisible;
                DebugMod.settings.MinInfoPanelVisible = false;
            }
        }

        [BindableMethod(name = "Toggle Top Menu", category = "Mod UI")]
        public static void ToggleTopRightPanel()
        {
            DebugMod.settings.TopMenuVisible = !DebugMod.settings.TopMenuVisible;
        }

        [BindableMethod(name = "Toggle Console", category = "Mod UI")]
        public static void ToggleConsole()
        {
            DebugMod.settings.ConsoleVisible = !DebugMod.settings.ConsoleVisible;
        }

        [BindableMethod(name = "Toggle Enemy Panel", category = "Mod UI")]
        public static void ToggleEnemyPanel()
        {
            DebugMod.settings.EnemiesPanelVisible = !DebugMod.settings.EnemiesPanelVisible;
            if (DebugMod.settings.EnemiesPanelVisible)
            {
                EnemiesPanel.RefreshEnemyList();
            }
        }

        [BindableMethod(name = "Toggle SaveState Panel", category = "Mod UI")]
        public static void ToggleSaveStatesPanel()
        {
            DebugMod.settings.SaveStatePanelVisible = !DebugMod.settings.SaveStatePanelVisible;
        }

        // A variant of info panel. View handled in the two InfoPanel classes
        //  TODO: stop not knowing how to use xor in c#
        [BindableMethod(name = "Alt. Info Switch", category = "Mod UI")]
        public static void ToggleFullInfo()
        {
            MinimalInfoPanel.minInfo = !MinimalInfoPanel.minInfo;
            
            if (MinimalInfoPanel.minInfo) 
            {
                if (DebugMod.settings.InfoPanelVisible)
                {
                    DebugMod.settings.InfoPanelVisible = false;
                    DebugMod.settings.MinInfoPanelVisible = true;
                }
            }
            else
            {
                if (DebugMod.settings.MinInfoPanelVisible)
                {
                    DebugMod.settings.MinInfoPanelVisible = false;
                    DebugMod.settings.InfoPanelVisible = true;
                }
            }
            
        }

        #endregion

        #region Enemies

        //probably should delete this as it has become obsolete becuase of the new Show Hitboxes in the visuals page
        [BindableMethod(name = "Toggle Enemy Hitboxes", category = "Enemy Panel")]
        public static void ToggleEnemyCollision()
        {
            EnemiesPanel.hitboxes = !EnemiesPanel.hitboxes;

            if (EnemiesPanel.hitboxes)
            {
                Console.AddLine("Enabled hitboxes");
            }
            else
            {
                Console.AddLine("Disabled hitboxes");
            }
        }

        [BindableMethod(name = "Toggle HP Bars", category = "Enemy Panel")]
        public static void ToggleEnemyHPBars()
        {
            EnemiesPanel.hpBars = !EnemiesPanel.hpBars;

            if (EnemiesPanel.hpBars)
            {
                Console.AddLine("Enabled HP bars");
            }
            else
            {
                Console.AddLine("Disabled HP bars");
            }
        }

        [BindableMethod(name = "Toggle Enemy Scan", category = "Enemy Panel")]
        public static void ToggleEnemyAutoScan()
        {
            EnemiesPanel.autoUpdate = !EnemiesPanel.autoUpdate;

            if (EnemiesPanel.autoUpdate)
            {
                Console.AddLine("Enabled auto-scan (May impact performance)");
            }
            else
            {
                Console.AddLine("Disabled auto-scan");
            }
        }

        [BindableMethod(name = "Enemy Scan", category = "Enemy Panel")]
        public static void EnemyScan()
        {
            EnemiesPanel.EnemyUpdate(200f);
            Console.AddLine("Refreshing collider data...");
        }

        [BindableMethod(name = "Self Damage", category = "Enemy Panel")]
        public static void SelfDamage()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene() || GameManager.instance.IsGamePaused() || HeroController.instance.cState.recoiling || HeroController.instance.cState.invulnerable)
            {
                Console.AddLine("Unacceptable conditions for selfDamage(" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            GameManager.instance.gameObject.AddComponent<SelfDamage>();

            Console.AddLine("Attempting self damage");
        }
        [BindableMethod(name = "Add a mask", category = "Enemy Panel")]
        public static void AddMask()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Unacceptable conditions for adding masks" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            HeroController.instance.AddHealth(1);

            Console.AddLine("Attempting to add mask");
        }
        [BindableMethod(name = "Remove a mask", category = "Enemy Panel")]
        public static void RemoveMask()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Unacceptable conditions for removing masks" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            HeroController.instance.TakeHealth(1);

            Console.AddLine("Attempting to remove mask");
        }

        #endregion

        #region Console

        [BindableMethod(name = "Dump Console", category = "Console")]
        public static void DumpConsoleLog()
        {
            Console.AddLine("Saving console log...");
            Console.SaveHistory();
        }

        #endregion

        #region Cheats

        [BindableMethod(name = "Kill All", category = "Cheats")]
        public static void KillAll()
        {
            PlayMakerFSM.BroadcastEvent("INSTA KILL");
            Console.AddLine("INSTA KILL broadcasted!");
        }

        [BindableMethod(name = "Infinite Jump", category = "Cheats")]
        public static void ToggleInfiniteJump()
        {
            PlayerData.instance.infiniteAirJump = !PlayerData.instance.infiniteAirJump;
            Console.AddLine("Infinite Jump set to " + PlayerData.instance.infiniteAirJump.ToString().ToUpper());
        }

        [BindableMethod(name = "Infinite Soul", category = "Cheats")]
        public static void ToggleInfiniteSoul()
        {
            DebugMod.infiniteSoul = !DebugMod.infiniteSoul;
            Console.AddLine("Infinite SOUL set to " + DebugMod.infiniteSoul.ToString().ToUpper());
        }

        [BindableMethod(name = "Infinite HP", category = "Cheats")]
        public static void ToggleInfiniteHP()
        {
            DebugMod.infiniteHP = !DebugMod.infiniteHP;
            Console.AddLine("Infinite HP set to " + DebugMod.infiniteHP.ToString().ToUpper());
        }

        [BindableMethod(name = "Invincibility", category = "Cheats")]
        public static void ToggleInvincibility()
        {
            PlayerData.instance.isInvincible = !PlayerData.instance.isInvincible;
            Console.AddLine("Invincibility set to " + PlayerData.instance.isInvincible.ToString().ToUpper());

            DebugMod.playerInvincible = PlayerData.instance.isInvincible;
        }

        [BindableMethod(name = "Noclip", category = "Cheats")]
        public static void ToggleNoclip()
        {
            DebugMod.noclip = !DebugMod.noclip;

            if (DebugMod.noclip)
            {
                Console.AddLine("Enabled noclip");
                DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
            }
            else
            {
                Console.AddLine("Disabled noclip");
            }
        }

        [BindableMethod(name = "Kill Self", category = "Cheats")]
        public static void KillSelf()
        {
            if (DebugMod.GM.isPaused) UIManager.instance.TogglePauseGame();
            HeroController.instance.TakeHealth(9999);
            
            HeroController.instance.heroDeathPrefab.SetActive(true);
            DebugMod.GM.ReadyForRespawn(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        }
        
        #endregion

        #region Charms

        [BindableMethod(name = "Give All Charms", category = "Charms")]
        public static void GiveAllCharms()
        {
            for (int i = 1; i <= 40; i++)
            {
                PlayerData.instance.SetBoolInternal("gotCharm_" + i, true);

                if (i == 36 && !DebugMod.GrimmTroupe())
                {
                    break;
                }
            }

            PlayerData.instance.charmSlots = 10;
            PlayerData.instance.hasCharm = true;
            PlayerData.instance.charmsOwned = 40;
            PlayerData.instance.royalCharmState = 4;
            PlayerData.instance.gotShadeCharm = true;
            PlayerData.instance.gotKingFragment = true;
            PlayerData.instance.gotQueenFragment = true;
            PlayerData.instance.notchShroomOgres = true;
            PlayerData.instance.notchFogCanyon = true;
            PlayerData.instance.colosseumBronzeOpened = true;
            PlayerData.instance.colosseumBronzeCompleted = true;
            PlayerData.instance.salubraNotch1 = true;
            PlayerData.instance.salubraNotch2 = true;
            PlayerData.instance.salubraNotch3 = true;
            PlayerData.instance.salubraNotch4 = true;

            if (DebugMod.GrimmTroupe())
            {
                PlayerData.instance.SetBoolInternal("fragileGreed_unbreakable", true);
                PlayerData.instance.SetBoolInternal("fragileHealth_unbreakable", true);
                PlayerData.instance.SetBoolInternal("fragileStrength_unbreakable", true);
                PlayerData.instance.SetIntInternal("grimmChildLevel", 5);
                PlayerData.instance.charmSlots = 11;
            }

            Console.AddLine("Added all charms to inventory");
        }

        [BindableMethod(name = "Increment Kingsoul", category = "Charms")]
        public static void IncreaseKingsoulLevel()
        {
            if (!PlayerData.instance.gotCharm_36)
            {
                PlayerData.instance.gotCharm_36 = true;
            }

            PlayerData.instance.royalCharmState++;

            PlayerData.instance.gotShadeCharm = PlayerData.instance.royalCharmState == 4;

            if (PlayerData.instance.royalCharmState >= 5)
            {
                PlayerData.instance.royalCharmState = 0;
            }
        }

        [BindableMethod(name = "Fix Fragile Heart", category = "Charms")]
        public static void FixFragileHeart()
        {
            if (PlayerData.instance.brokenCharm_23)
            {
                PlayerData.instance.brokenCharm_23 = false;
                Console.AddLine("Fixed fragile heart");
            }
        }

        [BindableMethod(name = "Fix Fragile Greed", category = "Charms")]
        public static void FixFragileGreed()
        {
            if (PlayerData.instance.brokenCharm_24)
            {
                PlayerData.instance.brokenCharm_24 = false;
                Console.AddLine("Fixed fragile greed");
            }
        }

        [BindableMethod(name = "Fix Fragile Strength", category = "Charms")]
        public static void FixFragileStrength()
        {
            if (PlayerData.instance.brokenCharm_25)
            {
                PlayerData.instance.brokenCharm_25 = false;
                Console.AddLine("Fixed fragile strength");
            }
        }

        [BindableMethod(name = "Overcharm", category = "Charms")]
        public static void ToggleOvercharm()
        {
            PlayerData.instance.canOvercharm = true;
            PlayerData.instance.overcharmed = !PlayerData.instance.overcharmed;

            Console.AddLine("Set overcharmed: " + PlayerData.instance.overcharmed);
        }

        [BindableMethod(name = "Increment Grimmchild", category = "Charms")]
        public static void IncreaseGrimmchildLevel()
        {
            if (!DebugMod.GrimmTroupe())
            {
                Console.AddLine("Grimmchild does not exist on this patch");
                return;
            }

            if (!PlayerData.instance.GetBoolInternal("gotCharm_40"))
            {
                PlayerData.instance.SetBoolInternal("gotCharm_40", true);
            }

            PlayerData.instance.SetIntInternal("grimmChildLevel", PlayerData.instance.GetIntInternal("grimmChildLevel") + 1);

            if (PlayerData.instance.GetIntInternal("grimmChildLevel") >= 6)
            {
                PlayerData.instance.SetIntInternal("grimmChildLevel", 0);
            }
        }

        #endregion

        #region Skills

        [BindableMethod(name = "Give All", category = "Skills")]
        public static void GiveAllSkills()
        {
            PlayerData.instance.screamLevel = 2;
            PlayerData.instance.fireballLevel = 2;
            PlayerData.instance.quakeLevel = 2;

            PlayerData.instance.hasDash = true;
            PlayerData.instance.canDash = true;
            PlayerData.instance.hasShadowDash = true;
            PlayerData.instance.canShadowDash = true;
            PlayerData.instance.hasWalljump = true;
            PlayerData.instance.canWallJump = true;
            PlayerData.instance.hasDoubleJump = true;
            PlayerData.instance.hasSuperDash = true;
            PlayerData.instance.canSuperDash = true;
            PlayerData.instance.hasAcidArmour = true;

            PlayerData.instance.hasDreamNail = true;
            PlayerData.instance.dreamNailUpgraded = true;
            PlayerData.instance.hasDreamGate = true;

            PlayerData.instance.hasNailArt = true;
            PlayerData.instance.hasCyclone = true;
            PlayerData.instance.hasDashSlash = true;
            PlayerData.instance.hasUpwardSlash = true;

            Console.AddLine("Giving player all skills");
        }

        [BindableMethod(name = "Increment Dash", category = "Skills")]
        public static void ToggleMothwingCloak()
        {
            if (!PlayerData.instance.hasDash && !PlayerData.instance.hasShadowDash)
            {
                PlayerData.instance.hasDash = true;
                PlayerData.instance.canDash = true;
                Console.AddLine("Giving player Mothwing Cloak");
            }
            else if (PlayerData.instance.hasDash && !PlayerData.instance.hasShadowDash)
            {
                PlayerData.instance.hasShadowDash = true;
                PlayerData.instance.canShadowDash = true;
                Console.AddLine("Giving player Shade Cloak");
            }
            else
            {
                PlayerData.instance.hasDash = false;
                PlayerData.instance.canDash = false;
                PlayerData.instance.hasShadowDash = false;
                PlayerData.instance.canShadowDash = false;
                Console.AddLine("Taking away both dash upgrades");
            }
        }

        [BindableMethod(name = "Give Mantis Claw", category = "Skills")]
        public static void ToggleMantisClaw()
        {
            if (!PlayerData.instance.hasWalljump)
            {
                PlayerData.instance.hasWalljump = true;
                PlayerData.instance.canWallJump = true;
                Console.AddLine("Giving player Mantis Claw");
            }
            else
            {
                PlayerData.instance.hasWalljump = false;
                PlayerData.instance.canWallJump = false;
                Console.AddLine("Taking away Mantis Claw");
            }
        }

        [BindableMethod(name = "Give Monarch Wings", category = "Skills")]
        public static void ToggleMonarchWings()
        {
            if (!PlayerData.instance.hasDoubleJump)
            {
                PlayerData.instance.hasDoubleJump = true;
                Console.AddLine("Giving player Monarch Wings");
            }
            else
            {
                PlayerData.instance.hasDoubleJump = false;
                Console.AddLine("Taking away Monarch Wings");
            }
        }

        [BindableMethod(name = "Give Crystal Heart", category = "Skills")]
        public static void ToggleCrystalHeart()
        {
            if (!PlayerData.instance.hasSuperDash)
            {
                PlayerData.instance.hasSuperDash = true;
                PlayerData.instance.canSuperDash = true;
                Console.AddLine("Giving player Crystal Heart");
            }
            else
            {
                PlayerData.instance.hasSuperDash = false;
                PlayerData.instance.canSuperDash = false;
                Console.AddLine("Taking away Crystal Heart");
            }
        }

        [BindableMethod(name = "Give Isma's Tear", category = "Skills")]
        public static void ToggleIsmasTear()
        {
            if (!PlayerData.instance.hasAcidArmour)
            {
                PlayerData.instance.hasAcidArmour = true;
                Console.AddLine("Giving player Isma's Tear");
            }
            else
            {
                PlayerData.instance.hasAcidArmour = false;
                Console.AddLine("Taking away Isma's Tear");
            }
        }

        [BindableMethod(name = "Give Dream Nail", category = "Skills")]
        public static void ToggleDreamNail()
        {
            if (!PlayerData.instance.hasDreamNail && !PlayerData.instance.dreamNailUpgraded)
            {
                PlayerData.instance.hasDreamNail = true;
                Console.AddLine("Giving player Dream Nail");
            }
            else if (PlayerData.instance.hasDreamNail && !PlayerData.instance.dreamNailUpgraded)
            {
                PlayerData.instance.dreamNailUpgraded = true;
                Console.AddLine("Giving player Awoken Dream Nail");
            }
            else
            {
                PlayerData.instance.hasDreamNail = false;
                PlayerData.instance.dreamNailUpgraded = false;
                Console.AddLine("Taking away both Dream Nail upgrades");
            }
        }

        [BindableMethod(name = "Give Dream Gate", category = "Skills")]
        public static void ToggleDreamGate()
        {
            if (!PlayerData.instance.hasDreamNail && !PlayerData.instance.hasDreamGate)
            {
                PlayerData.instance.hasDreamNail = true;
                PlayerData.instance.hasDreamGate = true;
                FSMUtility.LocateFSM(DebugMod.RefKnight, "Dream Nail").FsmVariables.GetFsmBool("Dream Warp Allowed").Value = true;
                Console.AddLine("Giving player both Dream Nail and Dream Gate");
            }
            else if (PlayerData.instance.hasDreamNail && !PlayerData.instance.hasDreamGate)
            {
                PlayerData.instance.hasDreamGate = true;
                FSMUtility.LocateFSM(DebugMod.RefKnight, "Dream Nail").FsmVariables.GetFsmBool("Dream Warp Allowed").Value = true;
                Console.AddLine("Giving player Dream Gate");
            }
            else
            {
                PlayerData.instance.hasDreamGate = false;
                FSMUtility.LocateFSM(DebugMod.RefKnight, "Dream Nail").FsmVariables.GetFsmBool("Dream Warp Allowed").Value = false;
                Console.AddLine("Taking away Dream Gate");
            }
        }

        [BindableMethod(name = "Give Great Slash", category = "Skills")]
        public static void ToggleGreatSlash()
        {
            if (!PlayerData.instance.hasDashSlash)
            {
                PlayerData.instance.hasDashSlash = true;
                PlayerData.instance.hasNailArt = true;
                Console.AddLine("Giving player Great Slash");
            }
            else
            {
                PlayerData.instance.hasDashSlash = false;
                Console.AddLine("Taking away Great Slash");
            }

            if (!PlayerData.instance.hasUpwardSlash && !PlayerData.instance.hasDashSlash && !PlayerData.instance.hasCyclone) PlayerData.instance.hasNailArt = false;
        }

        [BindableMethod(name = "Give Dash Slash", category = "Skills")]
        public static void ToggleDashSlash()
        {
            if (!PlayerData.instance.hasUpwardSlash)
            {
                PlayerData.instance.hasUpwardSlash = true;
                PlayerData.instance.hasNailArt = true;
                Console.AddLine("Giving player Dash Slash");
            }
            else
            {
                PlayerData.instance.hasUpwardSlash = false;
                Console.AddLine("Taking away Dash Slash");
            }

            if (!PlayerData.instance.hasUpwardSlash && !PlayerData.instance.hasDashSlash && !PlayerData.instance.hasCyclone) PlayerData.instance.hasNailArt = false;
        }

        [BindableMethod(name = "Give Cyclone Slash", category = "Skills")]
        public static void ToggleCycloneSlash()
        {
            if (!PlayerData.instance.hasCyclone)
            {
                PlayerData.instance.hasCyclone = true;
                PlayerData.instance.hasNailArt = true;
                Console.AddLine("Giving player Cyclone Slash");
            }
            else
            {
                PlayerData.instance.hasCyclone = false;
                Console.AddLine("Taking away Cyclone Slash");
            }

            if (!PlayerData.instance.hasUpwardSlash && !PlayerData.instance.hasDashSlash && !PlayerData.instance.hasCyclone) PlayerData.instance.hasNailArt = false;
        }

        #endregion

        #region Spells

        [BindableMethod(name = "Increment Scream", category = "Spells")]
        public static void IncreaseScreamLevel()
        {
            if (PlayerData.instance.screamLevel >= 2)
            {
                PlayerData.instance.screamLevel = 0;
            }
            else
            {
                PlayerData.instance.screamLevel++;
            }
        }

        [BindableMethod(name = "Increment Fireball", category = "Spells")]
        public static void IncreaseFireballLevel()
        {
            if (PlayerData.instance.fireballLevel >= 2)
            {
                PlayerData.instance.fireballLevel = 0;
            }
            else
            {
                PlayerData.instance.fireballLevel++;
            }
        }

        [BindableMethod(name = "Increment Quake", category = "Spells")]
        public static void IncreaseQuakeLevel()
        {
            if (PlayerData.instance.quakeLevel >= 2)
            {
                PlayerData.instance.quakeLevel = 0;
            }
            else
            {
                PlayerData.instance.quakeLevel++;
            }
        }

        #endregion

        #region Bosses

        [BindableMethod(name = "Respawn Ghost", category = "Bosses")]
        public static void RespawnGhost()
        {
            BossHandler.RespawnGhost();
        }

        [BindableMethod(name = "Respawn Boss", category = "Bosses")]
        public static void RespawnBoss()
        {
            BossHandler.RespawnBoss();
        }

        [BindableMethod(name = "Respawn Failed Champ", category = "Bosses")]
        public static void ToggleFailedChamp()
        {
            PlayerData.instance.falseKnightDreamDefeated = !PlayerData.instance.falseKnightDreamDefeated;

            Console.AddLine("Set Failed Champion killed: " + PlayerData.instance.falseKnightDreamDefeated);
        }

        [BindableMethod(name = "Respawn Soul Tyrant", category = "Bosses")]
        public static void ToggleSoulTyrant()
        {
            PlayerData.instance.mageLordDreamDefeated = !PlayerData.instance.mageLordDreamDefeated;

            Console.AddLine("Set Soul Tyrant killed: " + PlayerData.instance.mageLordDreamDefeated);
        }

        [BindableMethod(name = "Respawn Lost Kin", category = "Bosses")]
        public static void ToggleLostKin()
        {
            PlayerData.instance.infectedKnightDreamDefeated = !PlayerData.instance.infectedKnightDreamDefeated;

            Console.AddLine("Set Lost Kin killed: " + PlayerData.instance.infectedKnightDreamDefeated);
        }

        [BindableMethod(name = "Respawn NK Grimm", category = "Bosses")]
        public static void ToggleNKGrimm()
        {
            if (!DebugMod.GrimmTroupe())
            {
                Console.AddLine("Nightmare King Grimm does not exist on this patch");
                return;
            }

            if (PlayerData.instance.GetBoolInternal("killedNightmareGrimm") || PlayerData.instance.GetBoolInternal("destroyedNightmareLantern"))
            {
                PlayerData.instance.SetBoolInternal("troupeInTown", true);
                PlayerData.instance.SetBoolInternal("killedNightmareGrimm", false);
                PlayerData.instance.SetBoolInternal("destroyedNightmareLantern", false);
                PlayerData.instance.SetIntInternal("grimmChildLevel", 3);
                PlayerData.instance.SetIntInternal("flamesCollected", 3);
                PlayerData.instance.SetBoolInternal("grimmchildAwoken", false);
                PlayerData.instance.SetBoolInternal("metGrimm", true);
                PlayerData.instance.SetBoolInternal("foughtGrimm", true);
                PlayerData.instance.SetBoolInternal("killedGrimm", true);
            }
            else
            {
                PlayerData.instance.SetBoolInternal("troupeInTown", false);
                PlayerData.instance.SetBoolInternal("killedNightmareGrimm", true);
            }

            Console.AddLine("Set Nightmare King Grimm killed: " + PlayerData.instance.GetBoolInternal("killedNightmareGrimm"));
        }

        #endregion

        #region Items

        [BindableMethod(name = "Give Lantern", category = "Items")]
        public static void ToggleLantern()
        {
            if (!PlayerData.instance.hasLantern)
            {
                PlayerData.instance.hasLantern = true;
                Console.AddLine("Giving player lantern");
            }
            else
            {
                PlayerData.instance.hasLantern = false;
                Console.AddLine("Taking away lantern");
            }
        }

        [BindableMethod(name = "Give Tram Pass", category = "Items")]
        public static void ToggleTramPass()
        {
            if (!PlayerData.instance.hasTramPass)
            {
                PlayerData.instance.hasTramPass = true;
                Console.AddLine("Giving player tram pass");
            }
            else
            {
                PlayerData.instance.hasTramPass = false;
                Console.AddLine("Taking away tram pass");
            }
        }

        [BindableMethod(name = "Give Map & Quill", category = "Items")]
        public static void ToggleMapQuill()
        {
            if (!PlayerData.instance.hasQuill || !PlayerData.instance.hasMap)
            {
                PlayerData.instance.hasQuill = true;
                PlayerData.instance.hasMap = true;
                PlayerData.instance.mapDirtmouth = true;
                Console.AddLine("Giving player map & quill");
            }
            else
            {
                PlayerData.instance.hasQuill = false;
                PlayerData.instance.hasMap = false;
                Console.AddLine("Taking away map & quill");
            }
        }

        [BindableMethod(name = "Give City Crest", category = "Items")]
        public static void ToggleCityKey()
        {
            if (!PlayerData.instance.hasCityKey)
            {
                PlayerData.instance.hasCityKey = true;
                Console.AddLine("Giving player city crest");
            }
            else
            {
                PlayerData.instance.hasCityKey = false;
                Console.AddLine("Taking away city crest");
            }
        }

        [BindableMethod(name = "Give Shopkeeper's Key", category = "Items")]
        public static void ToggleSlyKey()
        {
            if (!PlayerData.instance.hasSlykey)
            {
                PlayerData.instance.hasSlykey = true;
                Console.AddLine("Giving player shopkeeper's key");
            }
            else
            {
                PlayerData.instance.hasSlykey = false;
                Console.AddLine("Taking away shopkeeper's key");
            }
        }

        [BindableMethod(name = "Give Elegant Key", category = "Items")]
        public static void ToggleElegantKey()
        {
            if (!PlayerData.instance.hasWhiteKey)
            {
                PlayerData.instance.hasWhiteKey = true;
                PlayerData.instance.usedWhiteKey = false;
                Console.AddLine("Giving player elegant key");
            }
            else
            {
                PlayerData.instance.hasWhiteKey = false;
                Console.AddLine("Taking away elegant key");
            }
        }

        [BindableMethod(name = "Give Love Key", category = "Items")]
        public static void ToggleLoveKey()
        {
            if (!PlayerData.instance.hasLoveKey)
            {
                PlayerData.instance.hasLoveKey = true;
                Console.AddLine("Giving player love key");
            }
            else
            {
                PlayerData.instance.hasLoveKey = false;
                Console.AddLine("Taking away love key");
            }
        }

        [BindableMethod(name = "Give Kingsbrand", category = "Items")]
        public static void ToggleKingsbrand()
        {
            if (!PlayerData.instance.hasKingsBrand)
            {
                PlayerData.instance.hasKingsBrand = true;
                Console.AddLine("Giving player kingsbrand");
            }
            else
            {
                PlayerData.instance.hasKingsBrand = false;
                Console.AddLine("Taking away kingsbrand");
            }
        }

        [BindableMethod(name = "Give Delicate Flower", category = "Items")]
        public static void ToggleXunFlower()
        {
            if (!PlayerData.instance.hasXunFlower || PlayerData.instance.xunFlowerBroken)
            {
                PlayerData.instance.hasXunFlower = true;
                PlayerData.instance.xunFlowerBroken = false;
                Console.AddLine("Giving player delicate flower");
            }
            else
            {
                PlayerData.instance.hasXunFlower = false;
                Console.AddLine("Taking away delicate flower");
            }
        }

        #endregion

        #region Consumables

        [BindableMethod(name = "Give Pale Ore", category = "Consumables")]
        public static void GivePaleOre()
        {
            PlayerData.instance.ore = 6;
            Console.AddLine("Set player pale ore to 6");
        }

        [BindableMethod(name = "Give Simple Keys", category = "Consumables")]
        public static void GiveSimpleKey()
        {
            PlayerData.instance.simpleKeys = 3;
            Console.AddLine("Set player simple keys to 3");
        }

        [BindableMethod(name = "Give Rancid Eggs", category = "Consumables")]
        public static void GiveRancidEgg()
        {
            PlayerData.instance.rancidEggs += 10;
            Console.AddLine("Giving player 10 rancid eggs");
        }

        [BindableMethod(name = "Give Geo", category = "Consumables")]
        public static void GiveGeo()
        {
            HeroController.instance.AddGeo(1000);
            Console.AddLine("Giving player 1000 geo");
        }

        [BindableMethod(name = "Give Essence", category = "Consumables")]
        public static void GiveEssence()
        {
            PlayerData.instance.dreamOrbs += 100;
            Console.AddLine("Giving player 100 essence");
        }


        #endregion

        #region Dreamgate

        [BindableMethod(name = "Update DG Data", category = "Dreamgate")]
        public static void ReadDGData()
        {
            DreamGate.delMenu = false;
            if (!DreamGate.dataBusy)
            {
                Console.AddLine("Updating DGdata from the file...");
                DreamGate.ReadData(true);
            }
        }

        [BindableMethod(name = "Save DG Data", category = "Dreamgate")]
        public static void SaveDGData()
        {
            DreamGate.delMenu = false;
            if (!DreamGate.dataBusy)
            {
                Console.AddLine("Writing DGdata to the file...");
                DreamGate.WriteData();
            }
        }

        [BindableMethod(name = "Add DG Position", category = "Dreamgate")]
        public static void AddDGPosition()
        {
            DreamGate.delMenu = false;

            string entryName = DebugMod.GM.GetSceneNameString();
            int i = 1;

            if (entryName.Length > 5) entryName = entryName.Substring(0, 5);

            while (DreamGate.dgData.ContainsKey(entryName))
            {
                entryName = DebugMod.GM.GetSceneNameString() + i;
                i++;
            }

            DreamGate.AddEntry(entryName);
        }

        #endregion
        
        #region MovePlayer

        [BindableMethod(name = "Move 0.1 units to the right", category = "PlayerMovement")]
        public static void MoveRight()
        {
            var HeroPos = HeroController.instance.transform.position;
            HeroController.instance.transform.position= new Vector3(HeroPos.x + DebugMod.settings.AmountToMove, HeroPos.y);
            Console.AddLine("Moved player 0.1 units to the right");
        }
        [BindableMethod(name = "Move 0.1 units to the left", category = "PlayerMovement")]
        public static void MoveL()
        {
            var HeroPos = HeroController.instance.transform.position;
            HeroController.instance.transform.position = new Vector3(HeroPos.x - DebugMod.settings.AmountToMove, HeroPos.y);
            Console.AddLine("Moved player 0.1 units to the left");
        }
        [BindableMethod(name = "Move 0.1 units up", category = "PlayerMovement")]
        public static void MoveUp()
        {
            var HeroPos = HeroController.instance.transform.position;
            HeroController.instance.transform.position = new Vector3(HeroPos.x, HeroPos.y +  DebugMod.settings.AmountToMove);
            Console.AddLine("Moved player 0.1 units to the up");
        }
        [BindableMethod(name = "Move 0.1 units down", category = "PlayerMovement")]
        public static void MoveDown()
        {
            var HeroPos = HeroController.instance.transform.position;
            HeroController.instance.transform.position = new Vector3(HeroPos.x, HeroPos.y - DebugMod.settings.AmountToMove);
            Console.AddLine("Moved player 0.1 units to the down");
        }
        
        [BindableMethod(name = "FaceLeft", category = "PlayerMovement")]
        public static void FaceLeft()
        {
            HeroController.instance.FaceLeft();
            Console.AddLine("Made player face left");
        }
        
        [BindableMethod(name = "FaceRight", category = "PlayerMovement")]
        public static void FaceRight()
        {
            HeroController.instance.FaceRight();
            Console.AddLine("Made player face right");
        }
        
        #endregion
        
        #region ExportData
        
        [BindableMethod(name = "SceneData to file", category = "ExportData")]
        public static void SceneDataToFile()
        {
            File.WriteAllText(string.Concat(
                    new object[] { Application.persistentDataPath, "/SceneData.json" }),
                JsonUtility.ToJson(
                    SceneData.instance,
                    prettyPrint: true
                )
            );
        }

        [BindableMethod(name = "PlayerData to file", category = "ExportData")]
        public static void PlayerDataToFile()
        {
            File.WriteAllText(string.Concat(
                    new object[] { Application.persistentDataPath, "/PlayerData.json" }),
                JsonUtility.ToJson(
                    PlayerData.instance,
                    prettyPrint: true
                )
            );
        }
        #endregion
    }

    public class SelfDamage : MonoBehaviour
    {
        
        public void Start()
        {
            GameObject Enemyprefab = DebugMod.PreloadedObjects["Enemy"];
            GameObject Enemy = Instantiate(Enemyprefab);
            Enemy.SetActive(false);
            
            CollisionSide side = HeroController.instance.cState.facingRight ? CollisionSide.right : CollisionSide.left;
            int damageAmount = 1;
            int hazardType = (int)HazardType.NON_HAZARD;

            HeroController.instance.TakeDamage(Enemy,side,damageAmount,hazardType);

            var MyMonoBehaviour = GameManager.instance.gameObject.GetComponent<SelfDamage>();
            if (MyMonoBehaviour != null) Destroy(GameManager.instance.gameObject.GetComponent<SelfDamage>());
        }
        
    }
}
