using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DebugMod.Hitbox;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    public static class BindableFunctions
    {
        private static readonly FieldInfo TimeSlowed = typeof(GameManager).GetField("timeSlowed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo IgnoreUnpause = typeof(UIManager).GetField("ignoreUnpause", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly string keybindFilePath = Path.Combine(DebugMod.settings.ModBaseDirectory, "Keybinds.json");
        
        internal static readonly FieldInfo cameraGameplayScene = typeof(CameraController).GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);

        private static float TimeScaleDuringFrameAdvance = 0f;
        
        #region GamePlayAltering
        [BindableMethod(name = "Nail Damage +4", category = "GamePlay Altering")]
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

        [BindableMethod(name = "Nail Damage -4", category = "GamePlay Altering")]
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
        
        [BindableMethod(name = "Decrease Timescale", category = "GamePlay Altering")]
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
            Time.timeScale = Time.timeScale = Mathf.Round(Time.timeScale * 10 - 1f) / 10f;
            DebugMod.CurrentTimeScale = Time.timeScale;

            DebugMod.TimeScaleActive = Math.Abs(DebugMod.CurrentTimeScale - 1f) > Mathf.Epsilon;

            switch (DebugMod.TimeScaleActive)
            {
                case true when wasTimeScaleActive == false:
                    if (GameManager.instance.GetComponent<TimeScale>() == null) GameManager.instance.gameObject.AddComponent<TimeScale>();
                    break;
                case false when wasTimeScaleActive:
                    if (GameManager.instance.GetComponent<TimeScale>() != null)
                    {
                        Object.Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
                    }

                    break;
            }
            Console.AddLine("New TimeScale value: " + DebugMod.CurrentTimeScale + " Old value: " + oldScale);

        }

        [BindableMethod(name = "Increase Timescale", category = "GamePlay Altering")]
        public static void TimescaleUp()
        {
            if (DebugMod.GM.IsGamePaused())
            {
                Console.AddLine("Cannot change timescale when paused");
                return;
            }
            float oldScale = Time.timeScale;
            bool wasTimeScaleActive = DebugMod.TimeScaleActive;
            Time.timeScale = Time.timeScale = Mathf.Round(Time.timeScale * 10 + 1f) / 10f;
            DebugMod.CurrentTimeScale = Time.timeScale;

            DebugMod.TimeScaleActive = Math.Abs(DebugMod.CurrentTimeScale - 1f) > Mathf.Epsilon;

            switch (DebugMod.TimeScaleActive)
            {
                case true when wasTimeScaleActive == false:
                    if (GameManager.instance.GetComponent<TimeScale>() == null) GameManager.instance.gameObject.AddComponent<TimeScale>();
                    break;
                case false when wasTimeScaleActive:
                    if (GameManager.instance.GetComponent<TimeScale>() != null)
                    {
                        Object.Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
                    }

                    break;
            }
            Console.AddLine("New TimeScale value: " + DebugMod.CurrentTimeScale + " Old value: " + oldScale);
        }

        [BindableMethod(name = "Freeze Game", category = "GamePlay Altering")]
        public static void PauseGameNoUI()
        {
            DebugMod.PauseGameNoUIActive = !DebugMod.PauseGameNoUIActive;

            if (DebugMod.PauseGameNoUIActive)
            {
                Time.timeScale = 0;
                GameCameras.instance.StopCameraShake();
                SetAlwaysShowCursor();
                Console.AddLine("Game was Frozen");
            }
            else
            {
                GameCameras.instance.ResumeCameraShake();
                GameManager.instance.isPaused = false;
                GameManager.instance.ui.SetState(UIState.PLAYING);
                GameManager.instance.SetState(GameState.PLAYING);
                if (HeroController.instance != null) HeroController.instance.UnPause();
                Time.timeScale = DebugMod.CurrentTimeScale;
                GameManager.instance.inputHandler.AllowPause();

                if (!DebugMod.settings.ShowCursorWhileUnpaused)
                {
                    UnsetAlwaysShowCursor();
                }
                
                Console.AddLine("Game was Unfrozen");
            }
        }

        [BindableMethod(name = "Reset settings", category = "GamePlay Altering")]
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
            if (!GC.hudCanvas.gameObject.activeInHierarchy) 
                GC.hudCanvas.gameObject.SetActive(true);
            
            //Hide Hero
            tk2dSprite component = DebugMod.RefKnight.GetComponent<tk2dSprite>();
            color = component.color;  color.a = 1f;
            component.color = color;

            //rest all is self explanatory
            if (GameManager.instance.GetComponent<TimeScale>() != null)
            {
                Object.Destroy(GameManager.instance.gameObject.GetComponent<TimeScale>());
            }
            GC.tk2dCam.ZoomFactor = 1f;
            HC.vignette.enabled = false;
            EnemiesPanel.hitboxes = false;
            EnemiesPanel.hpBars = false;
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

        [BindableMethod(name = "Next Save Page", category = "Savestates")]
        public static void NextStatePage()
        {
            SaveStateManager.currentStateFolder++;
            if (SaveStateManager.currentStateFolder >= SaveStateManager.savePages) { SaveStateManager.currentStateFolder = 0; } //rollback to 0 if higher than max
            SaveStateManager.path = Path.Combine(
                SaveStateManager.saveStatesBaseDirectory,
                SaveStateManager.currentStateFolder.ToString()); //change path
            DebugMod.saveStateManager.RefreshStateMenu(); // update menu
        }

        [BindableMethod(name = "Prev Save Page", category = "Savestates")]
        public static void PrevStatePage()
        {
            SaveStateManager.currentStateFolder--;
            if (SaveStateManager.currentStateFolder < 0) { SaveStateManager.currentStateFolder = SaveStateManager.savePages - 1; } //rollback to max if we go below page 0
            SaveStateManager.path = Path.Combine(
                SaveStateManager.saveStatesBaseDirectory,
                SaveStateManager.currentStateFolder.ToString()); //change path
            DebugMod.saveStateManager.RefreshStateMenu(); // update menu
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
        
        
        #region Misc

        /*[BindableMethod(name = "Nail Damage +4 Temp", category = "Misc")]
        public static void IncreaseNailDamageTemp()
        {
            int num = 4;
            if (DebugMod.NailDamage == 0)
            {
                num = 5;
            }
            DebugMod.NailDamage += num;
            Console.AddLine($"Increased base nailDamage to {DebugMod.NailDamage}");
        }

        [BindableMethod(name = "Nail Damage -4 Temp", category = "Misc")]
        public static void DecreaseNailDamageTemp()
        {
            int num2 = DebugMod.NailDamage - 4;
            if (num2 >= 0)
            {
                DebugMod.NailDamage = num2;
                Console.AddLine($"Decreased base nailDamage to {DebugMod.NailDamage}");
            }
            else
            {
                Console.AddLine("Cannot set base nailDamage less than 0 therefore forcing 0 value");
                DebugMod.NailDamage = 0;
            }
        }*/

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
                    InputHandler.Instance.StartCoroutine(GameManager.instance.PauseGameToggle());
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

        [BindableMethod(name = "Toggle Infected Crossroads", category = "Misc")]
        public static void ToggleInfection()
        {
            PlayerData.instance.crossroadsInfected = !PlayerData.instance.crossroadsInfected;
            Console.AddLine($"Crossroads are now " + (PlayerData.instance.crossroadsInfected ? "enabled" : "disabled"));
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
        [BindableMethod(name = "Clear White Screen", category = "Misc")]
        public static void ClearWhiteScreen()
        {
            GameObject.Find("Blanker White").LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
            HeroController.instance.EnableRenderer();
        }
        
        internal static Action ClearSceneDataHook;
        [BindableMethod(name = "Refresh Scene Data", category = "Misc")]
        public static void HookResetCurrentScene()
        {
            string scene = GameManager.instance.sceneName;
            Console.AddLine("Clearing scene data from this scene, re-enter scene or warp");
            ClearSceneDataHook?.Invoke();
            On.GameManager.SaveLevelState += GameManager_SaveLevelState;
            ClearSceneDataHook = () => On.GameManager.SaveLevelState -= GameManager_SaveLevelState;

            void GameManager_SaveLevelState(On.GameManager.orig_SaveLevelState orig, GameManager self)
            {
                orig(self);
                SceneData.instance.persistentBoolItems = SceneData.instance.persistentBoolItems.Where(x => x.sceneName != scene).ToList();
                SceneData.instance.persistentIntItems = SceneData.instance.persistentIntItems.Where(x => x.sceneName != scene).ToList();
                SceneData.instance.geoRocks = SceneData.instance.geoRocks.Where(x => x.sceneName != scene).ToList();

                On.GameManager.SaveLevelState -= GameManager_SaveLevelState;
                ClearSceneDataHook = null;
            }
        }
        [BindableMethod(name = "Block Scene Data Changes", category = "Misc")]
        public static void HookBlockCurrentSceneChanges()
        {
            Console.AddLine("Scene data changes made since entering this scene will not be saved");
            ClearSceneDataHook?.Invoke();
            On.GameManager.SaveLevelState += GameManager_BlockLevelChanges;
            ClearSceneDataHook = () => On.GameManager.SaveLevelState -= GameManager_BlockLevelChanges;

            void GameManager_BlockLevelChanges(On.GameManager.orig_SaveLevelState orig, GameManager self)
            {
                On.GameManager.SaveLevelState -= GameManager_BlockLevelChanges;
                ClearSceneDataHook = null;
            }
        }
        
        [BindableMethod(name = "Recover Shade", category = "Misc")]
        public static void RecoverShade()
        {
            PlayerData.instance.EndSoulLimiter();
            if (PlayerData.instance.geoPool > 0)
            {
                HeroController.instance.AddGeo(PlayerData.instance.geoPool);
                PlayerData.instance.geoPool = 0;
            }
            PlayerData.instance.shadeScene = "None";
            foreach (PlayMakerFSM fsm in GameCameras.instance.hudCanvas.transform.Find("Soul Orb").GetComponentsInChildren<PlayMakerFSM>())
            {
                fsm.SendEvent("SOUL LIMITER DOWN");
            }
            PlayMakerFSM.BroadcastEvent("HOLLOW SHADE KILLED");
        }
        
        [BindableMethod(name = "Start/End Frame Advance", category = "Misc")]
        public static void ToggleFrameAdvance()
        {
            if (Time.timeScale !=0)
            {
                if (GameManager.instance.GetComponent<TimeScale>() == null) GameManager.instance.gameObject.AddComponent<TimeScale>();
                Time.timeScale = 0f;
                TimeScaleDuringFrameAdvance = DebugMod.CurrentTimeScale;
                DebugMod.CurrentTimeScale = 0;
                DebugMod.TimeScaleActive = true;
                Console.AddLine("Starting frame by frame advance on keybind press");
            }
            else
            {
                DebugMod.CurrentTimeScale = TimeScaleDuringFrameAdvance;
                Time.timeScale = DebugMod.CurrentTimeScale;
                Console.AddLine("Stopping frame by frame advance on keybind press");
            }
        }

        [BindableMethod(name = "Advance Frame", category = "Misc")]
        public static void AdvanceFrame()
        {
            if (Time.timeScale != 0) ToggleFrameAdvance();
            GameManager.instance.StartCoroutine(AdvanceMyFrame());
        }

        private static IEnumerator AdvanceMyFrame()
        {
            Time.timeScale = 1f;
            yield return new WaitForFixedUpdate();
            Time.timeScale = 0;
        }

        #endregion
        
        #region Visual

        [BindableMethod(name = "Show Hitboxes", category = "Visual")]
        public static void ShowHitboxes()
        {
            if (++DebugMod.settings.ShowHitBoxes > 2) DebugMod.settings.ShowHitBoxes = 0;
            Console.AddLine("Toggled show hitboxes: " + DebugMod.settings.ShowHitBoxes);
        }
        
        [BindableMethod(name = "Shade Spawn Points", category = "Visual")]
        public static void ShadeSpawnPoint()
        {
            if (DebugMod.HC == null)
            {
                Console.AddLine("Player isn't in scene. How did you reach here?");
                return;
            }
            
            var component = HeroController.instance.gameObject.GetComponent<ShadeSpawnLocation>();
            if (component == null) HeroController.instance.gameObject.AddComponent<ShadeSpawnLocation>();
            //not gonna delete component if disabled to not break something if someone spams

            ShadeSpawnLocation.EnabledCompass = !ShadeSpawnLocation.EnabledCompass;
            Console.AddLine("Shade spawn point toggled " +  (ShadeSpawnLocation.EnabledCompass ? "On" : "Off"));
        }
        
        [BindableMethod(name = "Shade Retreat Border", category = "Visual")]
        public static void ShowShadeRetreatBorder()
        {
            if (DebugMod.HC == null)
            {
                Console.AddLine("Player isn't in scene. How did you reach here?");
                return;
            }
            
            var component = HeroController.instance.gameObject.GetComponent<ShadeSpawnLocation>();
            if (component == null) HeroController.instance.gameObject.AddComponent<ShadeSpawnLocation>();
            //not gonna delete component if disabled to not break something if someone spams

            if (!ShadeSpawnLocation.EnabledCompass)
            {
                Console.AddLine("Please Enable Shade compass first");
                return;
            }

            if (++ShadeSpawnLocation.ShowShadeRetreatBorder > 2) ShadeSpawnLocation.ShowShadeRetreatBorder = 0;

            string displaytext = ShadeSpawnLocation.ShowShadeRetreatBorder switch
            {
                1 => "Closest",
                2 => "All",
                _ => "None"
            };
            Console.AddLine($"Shade Reach Showing {displaytext}");
        }
        
        [BindableMethod(name = "Toggle Vignette", category = "Visual")]
        public static void ToggleVignette()
        {
            HeroController.instance.vignette.enabled = !HeroController.instance.vignette.enabled;
            Console.AddLine("Vignette toggled " + (HeroController.instance.vignette.enabled ? "On" : "Off"));
        }

        [BindableMethod(name = "Deactivate Visual Masks", category = "Visual")]
        public static void DoDeactivateVisualMasks()
        {
            MethodHelpers.VisualMaskHelper.InvokedBindableFunction();
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

        [BindableMethod(name = "Toggle Cursor", category = "Visual")]
        public static void ToggleAlwaysShowCursor()
        {
            DebugMod.settings.ShowCursorWhileUnpaused = !DebugMod.settings.ShowCursorWhileUnpaused;

            if (DebugMod.settings.ShowCursorWhileUnpaused)
            {
                SetAlwaysShowCursor();
                Console.AddLine("Showing cursor while unpaused");
            }
            else
            {
                UnsetAlwaysShowCursor();
                Console.AddLine("Not showing cursor while unpaused");
            }
        }
        internal static void SetAlwaysShowCursor()
        {
            On.InputHandler.OnGUI -= CursorDisplayActive;
            On.InputHandler.OnGUI += CursorDisplayActive;
            ModHooks.CursorHook -= CursorDisplayActive;
            ModHooks.CursorHook += CursorDisplayActive;
        }
        internal static void UnsetAlwaysShowCursor()
        {
            On.InputHandler.OnGUI -= CursorDisplayActive;
            ModHooks.CursorHook -= CursorDisplayActive;
        }
        private static void CursorDisplayActive(On.InputHandler.orig_OnGUI orig, InputHandler self)
        {
            orig(self);
            Cursor.visible = true;
        }
        private static void CursorDisplayActive()
        {
            Cursor.visible = true;
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
                DebugMod.settings.ConsoleVisible
                );

            DebugMod.settings.InfoPanelVisible = active;
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
            DebugMod.settings.InfoPanelVisible = !DebugMod.settings.InfoPanelVisible;
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

        // View handled in the InfoPanel classes
        [BindableMethod(name = "Info Panel Switch", category = "Mod UI")]
        public static void SwitchActiveInfoPanel()
        {
            InfoPanel.ToggleActivePanel();
        }

        #endregion

        #region Enemies

        //probably should delete this as it has become obsolete becuase of the new Show Hitboxes in the visuals page
        [BindableMethod(name = "Toggle Enemy Hitboxes (Redundant)", category = "Enemy Panel")]
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

            if (EnemiesPanel.hpBars) EnemiesPanel.autoUpdate = true;

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
            //GameManager.instance.gameObject.AddComponent<SelfDamage>();
            HeroController.instance.TakeDamage(new GameObject(),CollisionSide.left,1,(int)HazardType.NON_HAZARD);

            Console.AddLine("Attempting self damage");
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
            foreach (GameObject go in USceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (HealthManager hm in go.GetComponentsInChildren<HealthManager>())
                {
                    hm.Die(null, AttackTypes.Generic, true);
                }
            }
            Console.AddLine("Killing all Healthmanagers in scene!");
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
                if (DebugMod.RefKnight.LocateMyFSM("Surface Water").ActiveStateName != "Inactive")
                {
                    DebugMod.RefKnight.LocateMyFSM("Surface Water").SendEvent("JUMP");
                }
                Console.AddLine("Disabled noclip");
            }
        }

        [BindableMethod(name = "Toggle Hero Collider", category = "Cheats")]
        public static void ToggleHeroCollider()
        {
            if (!DebugMod.RefHeroCollider.enabled)
            {
                DebugMod.RefHeroCollider.enabled = true;
                DebugMod.RefHeroBox.enabled = true;
                Console.AddLine("Enabled hero collider" + (DebugMod.noclip ? " and disabled noclip" : ""));
                DebugMod.noclip = false;
            }
            else
            {
                DebugMod.RefHeroCollider.enabled = false;
                DebugMod.RefHeroBox.enabled = false;
                Console.AddLine("Disabled hero collider" + (DebugMod.noclip ? "" : " and enabled noclip"));
                DebugMod.noclip = true;
                DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
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

        [BindableMethod(name = "Toggle Bench Storage", category = "Cheats")]
        public static void ToggleBenchStorage()
        {
            PlayerData.instance.atBench = !PlayerData.instance.atBench;
            Console.AddLine($"{(PlayerData.instance.atBench ? "Given" : "Taken away")} bench storage");
        }

        [BindableMethod(name = "Toggle Collision", category = "Cheats")]
        public static void ToggleCollision()
        {
            var rb2d = HeroController.instance.GetComponent<Rigidbody2D>();
            rb2d.isKinematic = !rb2d.isKinematic;
            Console.AddLine($"{(rb2d.isKinematic ? "Enabled" : "Disabled")} collision");
        }

        [BindableMethod(name = "Dreamgate Invulnerability", category = "Cheats")]
        public static void GiveDgateInvuln()
        {
            PlayerData.instance.isInvincible = true;
            Object.FindObjectOfType<HeroBox>().gameObject.SetActive(false);
            HeroController.instance.gameObject.LocateMyFSM("Roar Lock").FsmVariables.FindFsmBool("No Roar").Value =
                true;
            Console.AddLine("Given dreamgate invulnerability");
        }
        
        #endregion

        #region Charms

        private static void UpdateCharms()
        {
            PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
            PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            EventRegister.SendEvent("CHARM EQUIP CHECK");
            EventRegister.SendEvent("CHARM INDICATOR CHECK");
        }

        [BindableMethod(name = "Give All Charms", category = "Charms")]
        public static void GiveAllCharms()
        {
            for (int i = 1; i <= 40; i++)
            {
                PlayerData.instance.SetBoolInternal("gotCharm_" + i, true);
            }

            PlayerData.instance.charmSlots = 10;
            PlayerData.instance.hasCharm = true;
            PlayerData.instance.charmsOwned = 40;
            PlayerData.instance.royalCharmState = 4;
            PlayerData.instance.gotShadeCharm = true;
            PlayerData.instance.charmCost_36 = 0;
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
            PlayerData.instance.fragileGreed_unbreakable = true;
            PlayerData.instance.fragileStrength_unbreakable = true;
            PlayerData.instance.fragileHealth_unbreakable = true;
            PlayerData.instance.grimmChildLevel = 5;
            PlayerData.instance.charmCost_40 = 3;
            PlayerData.instance.charmSlots = 11;
            UpdateCharms();

            Console.AddLine("Added all charms to inventory");
        }

        [BindableMethod(name = "Remove All Charms", category = "Charms")]
        public static void RemoveAllCharms()
        {
            for (int i = 1; i <= 40; i++)
            {
                PlayerData.instance.SetBoolInternal("gotCharm_" + i, false);
                PlayerData.instance.SetBoolInternal("equippedCharm_" + i, false);
            }

            PlayerData.instance.charmSlots = 3;
            PlayerData.instance.hasCharm = false;
            PlayerData.instance.charmsOwned = 0;
            PlayerData.instance.royalCharmState = 0;
            PlayerData.instance.gotShadeCharm = false;
            PlayerData.instance.gotKingFragment = false;
            PlayerData.instance.gotQueenFragment = false;
            PlayerData.instance.notchShroomOgres = false;
            PlayerData.instance.notchFogCanyon = false;
            PlayerData.instance.colosseumBronzeOpened = false;
            PlayerData.instance.colosseumBronzeCompleted = false;
            PlayerData.instance.salubraNotch1 = false;
            PlayerData.instance.salubraNotch2 = false;
            PlayerData.instance.salubraNotch3 = false;
            PlayerData.instance.salubraNotch4 = false;
            PlayerData.instance.fragileGreed_unbreakable = true;
            PlayerData.instance.fragileStrength_unbreakable = true;
            PlayerData.instance.fragileHealth_unbreakable = true;
            PlayerData.instance.grimmChildLevel = 5;
            PlayerData.instance.charmCost_40 = 2;
            PlayerData.instance.charmSlots = 3;
            PlayerData.instance.equippedCharms.Clear();



            UpdateCharms();
            Console.AddLine("Removed all charms from inventory");
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

            PlayerData.instance.charmCost_36 = PlayerData.instance.royalCharmState switch
            {
                3 => 5,
                4 => 0,
                _ => PlayerData.instance.charmCost_36,
            };
            
            UpdateCharms();
        }

        [BindableMethod(name = "Fix Fragile Heart", category = "Charms")]
        public static void FixFragileHeart()
        {
            if (PlayerData.instance.brokenCharm_23)
            {
                PlayerData.instance.brokenCharm_23 = false;
                Console.AddLine("Fixed fragile heart");
                
                PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
                PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            }
        }

        [BindableMethod(name = "Fix Fragile Greed", category = "Charms")]
        public static void FixFragileGreed()
        {
            if (PlayerData.instance.brokenCharm_24)
            {
                PlayerData.instance.brokenCharm_24 = false;
                Console.AddLine("Fixed fragile greed");
                
                UpdateCharms();
            }
        }

        [BindableMethod(name = "Fix Fragile Strength", category = "Charms")]
        public static void FixFragileStrength()
        {
            if (PlayerData.instance.brokenCharm_25)
            {
                PlayerData.instance.brokenCharm_25 = false;
                Console.AddLine("Fixed fragile strength");
                
                UpdateCharms();
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
            if (!PlayerData.instance.gotCharm_40)
            {
                PlayerData.instance.gotCharm_40 = true;
            }

            PlayerData.instance.grimmChildLevel += 1;

            if (PlayerData.instance.grimmChildLevel >= 6)
            {
                PlayerData.instance.grimmChildLevel = 0;
            }
            PlayerData.instance.charmCost_40 = PlayerData.instance.grimmChildLevel switch
            {
                5 => 3,
                _ => 2,
            };

            PlayerData.instance.destroyedNightmareLantern = PlayerData.instance.grimmChildLevel == 5;

            Object.Destroy(GameObject.FindWithTag("Grimmchild"));

            UpdateCharms();

            GameManager.instance.StartCoroutine(SpawnGrimmChild());

            IEnumerator SpawnGrimmChild()
            {
                yield return null;
                yield return null;
                HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild").SendEvent("CHARM EQUIP CHECK");
            }
        }

        [BindableMethod(name = "Add Charm Notch", category = "Charms")]
        public static void AddCharmNotch()
        {
            PlayerData.instance.charmSlots++;
        }
        
        [BindableMethod(name = "Decrease Charm Notch", category = "Charms")]
        public static void DecreaseCharmNotch()
        {
            PlayerData.instance.charmSlots--;
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
                EventRegister.SendEvent("GOT SHADOW DASH");
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
                PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR");
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

        [BindableMethod(name = "Open All Stags", category = "Items")]
        public static void AllStags()
        {
            PlayerData playerData = PlayerData.instance;
            playerData.SetBool("openedTown",true);
            playerData.SetBool("openedTownBuilding", true);
            playerData.SetBool("openedCrossroads", true);
            playerData.SetBool("openedGreenpath", true);
            playerData.SetBool("openedRuins1", true);
            playerData.SetBool("openedRuins2", true);
            playerData.SetBool("openedFungalWastes", true);
            playerData.SetBool("openedRoyalGardens", true);
            playerData.SetBool("openedRestingGrounds", true);
            playerData.SetBool("openedDeepnest", true);
            playerData.SetBool("openedStagNest", true);
            playerData.SetBool("openedHiddenStation", true);
            playerData.SetBool("gladeDoorOpened", true);
            playerData.SetBool("troupeInTown", true);
            
            Console.AddLine("Unlocked all stags");
        }
        
        #endregion

        #region Masks & Vessels
        
        [BindableMethod(name = "Give Mask", category = "Mask & Vessels")]
        public static void GiveMask()
        {
            if (PlayerData.instance.maxHealthBase < 9)
            {
                HeroController.instance.MaxHealth();
                HeroController.instance.AddToMaxHealth(1);
                PlayMakerFSM.BroadcastEvent("MAX HP UP");
                Console.AddLine("Added Mask");
            }
            else
            {
                Console.AddLine("You have the maximum number of masks");
            }
        }
        
        [BindableMethod(name = "Give Vessel", category = "Mask & Vessels")]
        public static void GiveVessel()
        {
            if (PlayerData.instance.MPReserveMax < 99)
            {
                HeroController.instance.AddToMaxMPReserve(33);
                PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
                Console.AddLine("Added Vessel");
            }
            else
            {
                Console.AddLine("You have the maximum number of vessels");
            }
        }
        
        [BindableMethod(name = "Take Away Mask", category = "Mask & Vessels")]
        public static void TakeAwayMask()
        {
            if (PlayerData.instance.maxHealthBase > 1)
            {
                PlayerData.instance.maxHealth -= 1;
                PlayerData.instance.maxHealthBase -= 1;
                if (!GameCameras.instance.hudCanvas.gameObject.activeInHierarchy)
                    GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                else
                {
                    GameCameras.instance.hudCanvas.gameObject.SetActive(false);
                    GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                }
                Console.AddLine("Took Away Mask");
            }
            else
            {
                Console.AddLine("You have the minimum number of masks");
            }
        }
        
        [BindableMethod(name = "Take Away Vessel", category = "Mask & Vessels")]
        public static void TakeAwayVessel()
        {
            if (PlayerData.instance.MPReserveMax > 0)
            {
                PlayerData.instance.MPReserveMax -= 33;
                if (!GameCameras.instance.hudCanvas.gameObject.activeInHierarchy)
                    GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                else
                {
                    GameCameras.instance.hudCanvas.gameObject.SetActive(false);
                    GameCameras.instance.hudCanvas.gameObject.SetActive(true);
                }
                Console.AddLine("Removed Vessel");
            }
            else
            {
                Console.AddLine("You have the minimum number of vessels");
            }
        }

        
        [BindableMethod(name = "Add Health", category = "Mask & Vessels")]
        public static void AddHealth()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Unacceptable conditions for adding health" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            HeroController.instance.AddHealth(1);

            Console.AddLine("Added Heaalth");
        }
        [BindableMethod(name = "Take Health", category = "Mask & Vessels")]
        public static void TakeHealth()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene())
            {
                Console.AddLine("Unacceptable conditions for taking health" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            HeroController.instance.TakeHealth(1);

            Console.AddLine("Attempting to take health");
        }
        
        [BindableMethod(name = "Add Soul", category = "Mask & Vessels")]
        public static void AddSoul()
        {
            HeroController.instance.AddMPCharge(33);

            Console.AddLine("Added Heaalth");
        }
        [BindableMethod(name = "Take Soul", category = "Mask & Vessels")]
        public static void TakeSoul()
        {
            HeroController.instance.TakeMP(33);

            Console.AddLine("Attempting to take health");
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
        
        //yes i added dreamers here because its funny
        [BindableMethod(name = "Give Lurien", category = "Consumables")]
        public static void GiveLurien()
        {
            if (!PlayerData.instance.lurienDefeated)
            {
                PlayerData.instance.lurienDefeated = true;
                MakeDreamersWork("WATCHER");
                
            }
            Console.AddLine("Giving Lurien");
        }
        
        [BindableMethod(name = "Give Monomon", category = "Consumables")]
        public static void GiveMonomon()
        {
            if (!PlayerData.instance.monomonDefeated)
            {
                PlayerData.instance.monomonDefeated = true;
                MakeDreamersWork("TEACHER");
            }
            Console.AddLine("Giving Monomon");
        }
        
        [BindableMethod(name = "Give Herrah", category = "Consumables")]
        public static void GiveHerrah()
        {
            if (!PlayerData.instance.hegemolDefeated)
            {
                PlayerData.instance.hegemolDefeated = true;
                MakeDreamersWork("BEAST");
            }
            Console.AddLine("Giving Herrah");
        }

        private static void MakeDreamersWork(string AchievementToGive)
        {
            var pd = PlayerData.instance;
            
            pd.guardiansDefeated += 1;
            if (!pd.crossroadsInfected)
            {
                pd.crossroadsInfected = true;
                pd.visitedCrossroads = false;
            }

            if (pd.guardiansDefeated == 3)
            {
                pd.brettaState += 1;
                pd.mrMushroomState = 1;
                pd.corniferAtHome = true;
                pd.iseldaConvo1 = true;
                pd.dungDefenderSleeping = true;
                pd.corn_crossroadsLeft = true;
                
                pd.corn_fogCanyonLeft = true;
                pd.corn_fungalWastesLeft = true;
                pd.corn_cityLeft = true;
                pd.corn_waterwaysLeft = true;
                pd.corn_minesLeft = true;
                pd.corn_cliffsLeft = true;
                pd.corn_deepnestLeft = true;
                pd.corn_outskirtsLeft = true;
                pd.corn_royalGardensLeft = true;
                pd.corn_abyssLeft = true;
                pd.metIselda = true;
            }
            
            GameManager.instance.AwardAchievement(AchievementToGive);
        }

        [BindableMethod(name = "Add GrimmKin Flames", category = "Consumables")]
        public static void GrimmKinFlames()
        {
            if (PlayerData.instance.flamesCollected == 3) PlayerData.instance.flamesCollected = 0;
            else PlayerData.instance.flamesCollected += 1;
            Console.AddLine("Grimm kin flames incremented");
        } 
        
        
        private static string[] AllMapBools = new[]
        {
            "mapCrossroads",
            "mapGreenpath",
            "mapFogCanyon",
            "mapRoyalGardens",
            "mapFungalWastes",
            "mapCity",
            "mapWaterways",
            "mapMines",
            "mapDeepnest",
            "mapCliffs",
            "mapOutskirts",
            "mapRestingGrounds",
            "mapAbyss"
        };
        
        [BindableMethod(name = "Give All Maps", category = "Consumables")]
        public static void GiveAllMaps()
        {
            PlayerData.instance.hasMap = true;
            PlayerData.instance.mapAllRooms = true;
            PlayerData.instance.mapCrossroads = true;
            PlayerData.instance.mapGreenpath = true;
            PlayerData.instance.mapFogCanyon = true;
            PlayerData.instance.mapRoyalGardens = true;
            PlayerData.instance.mapFungalWastes = true;
            PlayerData.instance.mapCity = true;
            PlayerData.instance.mapWaterways = true;
            PlayerData.instance.mapMines = true;
            PlayerData.instance.mapDeepnest = true;
            PlayerData.instance.mapCliffs = true;
            PlayerData.instance.mapOutskirts = true;
            PlayerData.instance.mapRestingGrounds = true;
            PlayerData.instance.mapAbyss = true;
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
            File.WriteAllText(Path.Combine(DebugMod.settings.ModBaseDirectory, "SceneData.json"),
                JsonUtility.ToJson(
                    SceneData.instance,
                    prettyPrint: true
                )
            );
        }

        [BindableMethod(name = "PlayerData to file", category = "ExportData")]
        public static void PlayerDataToFile()
        {
            File.WriteAllText(Path.Combine(DebugMod.settings.ModBaseDirectory, "PlayerData.json"),
                JsonUtility.ToJson(
                    PlayerData.instance,
                    prettyPrint: true
                )
            );
        }
        
        /*[BindableMethod(name = "List all GameObjects in Scene", category = "ExportData")]
        public static void Dump()
        {
            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                DebugMod.instance.Log(go.name);
            }
        }*/
        
        [BindableMethod(name = "Save Key Binds To File", category = "ExportData")]
        public static void GenerateKeyBindToFile()
        {
            var DictToSave = new KeyBinds
            {
                binds_to_file = Convert_Int_To_String_Dict(DebugMod.settings.binds)
            };

            File.WriteAllText(keybindFilePath,JsonConvert.SerializeObject(DictToSave));
            
            Console.AddLine("Keybind File Created");
        }
        
        [BindableMethod(name = "Load Key Binds From File", category = "ExportData")]
        public static void LoadKeyBindFromFile()
        {
            if (!File.Exists(keybindFilePath))
            {
                Console.AddLine("Keybind file not found. Please generate one first");
                return;
            }
            var NewDict = JsonConvert.DeserializeObject<KeyBinds>(File.ReadAllText(keybindFilePath))?.binds_to_file;

            DebugMod.settings.binds = Convert_String_To_Int_Dict(NewDict);
            
            KeyBindPanel.UpdateHelpText();


            Console.AddLine("New Keybinds loaded");
        }

        private static Dictionary<string,string> Convert_Int_To_String_Dict(Dictionary<string,int> IntDict)
        {
            var NewDict = new Dictionary<string, string>();
            foreach (var bind in IntDict)
            {
                KeyCode Newvalue = (KeyCode) bind.Value;
                NewDict.Add(bind.Key, Newvalue.ToString());
            }
            return NewDict;
        }
        private static Dictionary<string,int> Convert_String_To_Int_Dict(Dictionary<string,string> StringDict)
        {
            var NewDict = new Dictionary<string, int>();
            foreach (var bind in StringDict)
            {
                NewDict.Add(bind.Key, (int) Enum.Parse(typeof(KeyCode), bind.Value, true));
            }

            return NewDict;
        }

        #endregion
        
    }
}
