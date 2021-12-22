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
            public static Action<Action, string, string> AddActionToKeyBindList = null;
            public static Action<string> LogToConsole = null;
            public static Func<bool, string> GetStringForBool = null;
            public static Action<string, bool> CreateCustomInfoPanel = null;
            public static Action<string, float, float, float, string, Func<string>> AddInfoToPanel = null;
            public static Action<string, float> CreateSimpleInfoPanel = null;
            public static Action<string, string, Func<string>> AddInfoToSimplePanel = null;
        }
        static DebugMod()
        {
            // MonoMod will automatically fill in the actions in DebugImport the first time they're used
            typeof(DebugImport).ModInterop();
        }

        public static void AddActionToKeyBindList(Action method, string name, string category)
            => DebugImport.AddActionToKeyBindList?.Invoke(method, name, category);

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
    }
}