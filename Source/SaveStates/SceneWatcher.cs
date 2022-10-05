using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    public static class SceneWatcher
    {
        private static List<LoadedSceneInfo> scenes;
        public static ReadOnlyCollection<LoadedSceneInfo> LoadedScenes => scenes.AsReadOnly();

        private static Dictionary<Scene, int> scenesWithManager;

        private static void AddScene(Scene scene, LoadSceneMode mode, bool checkSceneManager = true)
        {
            if (mode == LoadSceneMode.Single)
                scenes.Clear();

            LoadedSceneInfo d = new LoadedSceneInfo(scene.name, scene.name);
            scenes.Add(d);

            if (checkSceneManager && Object.FindObjectsOfType<SceneManager>().Any(m => m.gameObject.scene == scene))
                scenesWithManager.Add(scene, d.id);
        }

        public static void Init()
        {
            scenesWithManager = new();
            scenes = new();

            for (int i = 0; i < USceneManager.sceneCount; i++)
                AddScene(USceneManager.GetSceneAt(i), LoadSceneMode.Additive, false);
            
            USceneManager.sceneLoaded += (scene, mode) => AddScene(scene, mode);
            On.SceneManager.Start += (orig, self) =>
            {
                orig(self);

                if (!scenesWithManager.ContainsKey(self.gameObject.scene))
                    return;

                int id = scenesWithManager[self.gameObject.scene];
                LoadedSceneInfo lsi = scenes.FirstOrDefault(i => i.id == id);
                scenesWithManager.Remove(self.gameObject.scene);
                
                if (lsi != null)
                    lsi.activeSceneWhenLoaded = GameManager.instance.sceneName;
            };
            USceneManager.sceneUnloaded += s => scenes.RemoveAt(scenes.FindIndex(d => d.name == s.name));
        }
        
        public class LoadedSceneInfo
        {
            private static int counter = 0;
            
            public readonly string name;
            public string activeSceneWhenLoaded { get; internal set; }
            public readonly int id;

            public LoadedSceneInfo(string name, string activeSceneName)
            {
                this.name = name;
                this.id = counter++;
                this.activeSceneWhenLoaded = activeSceneName;
            }

            public void LoadHook()
            {
                On.GameManager.UpdateSceneName += UpdateSceneNameOverride;
                GameManager.instance.OnFinishedSceneTransition += FinishedSceneTransitionHook;
            }

            private void FinishedSceneTransitionHook()
            {
                GameManager.instance.OnFinishedSceneTransition -= FinishedSceneTransitionHook;
                On.GameManager.UpdateSceneName -= UpdateSceneNameOverride;
            }

            public void UpdateSceneNameOverride(On.GameManager.orig_UpdateSceneName orig, GameManager self)
            {
                self.sceneName = activeSceneWhenLoaded;
            }
        }
    }
}