using System;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using JetBrains.Annotations;
using DebugMod.Canvas;
using DebugMod.InfoPanels;

namespace DebugMod
{
    /// <summary>
    /// Represents an info panel in the style of the standard info panel
    /// </summary>
    public abstract class InfoPanel
    {
        public const string MainInfoPanelName = "DebugMod.MainInfoPanel";
        public const string MinimalInfoPanelName = "DebugMod.MinimalInfoPanel";

        protected CanvasPanel panel = null;

        /// <summary>
        /// Determine whether the panel should be displayed.
        /// </summary>
        public abstract bool Active { get; }

        /// <summary>
        /// Called once to set up the panel.
        /// </summary>
        public abstract void BuildPanel(GameObject canvas);
        /// <summary>
        /// Called every frame to change the text on the panel.
        /// </summary>
        public abstract void UpdatePanel();


        private static Dictionary<string, InfoPanel> AllPanels = new()
        {
            ["DebugMod.BottomRightInfoPanel"] = new BottomRightInfoPanel(),
            [MainInfoPanelName] = CustomInfoPanel.GetMainInfoPanel(),
            [MinimalInfoPanelName] = CustomInfoPanel.GetMinimalInfoPanel(),
        };
        private static List<string> TogglablePanelNames = new() { MainInfoPanelName, MinimalInfoPanelName };

        public static void ToggleActivePanel()
        {
            int i = TogglablePanelNames.IndexOf(DebugMod.settings.CurrentInfoPanelName);
            i = (i + 1) % TogglablePanelNames.Count;
            DebugMod.settings.CurrentInfoPanelName = TogglablePanelNames[i];
        }

        public static void BuildInfoPanels(GameObject canvas)
        {
            foreach (InfoPanel panel in AllPanels.Values)
            {
                panel.BuildPanel(canvas);
            }

            if (!TogglablePanelNames.Contains(DebugMod.settings.CurrentInfoPanelName))
            {
                DebugMod.settings.CurrentInfoPanelName = MainInfoPanelName;
            }
        }
        public static void Update()
        {
            if (GUIController.ForceHideUI() || !DebugMod.settings.InfoPanelVisible)
            {
                foreach (InfoPanel infoPanel in AllPanels.Values)
                {
                    infoPanel.panel?.SetActive(false, true);
                }
                return;
            }

            foreach (InfoPanel infoPanel in AllPanels.Values)
            {
                if (infoPanel.Active != infoPanel.panel.active)
                {
                    infoPanel.panel?.SetActive(infoPanel.Active, !infoPanel.Active);
                }

                if (infoPanel.Active)
                {
                    infoPanel.UpdatePanel();
                }
            }
        }

        #region Custom Panel API
        /// <summary>
        /// Create a new Custom Info Panel with the given name.
        /// </summary>
        /// <param name="Name">The name of the panel - this will not be displayed.</param>
        /// <param name="ShowSprite">Whether to place the panel on the background sprite.</param>
        [PublicAPI]
        public static void CreateCustomInfoPanel(string Name, bool ShowSprite)
        {
            AddInfoPanel(Name, new CustomInfoPanel(Name, ShowSprite));
        }

        /// <summary>
        /// Add an info entry to the specified info panel.
        /// </summary>
        /// <param name="Name">The name of the panel.</param>
        /// <param name="xLabel">The x coordinate of the label.</param>
        /// <param name="xInfo">The x coordinate of the info string.</param>
        /// <param name="y">The y coordinate of the entry.</param>
        /// <param name="label">The text to display on the label.</param>
        /// <param name="textFunc">A function that returns the text to show on the info string; will be called every frame.</param>
        [PublicAPI]
        public static void AddInfoToPanel(string Name, float xLabel, float xInfo, float y, string label, Func<string> textFunc)
        {
            ((CustomInfoPanel)AllPanels[Name]).AddInfo(xLabel, xInfo, y, label, textFunc);
        }

        /// <summary>
        /// Create a new Simple Info Panel with the given name.
        /// </summary>
        /// <param name="Name">The name of the panel - this will not be displayed.</param>
        /// <param name="sep">The separation between the columns.</param>
        [PublicAPI]
        public static void CreateSimpleInfoPanel(string Name, float sep)
        {
            AddInfoPanel(Name, new SimpleInfoPanel(Name, sep));
        }

        /// <summary>
        /// Add an info entry to the specified simple info panel.
        /// </summary>
        /// <param name="Name">The name of the panel.</param>
        /// <param name="label">The text to display on the label.</param>
        /// <param name="textFunc">A function that returns the text to show on the info string; will be called every frame.</param>
        [PublicAPI]
        public static void AddInfoToSimplePanel(string Name, string label, Func<string> textFunc)
        {
            ((SimpleInfoPanel)AllPanels[Name]).AddInfo(label, textFunc);
        }

        /// <summary>
        /// Add an info panel to the rotation. Must be done during mod initialization.
        /// </summary>
        /// <param name="Name">The name of the panel.</param>
        /// <param name="p">The panel to add.</param>
        /// <exception cref="InvalidOperationException">A panel with this name already exists.</exception>
        [PublicAPI]
        public static void AddInfoPanel(string Name, TogglableInfoPanel p)
        {
            if (AllPanels.ContainsKey(Name))
            {
                throw new InvalidOperationException("A panel with this name already exists");
            }

            AllPanels.Add(Name, p);
            TogglablePanelNames.Add(Name);
        }
        #endregion

        public static string GetTransState()
        {
            string transState = HeroController.instance.transitionState.ToString();
            if (transState == "WAITING_TO_ENTER_LEVEL") transState = "LOADING";
            if (transState == "WAITING_TO_TRANSITION") transState = "WAITING";
            return transState;
        }
        public static string GetHeroPos()
        {
            if (DebugMod.RefKnight == null)
            {
                return String.Empty;
            }
            float HeroX = DebugMod.RefKnight.transform.position.x;
            float HeroY = DebugMod.RefKnight.transform.position.y;

            return $"({HeroX}, {HeroY})";
        }
        public static string GetStringForBool(bool b)
        {
            return b ? "✓" : "X";
        }
    }
}
