using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DebugMod.Hitbox;
using DebugMod.MethodHelpers;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using SetSpriteRenderer = On.HutongGames.PlayMaker.Actions.SetSpriteRenderer;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    public static partial class BindableFunctions
    {

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
            Console.AddLine("Shade spawn point toggled " + (ShadeSpawnLocation.EnabledCompass ? "On" : "Off"));
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
            VisualMaskHelper.ToggleVignette();
        }

        [BindableMethod(name = "Deactivate Visual Masks", category = "Visual")]
        public static void DoDeactivateVisualMasks()
        {
            MethodHelpers.VisualMaskHelper.ToggleAllMasks();
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

    }
}