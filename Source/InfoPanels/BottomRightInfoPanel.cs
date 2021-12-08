using System;
using System.Collections.Generic;
using DebugMod.Canvas;
using UnityEngine;
using InControl;
using JetBrains.Annotations;

namespace DebugMod.InfoPanels
{
    public class BottomRightInfoPanel : InfoPanel
    {
        public override void BuildPanel(GameObject canvas)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadRawTextureData(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            tex.Apply();

            // Puke
            panel = new CanvasPanel(
                canvas,
                tex,
                new Vector2(0f, 223f),
                Vector2.zero,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f));

            panel.AddText("Right1 Label", "Session Time\nLoad\nHero Pos\nMove Raw", new Vector2(1285, 747), Vector2.zero, GUIController.Instance.arial);
            panel.AddText("Right1", "", new Vector2(1385, 747), Vector2.zero, GUIController.Instance.trajanNormal);

            panel.AddText("Right2 Label", "Move Vector\nKey Pressed\nMove Pressed\nInput X", new Vector2(1550, 747), Vector2.zero, GUIController.Instance.arial);
            panel.AddText("Right2", "", new Vector2(1650, 747), Vector2.zero, GUIController.Instance.trajanNormal);

            panel.FixRenderOrder();
        }

        public override void UpdatePanel()
        {
            if (panel == null) return;

            int time1 = Mathf.FloorToInt(Time.realtimeSinceStartup / 60f);
            int time2 = Mathf.FloorToInt(Time.realtimeSinceStartup - (float)(time1 * 60));

            panel.GetText("Right1").UpdateText(string.Format("{0:00}:{1:00}", time1, time2) + "\n" + DebugMod.GetLoadTime() + "s\n" + InfoPanel.GetHeroPos() + "\n" + string.Format("L: {0} R: {1}", DebugMod.IH.inputActions.left.RawValue, DebugMod.IH.inputActions.right.RawValue));
            panel.GetText("Right2").UpdateText(DebugMod.IH.inputActions.moveVector.Vector.x + ", " + DebugMod.IH.inputActions.moveVector.Vector.y + "\n" + InfoPanel.GetStringForBool(InputManager.AnyKeyIsPressed) + "\n" + InfoPanel.GetStringForBool(DebugMod.IH.inputActions.left.IsPressed || DebugMod.IH.inputActions.right.IsPressed) + "\n" + DebugMod.IH.inputX);
        }

        public override bool Active => true;
    }
}
