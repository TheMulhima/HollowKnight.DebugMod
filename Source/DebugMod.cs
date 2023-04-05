using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Modding;
using MonoMod.ModInterop;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;
using JetBrains.Annotations;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace DebugMod
{
    public class DebugMod : Mod<SaveSettings, GlobalSettings>
    {
        public override string GetVersion()
        {
            Assembly asm = typeof(DebugMod).Assembly;
            string ver = asm.GetName().Version.ToString();

            using var sha1 = SHA1.Create();
            using FileStream stream = File.OpenRead(asm.Location);

            byte[] hashBytes = sha1.ComputeHash(stream);

            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return $"{ver}-{hash.Substring(0, 6)}";
        }

        private static GameManager _gm;
        private static InputHandler _ih;
        private static HeroController _hc;
        private static GameObject _refKnight;
        private static PlayMakerFSM _refKnightSlash;
        private static CameraController _refCamera;
        private static PlayMakerFSM _refDreamNail;
        private static Collider2D _refHeroCollider;
        private static Collider2D _refHeroBox;

        internal static GameManager GM => _gm != null ? _gm : (_gm = GameManager.instance);
        internal static InputHandler IH => _ih != null ? _ih : (_ih = GM.inputHandler);
        internal static HeroController HC => _hc != null ? _hc : (_hc = HeroController.instance);
        internal static GameObject RefKnight => _refKnight != null ? _refKnight : (_refKnight = HC.gameObject);
        internal static PlayMakerFSM RefKnightSlash => _refKnightSlash != null ? _refKnightSlash : (_refKnightSlash = RefKnight.transform.Find("Attacks/Slash").GetComponent<PlayMakerFSM>());
        internal static CameraController RefCamera => _refCamera != null ? _refCamera : (_refCamera = GM.cameraCtrl);
        internal static PlayMakerFSM RefDreamNail => _refDreamNail != null ? _refDreamNail : (_refDreamNail = FSMUtility.LocateFSM(RefKnight, "Dream Nail"));
        internal static Collider2D RefHeroCollider => _refHeroCollider != null ? _refHeroCollider : (_refHeroCollider = RefKnight.GetComponent<Collider2D>());
        internal static Collider2D RefHeroBox => _refHeroBox != null ? _refHeroBox : (_refHeroBox = RefKnight.transform.Find("HeroBox").GetComponent<Collider2D>());


        internal static DebugMod instance;

        //internal static int NailDamage;

        public static GlobalSettings settings;

        private static float _loadTime;
        private static float _unloadTime;
        private static bool _loadingChar;

        internal static bool infiniteHP;
        internal static bool infiniteSoul;
        internal static bool playerInvincible;
        internal static bool noclip;
        internal static Vector3 noclipPos;
        internal static bool cameraFollow;
        internal static SaveStateManager saveStateManager;
        public static bool KeyBindLock;
        internal static bool TimeScaleActive;
        internal static float CurrentTimeScale = 1f;
        internal static bool PauseGameNoUIActive = false;

        internal static Dictionary<string, (string category, bool allowLock, Action method)> bindMethods = new();
        internal static Dictionary<string, (string category, bool allowLock, Action method)> AdditionalBindMethods = new();

        internal static Dictionary<KeyCode, int> alphaKeyDict = new Dictionary<KeyCode, int>();

        static int alphaStart;
        static int alphaEnd;
        
        public override void Initialize()
        {
            settings = GlobalSettings;
            instance.Log("Initializing");

            float startTime = Time.realtimeSinceStartup;
            instance.Log("Building MethodInfo dict...");
            
            bindMethods.Clear();
            foreach (MethodInfo method in typeof(BindableFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                object[] attributes = method.GetCustomAttributes(typeof(BindableMethod), false);

                if (attributes.Any())
                {
                    BindableMethod attr = (BindableMethod)attributes[0];
                    string name = attr.name;
                    string cat = attr.category;
                    bool allowLock = attr.allowLock;

                    bindMethods.Add(name, (cat, allowLock, (Action)Delegate.CreateDelegate(typeof(Action), method)));
                }
            }
            
            instance.Log("Done! Time taken: " + (Time.realtimeSinceStartup - startTime) + "s. Found " + bindMethods.Count + " methods");

            if (settings.FirstRun)
            {
                instance.Log("First run detected, setting default binds");

                settings.FirstRun = false;
                ResetKeyBinds();
            }

            if (!Directory.Exists(settings.ModBaseDirectory)) 
            {
                Directory.CreateDirectory(settings.ModBaseDirectory);
            }

            if (settings.NumPadForSaveStates)
            {
                alphaStart = (int)KeyCode.Keypad0;
                alphaEnd = (int)KeyCode.Keypad9;
            }
            else
            {
                alphaStart = (int)KeyCode.Alpha0;
                alphaEnd = (int)KeyCode.Alpha9;
            }

            int alphaInt = 0;
            alphaKeyDict.Clear();
                
            for (int i = alphaStart; i <= alphaEnd; i++)
            {
                KeyCode tmpKeyCode = (KeyCode)i;
                alphaKeyDict.Add(tmpKeyCode, alphaInt++);
            }
            

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += LevelActivated;
            GameObject UIObj = new GameObject();
            UIObj.AddComponent<GUIController>();
            Object.DontDestroyOnLoad(UIObj);
            
            saveStateManager = new SaveStateManager();
            ModHooks.Instance.AfterSavegameLoadHook += LoadCharacter;

            //ModHooks.Instance.HitInstanceHook += DoDamage;
            
            ModHooks.Instance.NewGameHook += NewCharacter;
            ModHooks.Instance.BeforeSceneLoadHook += OnLevelUnload;
            ModHooks.Instance.TakeHealthHook += PlayerDamaged;
            ModHooks.Instance.ApplicationQuitHook += SaveSettings;

            if (settings.ShowCursorWhileUnpaused)
            {
                BindableFunctions.SetAlwaysShowCursor();
            }

            BossHandler.PopulateBossLists();
            GUIController.Instance.BuildMenus();
            SceneWatcher.Init();

            KeyBindLock = false;
            TimeScaleActive = false;

            Console.AddLine("New session started " + DateTime.Now);
        }

        public DebugMod()
        {
            instance = this;
            // Register exports early so other mods can use them when initializing
            typeof(DebugExport).ModInterop();

            // idk
            DoTrollMenu();
        }

        #region Troll Menu
        private static int chooser;
        private static bool OpenedSave;
        private void DoTrollMenu()
        {
            chooser = Random.Range(1, 1000);
            OpenedSave = false;
            if (chooser != 1) return;
            GameObject DebugEasterEgg = new GameObject("DebugEasterEgg");
            Object.DontDestroyOnLoad(DebugEasterEgg);

            On.SetVersionNumber.Start += ChangeVersionNumber;
            On.MenuStyleTitle.SetTitle += FixMenuTitle;
        }

        private void FixMenuTitle(On.MenuStyleTitle.orig_SetTitle orig, MenuStyleTitle self, int index)
        {
            if (GameObject.Find("DebugEasterEgg") == null) orig(self, index);
            else if (OpenedSave) orig(self, index);

            else
            {
                Log("Running");
                MenuStyleTitle.TitleSpriteCollection spriteCollection =
                    index < 0 || index >= self.TitleSprites.Length
                        ? self.DefaultTitleSprite
                        : self.TitleSprites[index];

                Texture2D RealTitle_texture = new Texture2D(1, 1);
                using (Stream stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("DebugMod.Images.SilkNever.png"))
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    RealTitle_texture.LoadImage(bytes, false);
                    RealTitle_texture.name = "SilkNever";
                }

                var RealTitle = Sprite.Create(RealTitle_texture,
                    new Rect(0, 0, RealTitle_texture.width, RealTitle_texture.height),
                    new Vector2(0.5f, 0.5f), spriteCollection.Default.pixelsPerUnit, 0, SpriteMeshType.FullRect);

                self.Title.sprite = RealTitle;
            }
        }

        private void ChangeVersionNumber(On.SetVersionNumber.orig_Start orig, SetVersionNumber self)
        {
            Text textUi = ReflectionHelper.GetAttr<SetVersionNumber, Text>(self, "textUi");

            if (!(textUi != null)) return;
            
            string VersionNumber = OpenedSave ? Constants.GAME_VERSION : "1.2.2.1";
            StringBuilder stringBuilder = new StringBuilder(VersionNumber);
            textUi.text = stringBuilder.ToString();
        }

        #endregion


        internal static void ResetKeyBinds()
        {
            settings.binds.Clear();

            settings.binds.Add("Toggle All UI", KeyCode.F1);
            settings.binds.Add("Toggle Info", KeyCode.F2);
            settings.binds.Add("Toggle Menu", KeyCode.F3);
            settings.binds.Add("Toggle Console", KeyCode.F4);
            settings.binds.Add("Full/Min Info Switch", KeyCode.F6);
            settings.binds.Add("Force Camera Follow", KeyCode.F8);
            settings.binds.Add("Toggle Enemy Panel", KeyCode.F9);
            settings.binds.Add("Toggle Binds", KeyCode.BackQuote);
            settings.binds.Add("Nail Damage +4", KeyCode.Equals);
            settings.binds.Add("Nail Damage -4", KeyCode.Minus);
            settings.binds.Add("Increase Timescale", KeyCode.KeypadPlus);
            settings.binds.Add("Decrease Timescale", KeyCode.KeypadMinus);
            settings.binds.Add("Zoom In", KeyCode.PageUp);
            settings.binds.Add("Zoom Out", KeyCode.PageDown);
        }
        private void SaveSettings()
        {
            SaveGlobalSettings();
            instance.Log("Saved");
        }

        private int PlayerDamaged(int damageAmount) => infiniteHP ? 0 : damageAmount;

        private void NewCharacter() => LoadCharacter(null);

        private void LoadCharacter(SaveGameData saveGameData)
        {
            OpenedSave = true;
            var DebugEasterEggChecker = GameObject.Find("DebugEasterEgg");
            if (DebugEasterEggChecker != null) GameObject.Destroy(DebugEasterEggChecker);

            //NailDamage = saveGameData?.playerData.nailDamage ?? 5;
            
            Console.Reset();
            EnemiesPanel.Reset();
            DreamGate.Reset();

            playerInvincible = false;
            infiniteHP = false;
            infiniteSoul = false;
            noclip = false;

            _loadingChar = true;
        }

        private void LevelActivated(Scene sceneFrom, Scene sceneTo)
        {
            string sceneName = sceneTo.name;
            
            if (_loadingChar)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(PlayerData.instance.playTime);
                string text = string.Format("{0:00}.{1:00}", Math.Floor(timeSpan.TotalHours), timeSpan.Minutes);
                int profileID = PlayerData.instance.profileID;
                Console.AddLine("New savegame loaded. Profile playtime " + text + " Completion: " + PlayerData.instance.completionPercentage + " Save slot: " + profileID + " Game Version: " + PlayerData.instance.version);
                _loadingChar = false;
            }

            if (GM.IsGameplayScene())
            {
                _loadTime = Time.realtimeSinceStartup;
                Console.AddLine("New scene loaded: " + sceneName);
                EnemiesPanel.Reset();
                PlayerDeathWatcher.Reset();
                BossHandler.LookForBoss(sceneName);
                MethodHelpers.VisualMaskHelper.OnSceneChange(sceneTo);
            }
        }

        private string OnLevelUnload(string toScene)
        {
            _unloadTime = Time.realtimeSinceStartup;

            return toScene;
        }

        public static string GetSceneName()
        {
            if (GM == null)
            {
                instance.LogWarn("GameManager reference is null in GetSceneName");
                return "";
            }

            string sceneName = GM.GetSceneNameString();
            return sceneName;
        }

        public static float GetLoadTime()
        {
            return (float)Math.Round(_loadTime - _unloadTime, 2);
        }

        public static void Teleport(string scenename, Vector3 pos)
        {
            HC.transform.position = pos;

            HC.EnterWithoutInput(false);
            HC.proxyFSM.SendEvent("HeroCtrl-LeavingScene");
            HC.transform.SetParent(null);

            GM.NoLongerFirstGame();
            GM.SaveLevelState();
            GM.SetState(GameState.EXITING_LEVEL);
            GM.entryGateName = "dreamGate";
            RefCamera.FreezeInPlace();

            HC.ResetState();

            GM.LoadScene(scenename);
        }

        /// <summary>
        /// Adds a menu to the top menu, with the provided name and button list.
        /// </summary>
        [PublicAPI]
        public static void AddTopMenuContent(string MenuName, List<TopMenuButton> ButtonList) => TopMenu.AddTopMenuContent(MenuName, ButtonList);
        
        /// <summary>
        /// Add all public static methods on a type to the keybinds list. Methods must be decorated with the BindableMethod attribute.
        /// </summary>
        [PublicAPI]
        public static void AddToKeyBindList(Type BindableFunctionsClass)
        {
            foreach (MethodInfo method in BindableFunctionsClass.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                object[] attributes = method.GetCustomAttributes(typeof(BindableMethod), false);

                if (attributes.Any())
                {
                    BindableMethod attr = (BindableMethod) attributes[0];
                    string name = attr.name;
                    string cat = attr.category;
                    bool allowLock = attr.allowLock;

                    instance.Log($"Recieved Action: {name} (from {BindableFunctionsClass.Name})");
                    AdditionalBindMethods.Add(name, (cat, allowLock, (Action) Delegate.CreateDelegate(typeof(Action), method)));
                }
            }
        }

        /// <summary>
        /// Add an action to the keybinds list.
        /// </summary>
        [PublicAPI]
        public static void AddActionToKeyBindList(Action method, string name, string category)
        {
            AddActionToKeyBindList(method, name, category, true);   
        }

        /// <summary>
        /// Add an action to the keybinds list.
        /// </summary>
        [PublicAPI]
        public static void AddActionToKeyBindList(Action method, string name, string category, bool allowLock)
        {
            instance.Log($"Received Action: {name}");
            AdditionalBindMethods.Add(name, (category, allowLock, method));
        }
    }
}
