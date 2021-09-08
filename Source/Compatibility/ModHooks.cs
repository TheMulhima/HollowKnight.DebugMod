using System;
using System.Reflection;
using System.Text;
using Modding;
#if CURRENTVERSION
using Modding.Delegates;
#endif
using UnityEngine;
namespace DebugMod.Compatibility
{
    public static class ModHooks
    {
        
        #if CURRENTVERSION
        public static event Action<int> SavegameLoadHook
        {
            add => Modding.ModHooks.SavegameLoadHook += value;
            remove => Modding.ModHooks.SavegameLoadHook -= value;
        }
        
        #elif OLDVERSION
        public static event SavegameLoadHandler SavegameLoadHook
        {
            add => Modding.ModHooks.Instance.SavegameLoadHook += value;
            remove => Modding.ModHooks.Instance.SavegameLoadHook -= value;
        }
        #endif
        
        #if CURRENTVERSION
        public static event Action NewGameHook
        {
            add => Modding.ModHooks.NewGameHook += value;
            remove => Modding.ModHooks.NewGameHook -= value;
        }
        
        #elif OLDVERSION
        public static event NewGameHandler NewGameHook
        {
            add => Modding.ModHooks.Instance.NewGameHook += value;
            remove => Modding.ModHooks.Instance.NewGameHook -= value;
        }
        #endif
        
        #if CURRENTVERSION
        public static event Func<string, string> BeforeSceneLoadHook
        {
            add => Modding.ModHooks.BeforeSceneLoadHook += value;
            remove => Modding.ModHooks.BeforeSceneLoadHook -= value;
        }
        
        #elif OLDVERSION
        public static event BeforeSceneLoadHandler BeforeSceneLoadHook
        {
            add => Modding.ModHooks.Instance.BeforeSceneLoadHook += value;
            remove => Modding.ModHooks.Instance.BeforeSceneLoadHook -= value;
        }
        #endif


        public static event TakeHealthProxy TakeHealthHook
        {
            #if CURRENTVERSION
            add => Modding.ModHooks.TakeHealthHook += value;
            remove => Modding.ModHooks.TakeHealthHook -= value;
            #elif OLDVERSION
            add => Modding.ModHooks.Instance.TakeHealthHook += value;
            remove => Modding.ModHooks.Instance.TakeHealthHook -= value;
            #endif
        }

        
        #if CURRENTVERSION
        public static event Action ApplicationQuitHook
        {
            add => Modding.ModHooks.ApplicationQuitHook += value;
            remove => Modding.ModHooks.ApplicationQuitHook -= value;
        }
        
        #elif OLDVERSION
        public static event ApplicationQuitHandler ApplicationQuitHook
        {
            add => Modding.ModHooks.Instance.ApplicationQuitHook += value;
            remove => Modding.ModHooks.Instance.ApplicationQuitHook -= value;
        }
        #endif
        
        #if CURRENTVERSION
        public static event Action<GameObject> ColliderCreateHook
        {
            add => Modding.ModHooks.ColliderCreateHook += value;
            remove => Modding.ModHooks.ColliderCreateHook -= value;
        }
        #elif OLDVERSION
        public static event ColliderCreateHandler ColliderCreateHook
        {
            add => Modding.ModHooks.Instance.ColliderCreateHook += value;
            remove => Modding.ModHooks.Instance.ColliderCreateHook -= value;
        }
        #endif
        
        #if CURRENTVERSION
        public static event Action<SaveGameData> AfterSavegameLoadHook
        {
            add => Modding.ModHooks.AfterSavegameLoadHook += value;
            remove => Modding.ModHooks.AfterSavegameLoadHook -= value;
        }
        #elif OLDVERSION
        public static event AfterSavegameLoadHandler AfterSavegameLoadHook
        {
            add => Modding.ModHooks.Instance.AfterSavegameLoadHook += value;
            remove => Modding.ModHooks.Instance.AfterSavegameLoadHook -= value;
        }
        #endif
    }
}