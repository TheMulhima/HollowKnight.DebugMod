using System;
using System.Linq;
using DebugMod;
using GlobalEnums;
using Modding;
using UnityEngine.Events;

namespace DebugAddition
{
    public class DebugAddition:Mod
    {
        public override string GetVersion() => "v1.0.0";

        public static DebugAddition Instance;

        public override void Initialize()
        {
            Instance = this;

            //checks if Debug Exists. makes sure the mod doesnt fail to load
            if (ModHooks.GetMod("DebugMod") is Mod)
            {
                //this needs to be in different function or else mod will fail to load if debug exists
                AddStuffToDebug();
            };
        }

        private void AddStuffToDebug()
        {
            //Add the keybinds into debug's keybind menu
            //you have to pass in typeof(classyoumade)
            //see BindableFunctions2 for example on how to set it up
            DebugMod.DebugMod.AddToKeyBindList(typeof(BindableFunctions2));
            
			//To add to Debug TopMenu, Syntax would look something like this
			DebugMod.DebugMod.AddTopMenuContent("TestMenu",
                new List<TopMenuButton>()
                {
                     new TextButton("Button 1", _ => { DebugMod.Console.AddLine("Run any code here"); }),
					 new ImageButton(Image, _ => { DebugMod.Console.AddLine("Run any code here"); }),
                });
			// the AddTopMenuContent takes 2 parameters, the menu name and the List of TopMenuButton. This list will contain the buttons that will be added
			// currectly how it works is it will take reach entry in the list in the order it was added and place it into the menu as per the following rules
			// 1 text button per row and 2 image buttons per row
			
			//to add a text button to the menu, call new TextButton to create a text button. 
			// the parameters it takes is the string MenuName and the UnityAction<string> which is the action that will run when the button is clicked
			
			//to add an image button to the menu, call new ImageButton to create an image button. 
			// the parameters it takes is the Texture2D which will be the image shown to user and the UnityAction<string> which is the action that will run when the button is clicked
			// note that for 2 images to appear in the same row, they'll need to be added next to each other in the list
			
        }   
		private Texture2D Image = null;
    }
    
    public static class BindableFunctions2
    {
        [BindableMethod(name = "New fun", category = "new cat")]
        public static void Logger()
        {
            DebugAddition.Instance.Log("Nice");
            DebugMod.Console.AddLine("Nice");
        }
    }
}
