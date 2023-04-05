using System;

namespace DebugMod.InfoPanels
{
    /// <summary>
    /// Represents an info panel which is part of the toggle rotation.
    /// </summary>
    public abstract class TogglableInfoPanel : InfoPanel
    {
        private string Name;

        public TogglableInfoPanel(string Name)
        {
            this.Name = Name;
        }

        public override bool Active => DebugMod.settings.CurrentInfoPanelName == this.Name;
    }
}
