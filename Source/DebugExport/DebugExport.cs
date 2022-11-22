using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoMod.ModInterop;

namespace DebugMod
{
    [ModExportName(nameof(DebugMod))]
    public static class DebugExport
    {
        public static void AddActionToKeyBindList(Action method, string name, string category) 
            => DebugMod.AddActionToKeyBindList(method, name, category);
        
        public static void AddActionToKeyBindList(Action method, string name, string category, bool allowLock) 
            => DebugMod.AddActionToKeyBindList(method, name, category, allowLock);

        public static void LogToConsole(string msg)
            => Console.AddLine(msg);

        public static string GetStringForBool(bool b)
            => InfoPanel.GetStringForBool(b);

        public static void CreateCustomInfoPanel(string Name, bool ShowSprite)
            => InfoPanel.CreateCustomInfoPanel(Name, ShowSprite);

        public static void AddInfoToPanel(string Name, float xLabel, float xInfo, float y, string label, Func<string> textFunc)
            => InfoPanel.AddInfoToPanel(Name, xLabel, xInfo, y, label, textFunc);

        public static void CreateSimpleInfoPanel(string Name, float sep)
            => InfoPanel.CreateSimpleInfoPanel(Name, sep);

        public static void AddInfoToSimplePanel(string Name, string label, Func<string> textFunc)
            => InfoPanel.AddInfoToSimplePanel(Name, label, textFunc);
        
        public static void SetLockKeyBinds(bool value)
            => DebugMod.KeyBindLock = value;

        public static void AddToOnGiveAllCharm(Action onGiveCharms)
            => BindableFunctions.OnGiveAllCharms += onGiveCharms;
        
        public static void RemoveFromOnGiveAllCharm(Action onGiveCharms)
            => BindableFunctions.OnGiveAllCharms -= onGiveCharms;
        
        public static void AddToOnRemoveAllCharm(Action onRemoveCharms)
            => BindableFunctions.OnRemoveAllCharms += onRemoveCharms;
        
        public static void RemoveFromOnRemoveAllCharm(Action onRemoveCharms)
            => BindableFunctions.OnRemoveAllCharms -= onRemoveCharms;
    }
}
