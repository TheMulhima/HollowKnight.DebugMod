using UnityEngine;

namespace DebugMod.MonoBehaviours
{
    
    //taken from Decoration Master Mod
    public class MyCursor : MonoBehaviour
    {
        private static Texture2D Cursor;
        private void Awake() => Cursor = GUIController.Instance.images["Cursor"];
        
        private void OnGUI() => GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Cursor.width, Cursor.height), Cursor);
    }
}