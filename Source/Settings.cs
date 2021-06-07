using System.Collections.Generic;
using Modding;

namespace DebugMod
{
    //Empty class required for DebugMod class definition
    public class SaveSettings { }

    public class GlobalSettings
    {
        //Save members
        public Dictionary<string, int> binds = new Dictionary<string, int>();

        public bool ConsoleVisible = true;

        public bool EnemiesPanelVisible = true;

        public bool HelpPanelVisible = true;

        public bool InfoPanelVisible = true;

        public bool TopMenuVisible = true;

        public bool FirstRun = true;
        public int ShowHitBoxes;

        public float AmountToMove = 0.1f;
    }
}
