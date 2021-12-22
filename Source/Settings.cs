using System;
using System.IO;
using System.Collections.Generic;
using Modding;

using UnityEngine;

namespace DebugMod
{
    //Empty class required for DebugMod class definition
    public class SaveSettings:ModSettings {}

    [Serializable]
    public class KeyBinds
    {
        public Dictionary<string, string> binds_to_file = new Dictionary<string, string>();
    }
    
    public class GlobalSettings:ModSettings
    {
        //Save members
        public Dictionary<string, int> binds = new Dictionary<string, int>();

        public readonly string ModBaseDirectory = Path.Combine(Application.persistentDataPath, "DebugModData");

        public bool ConsoleVisible = true;

        public bool EnemiesPanelVisible = true;

        public bool HelpPanelVisible = true;

        public bool InfoPanelVisible = true;

        [Obsolete]
        public bool MinInfoPanelVisible = true;
        public string CurrentInfoPanelName = "";

        public bool SaveStatePanelVisible = true;
        
        public bool TopMenuVisible = true;

        public bool FirstRun = true;

        public bool NumPadForSaveStates = false;
        
        public int ShowHitBoxes;

        public int MaxSaveStates = 10;

        public int MaxSavePages = 10;

        public float AmountToMove = 0.1f;

        public float NoClipSpeedModifier = 1f;

        public bool ShowCursorWhileUnpaused = false;
    }
}
