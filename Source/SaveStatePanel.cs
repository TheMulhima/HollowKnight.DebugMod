using UnityEngine;
using InControl;
using System.Collections.Generic;

namespace DebugMod
{
    public static class SaveStatesPanel
    {
        private static CanvasPanel statePanel;

        public static void BuildMenu(GameObject canvas)
        {
            statePanel = new CanvasPanel(
                canvas,
                GUIController.Instance.images["BlankVertical"],
                new Vector2(720f, 40f),
                Vector2.zero,
                new Rect(
                    0f,
                    0f,
                    GUIController.Instance.images["BlankVertical"].width,
                    GUIController.Instance.images["BlankVertical"].height
                )
            );

            statePanel.AddText("Mode", "mode: ", new Vector2(8, 20), Vector2.zero, GUIController.Instance.arial, 15);
            statePanel.AddText("currentmode", "-", new Vector2(60, 20), Vector2.zero, GUIController.Instance.arial, 15);

            for (int i = 0; i < SaveStateManager.maxSaveStates; i++) { 

                //Labels
                statePanel.AddText("Slot " + i, i.ToString(), new Vector2(10, i * 20 + 40), Vector2.zero, GUIController.Instance.arial, 15);
                
                //Values
                statePanel.AddText(i.ToString(), "", new Vector2(50, i * 20 + 40), Vector2.zero, GUIController.Instance.arial, 15);
            }
        }

        public static void Update()
        {
            if (statePanel == null)
            {
                return;
            }

            if (DebugMod.GM.IsNonGameplayScene())
            {
                if (statePanel.active)
                {
                    statePanel.SetActive(false, true);
                }

                return;
            }

            if (DebugMod.settings.SaveStatePanelVisible && !statePanel.active)
            {
                statePanel.SetActive(true, false);
            }
            else if (!DebugMod.settings.SaveStatePanelVisible && statePanel.active)
            {
                statePanel.SetActive(false, true);
            }

            if (statePanel.active)
            {
                statePanel.GetText("currentmode").UpdateText(SaveStateManager.currentStateOperation);

                for (int i = 0; i < SaveStateManager.maxSaveStates; i++)
                {
                    statePanel.GetText(i.ToString()).UpdateText("open");
                }

                foreach (KeyValuePair<int, string[]> entry in SaveStateManager.GetSaveStatesInfo())
                {
                    statePanel.GetText(entry.Key.ToString()).UpdateText(string.Format("{0} - {1}", entry.Value[0], entry.Value[1]));
                }
            }
        }

        private static string GetStringForBool(bool b)
        {
            return b ? "✓" : "X";
        }
    }
}