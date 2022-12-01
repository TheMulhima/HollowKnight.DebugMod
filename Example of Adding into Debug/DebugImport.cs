using System;
using MonoMod.ModInterop;

// Replace the namespace with your project's root namespace
namespace DebugAddition
{
    internal static class DebugMod
    {
        [ModImportName("DebugMod")]
        private static class DebugImport
        {
            public static Action<Action, string, string, bool> AddActionToKeyBindList = null;
            public static Action<string> LogToConsole = null;
            public static Func<bool, string> GetStringForBool = null;
            public static Action<string, bool> CreateCustomInfoPanel = null;
            public static Action<string, float, float, float, string, Func<string>> AddInfoToPanel = null;
            public static Action<string, float> CreateSimpleInfoPanel = null;
            public static Action<string, string, Func<string>> AddInfoToSimplePanel = null;
            public static Action<bool> SetLockKeyBinds;
            public static Action<Action> AddToOnGiveAllCharm;
            public static Action<Action> RemoveFromOnGiveAllCharm;
            public static Action<Action> AddToOnRemoveAllCharm;
            public static Action<Action> RemoveFromOnRemoveAllCharm;
        }
        static DebugMod()
        {
            // MonoMod will automatically fill in the actions in DebugImport the first time they're used
            typeof(DebugImport).ModInterop();
        }

        /// <summary>
        /// Add an action to the keybinds list.
        /// </summary>
        /// <param name="method">The method to run when keybind is pressed</param>
        /// <param name="name">The name of the keybind</param>
        /// <param name="category">The page on the keybind list containing this bind</param>
        /// <param name="allowLock">Whether or not this keybind can be unusable when lock keybinds is active (should usually default to true). if allow lock is set to false, lock keybinds will not affect this</param>
        public static void AddActionToKeyBindList(Action method, string name, string category, bool allowLock)
            => DebugImport.AddActionToKeyBindList?.Invoke(method, name, category, allowLock);

        public static void LogToConsole(string msg)
            => DebugImport.LogToConsole?.Invoke(msg);

        public static string GetStringForBool(bool b)
            => DebugImport.GetStringForBool?.Invoke(b) ?? default;

        public static void CreateCustomInfoPanel(string Name, bool ShowSprite)
            => DebugImport.CreateCustomInfoPanel?.Invoke(Name, ShowSprite);

        public static void AddInfoToPanel(string Name, float xLabel, float xInfo, float y, string label, Func<string> textFunc)
            => DebugImport.AddInfoToPanel?.Invoke(Name, xLabel, xInfo, y, label, textFunc);

        public static void CreateSimpleInfoPanel(string Name, float sep)
            => DebugImport.CreateSimpleInfoPanel?.Invoke(Name, sep);

        public static void AddInfoToSimplePanel(string Name, string label, Func<string> textFunc)
            => DebugImport.AddInfoToSimplePanel?.Invoke(Name, label, textFunc);

        public static void SetLockKeyBinds(bool value)
            => DebugImport.SetLockKeyBinds?.Invoke(value);

        public static void AddToOnGiveAllCharm(Action onGiveCharms)
            => DebugImport.AddToOnGiveAllCharm?.Invoke(onGiveCharms);
        
        public static void RemoveFromOnGiveAllCharm(Action onGiveCharms)
           => DebugImport.RemoveFromOnGiveAllCharm?.Invoke(onGiveCharms);
        
        public static void AddToOnRemoveAllCharm(Action onRemoveCharms)
            => DebugImport.AddToOnRemoveAllCharm?.Invoke(onRemoveCharms);
        
        public static void RemoveFromOnRemoveAllCharm(Action onRemoveCharms)
            => DebugImport.RemoveFromOnRemoveAllCharm?.Invoke(onRemoveCharms);
    }
}
