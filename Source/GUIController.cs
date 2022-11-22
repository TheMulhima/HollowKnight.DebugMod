using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DebugMod.Hitbox;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod
{
    public class GUIController : MonoBehaviour
    {
        public Font trajanBold;
        public Font trajanNormal;
        public Font arial;
        public Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();
        public Vector3 hazardLocation;
        public string respawnSceneWatch;
        public static bool didInput, inputEsc;
        private static readonly HitboxViewer hitboxes = new();

        public GameObject canvas;
        private static GUIController _instance;

        /// <summary>
        /// If this returns true, all DebugMod UI elements will be hidden.
        /// </summary>
        public static bool ForceHideUI()
        {
            return DebugMod.GM.IsNonGameplayScene()
                && !DebugMod.GM.IsCinematicScene(); // Show UI in cutscenes
        }

        public void Awake()
        {
            hazardLocation = PlayerData.instance.hazardRespawnLocation;
            respawnSceneWatch = PlayerData.instance.respawnScene;
        }

        public void BuildMenus()
        {
            LoadResources();

            canvas = new GameObject();
            canvas.AddComponent<UnityEngine.Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvas.AddComponent<GraphicRaycaster>();

            SaveStatesPanel.BuildMenu(canvas);
            TopMenu.BuildMenu(canvas);
            EnemiesPanel.BuildMenu(canvas);
            Console.BuildMenu(canvas);

            Modding.ModHooks.FinishedLoadingModsHook += () => InfoPanel.BuildInfoPanels(canvas);
            Modding.ModHooks.FinishedLoadingModsHook += () => KeyBindPanel.BuildMenu(canvas);

            DontDestroyOnLoad(canvas);
        }

        private void LoadResources()
        {
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == "TrajanPro-Bold")
                {
                    trajanBold = f;
                }

                if (f != null && f.name == "TrajanPro-Regular")
                {
                    trajanNormal = f;
                }

                //Just in case for some reason the computer doesn't have arial
                if (f != null && f.name == "Perpetua")
                {
                    arial = f;
                }

                foreach (string font in Font.GetOSInstalledFontNames())
                {
                    if (font.ToLower().Contains("arial"))
                    {
                        arial = Font.CreateDynamicFontFromOSFont(font, 13);
                        break;
                    }
                }
            }

            if (trajanBold == null || trajanNormal == null || arial == null) DebugMod.instance.LogError("Could not find game fonts");

            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (string res in resourceNames)
            {
                if (res.StartsWith("DebugMod.Images."))
                {
                    try
                    {
                        Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                        byte[] buffer = new byte[imageStream.Length];
                        imageStream.Read(buffer, 0, buffer.Length);

                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(buffer.ToArray());

                        string[] split = res.Split('.');
                        string internalName = split[split.Length - 2];
                        images.Add(internalName, tex);

                        DebugMod.instance.LogDebug("Loaded image: " + internalName);
                    }
                    catch (Exception e)
                    {
                        DebugMod.instance.LogError("Failed to load image: " + res + "\n" + e.ToString());
                    }
                }
            }
        }

        public void Update()
        {
            if (DebugMod.GM == null) return;
            
            SaveStatesPanel.Update();
            TopMenu.Update();
            EnemiesPanel.Update();
            Console.Update();
            KeyBindPanel.Update();
            InfoPanel.Update();
            
            if (DebugMod.GetSceneName() == "Menu_Title") return;
            
            // If the mouse is visible, then make sure it can be used.
            // Normally, allowMouseInput is false until first pause and then true from then on (even when not paused)
            if (DebugMod.settings.ShowCursorWhileUnpaused && !ForceHideUI() && !UIManager.instance.inputModule.allowMouseInput)
            {
                InputHandler.Instance.StartUIInput();
            }

            //Handle keybinds
            foreach (var (bindName, bindKeyCode) in DebugMod.settings.binds)
            {
                if (DebugMod.bindMethods.ContainsKey(bindName) || DebugMod.AdditionalBindMethods.ContainsKey(bindName))
                {
                    if (bindKeyCode == KeyCode.None)
                    {
                        foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
                        {
                            if (Input.GetKeyDown(kc) && kc != KeyCode.Mouse0)
                            {
                                // Fix UX
                                if (KeyBindPanel.keyWarning != kc)
                                {
                                    foreach (KeyValuePair<string, KeyCode> kvp in DebugMod.settings.binds)
                                    {
                                        if (kvp.Value == kc)
                                        {
                                            Console.AddLine(kc.ToString() + " already bound to " + kvp.Key +
                                                            ", press again to confirm");
                                            KeyBindPanel.keyWarning = kc;
                                        }
                                    }

                                    if (KeyBindPanel.keyWarning == kc) break;
                                }

                                KeyBindPanel.keyWarning = KeyCode.None;

                                //remove bind
                                if (kc == KeyCode.Escape)
                                {
                                    DebugMod.settings.binds.Remove(bindName);
                                    DebugMod.instance.LogWarn($"The key {Enum.GetName(typeof(KeyCode),kc)} has been unbound from {bindName}");
                                }
                                else if (kc != KeyCode.Escape)
                                {
                                    DebugMod.settings.binds[bindName] = kc;
                                }

                                KeyBindPanel.UpdateHelpText();
                                break;
                            }
                        }
                    }
                    else if (Input.GetKeyDown(bindKeyCode))
                    {
                        //This makes sure atleast you can close the UI when the KeyBindLock is active.
                        //Im sure theres a better way to do this but idk. 
                        try
                        {
                            //cat, allowLock, the method
                            (string, bool, Action) methodData;
                            
                            if (DebugMod.bindMethods.TryGetValue(bindName, out methodData) 
                                || DebugMod.AdditionalBindMethods.TryGetValue(bindName, out methodData))
                            {
                                //run if not locked or locked but bind doesnt allow locks
                                if (!DebugMod.KeyBindLock || DebugMod.KeyBindLock && !methodData.Item2)
                                {
                                    methodData.Item3.Invoke();
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            DebugMod.instance.LogError("Error running keybind method " + bindName + ":\n" +
                                                       e.ToString());
                        }
                        
                    }
                }
            }
            if (SaveStateManager.inSelectSlotState && DebugMod.settings.SaveStatePanelVisible)
            {
                foreach (KeyValuePair<KeyCode, int> entry in DebugMod.alphaKeyDict)
                {
                    
                    if (Input.GetKeyDown(entry.Key))
                    {
                        if (DebugMod.alphaKeyDict.TryGetValue(entry.Key, out int keyInt))
                        {
                            // keyInt should be between 0-9
                            SaveStateManager.currentStateSlot = keyInt;
                            didInput = true;
                            break;
                        }
                        else
                        {
                            didInput = inputEsc = true;
                            break;
                        }
                    }
                }
            }

            if (DebugMod.infiniteSoul 
                && PlayerData.instance.MPCharge < PlayerData.instance.maxMP 
                && PlayerData.instance.health > 0 
                && HeroController.instance != null
                && !HeroController.instance.cState.dead
                && GameManager.instance.IsGameplayScene())
            {
                PlayerData.instance.MPCharge = PlayerData.instance.maxMP - 1;
                if (PlayerData.instance.MPReserveMax > 0)
                {
                    PlayerData.instance.MPReserve = PlayerData.instance.MPReserveMax - 1;
                    HeroController.instance.TakeReserveMP(1);
                    HeroController.instance.AddMPChargeSpa(2);
                }
                //HeroController.instance.TakeReserveMP(1);
                HeroController.instance.AddMPChargeSpa(1);
            }

            if (DebugMod.playerInvincible && PlayerData.instance != null)
            {
                PlayerData.instance.isInvincible = true;
            }

            if (DebugMod.noclip)
            {
                if (DebugMod.IH.inputActions.left.IsPressed)
                {
                    DebugMod.noclipPos = new Vector3(DebugMod.noclipPos.x - Time.deltaTime * 20f * DebugMod.settings.NoClipSpeedModifier, DebugMod.noclipPos.y, DebugMod.noclipPos.z);
                }

                if (DebugMod.IH.inputActions.right.IsPressed)
                {
                    DebugMod.noclipPos = new Vector3(DebugMod.noclipPos.x + Time.deltaTime * 20f * DebugMod.settings.NoClipSpeedModifier, DebugMod.noclipPos.y, DebugMod.noclipPos.z);
                }

                if (DebugMod.IH.inputActions.up.IsPressed)
                {
                    DebugMod.noclipPos = new Vector3(DebugMod.noclipPos.x, DebugMod.noclipPos.y + Time.deltaTime * 20f * DebugMod.settings.NoClipSpeedModifier, DebugMod.noclipPos.z);
                }

                if (DebugMod.IH.inputActions.down.IsPressed)
                {
                    DebugMod.noclipPos = new Vector3(DebugMod.noclipPos.x, DebugMod.noclipPos.y - Time.deltaTime * 20f * DebugMod.settings.NoClipSpeedModifier, DebugMod.noclipPos.z);
                }

                if (HeroController.instance.transitionState == GlobalEnums.HeroTransitionState.WAITING_TO_TRANSITION)
                {
                    DebugMod.RefKnight.transform.position = DebugMod.noclipPos;
                }
                else
                {
                    DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
                }
            }

            if (DebugMod.cameraFollow)
            {
                BindableFunctions.cameraGameplayScene.SetValue(DebugMod.RefCamera, false);
                DebugMod.RefCamera.SnapTo(DebugMod.RefKnight.transform.position.x, DebugMod.RefKnight.transform.position.y);
            }

            if (PlayerData.instance.hazardRespawnLocation != hazardLocation)
            {
                hazardLocation = PlayerData.instance.hazardRespawnLocation;
                Console.AddLine("Hazard Respawn location updated: " + hazardLocation.ToString());

                if (DebugMod.settings.EnemiesPanelVisible)
                {
                    EnemiesPanel.EnemyUpdate(200f);
                }
            }
            if (!string.IsNullOrEmpty(respawnSceneWatch) && respawnSceneWatch != PlayerData.instance.respawnScene)
            {
                respawnSceneWatch = PlayerData.instance.respawnScene;
                Console.AddLine(string.Concat(new string[]
                {
                    "Save Respawn updated, new scene: ",
                    PlayerData.instance.respawnScene.ToString(),
                    ", Map Zone: ",
                    GameManager.instance.GetCurrentMapZone(),
                    ", Respawn Marker: ",
                    PlayerData.instance.respawnMarkerName.ToString()
                }));
            }
            if (HitboxViewer.State != DebugMod.settings.ShowHitBoxes)
            {
                if (DebugMod.settings.ShowHitBoxes != 0)
                {
                    hitboxes.Load();
                }
                else if (HitboxViewer.State != 0 && DebugMod.settings.ShowHitBoxes == 0)
                {
                    hitboxes.Unload();
                }
            }
        }

        public static GUIController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindObjectOfType<GUIController>();
                    if (_instance == null)
                    {
                        DebugMod.instance.LogWarn("[DEBUG MOD] Couldn't find GUIController");

                        GameObject GUIObj = new GameObject();
                        _instance = GUIObj.AddComponent<GUIController>();
                        DontDestroyOnLoad(GUIObj);
                    }
                }
                return _instance;
            }
            set
            {
            }
        }
    }
}
