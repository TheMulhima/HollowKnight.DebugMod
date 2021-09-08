using System;
using System.Reflection;
using GlobalEnums;
using Modding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugMod.MonoBehaviours
{
    
    //taken from Decoration Master Mod
    public class MyCursor : MonoBehaviour
    {
        private static Texture2D Cursor;
        private void Awake() => Cursor = GUIController.Instance.images["Cursor"];

        private void OnGUI() => GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Cursor.width, Cursor.height), Cursor);
        private void Update()
        {
            if (GameManager.instance.gameState == GameState.PAUSED)
            {
                DebugMod.PauseGameNoUIActive = false;
                Console.AddLine("Game was paused and unfrozen");
                var component = GameManager.instance.gameObject.GetComponent<MyCursor>();
                if (component != null) Destroy(component);
            }
        }
    }
}