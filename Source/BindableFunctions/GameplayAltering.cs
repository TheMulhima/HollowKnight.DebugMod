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
    public static partial class BindableFunctions
    {
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
    }
}