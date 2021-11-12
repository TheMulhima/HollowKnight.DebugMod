using UnityEngine;
using UnityEngine.Events;

namespace DebugMod
{
    public abstract class TopMenuButton
    {
        public UnityAction<string> ClickedFunction { get; protected set; }
    }
    public class TextButton:TopMenuButton
    {
        public string ButtonText { get; }

        
        public TextButton(string buttonText, UnityAction<string> clickedFunction)
        {
            ButtonText = buttonText;
            ClickedFunction = clickedFunction;
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
    }
}