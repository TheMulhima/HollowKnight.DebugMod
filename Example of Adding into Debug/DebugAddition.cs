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
            if (AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName().Name).Contains("DebugMod"))
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
            
            string MenuName = "My Menu";
            
            //make a new button in top menu. when this button is clicked, it'll open a panel where you can add text/images as buttons
            //you need to pass in name of menu
            DebugMod.TopMenu.AddNewMenuToTopMenu(MenuName);

            //some functions we'll need when adding buttons
            UnityAction<string> TakeDamage = _ => HeroController.instance.TakeDamage(HeroController.instance.takeHitPrefab, CollisionSide.left, 1, (int) HazardType.LAVA);
            UnityAction<string> GiveHealth = _  => HeroController.instance.AddHealth(1);

            //Add a text button onto the panel. requires AddNewMenuToTopMenu to be called before this is called
            //parameters:
            //MenuName: the name of the panel you made
            //ButtonText: name of button that will show to the player
            //ClickedFunction: Unity<string> function which will be executed when the player clicks button being made
            //Y_Position: Where thee button is going to be. For reference, all text panels use 30, 60, 90.... and so on. Max shouldnt be bigger than 357
            DebugMod.TopMenu.AddTextToMenuButton(MenuName,
                "Take Damage", 
                TakeDamage,
                30f);
            
            //For AddImageToMenuButton(); See how debug does it in TopMenu.cs. its basically the same function. couldnt find a way to simplify it
        }   
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
