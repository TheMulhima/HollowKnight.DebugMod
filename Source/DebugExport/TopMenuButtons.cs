using DebugMod.Canvas;
using UnityEngine;
using UnityEngine.Events;

namespace DebugMod
{
    public abstract class TopMenuButton
    {
        public UnityAction<string> ClickedFunction { get; protected set; }

        public abstract void CreateButton(CanvasPanel panel);
    }
    public class TextButton:TopMenuButton
    {
        public string ButtonText { get; }
        
        public TextButton(string buttonText, UnityAction<string> clickedFunction)
        {
            ButtonText = buttonText;
            ClickedFunction = clickedFunction;
        }

        public override void CreateButton(CanvasPanel panel)
        {
            panel.AddButton(ButtonText,
                GUIController.Instance.images["ButtonRectEmpty"],
                panel.GetNextPos(CanvasPanel.MenuItems.TextButton),
                Vector2.zero,
                ClickedFunction,
                new Rect(0f, 0f, 80f, 20f),
                GUIController.Instance.trajanNormal,
                ButtonText,
                10);
        }
    }

    public class ImageButton : TopMenuButton
    {
        public Texture2D ButtonImage { get; }
        
        public ImageButton(Texture2D buttonImage,UnityAction<string> clickedFunction)
        {
            ClickedFunction = clickedFunction;
            ButtonImage = buttonImage;
        }

        public override void CreateButton(CanvasPanel panel)
        {
            panel.AddButton($"{ButtonImage.name} {panel.NumButtons}",
                ButtonImage,
                panel.GetNextPos(CanvasPanel.MenuItems.ImageButton),
                new Vector2(27f, 27f),
                ClickedFunction,
                new Rect(0, 0, ButtonImage.width, ButtonImage.height));

        }
    }
}