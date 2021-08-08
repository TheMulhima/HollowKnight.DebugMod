# Basic Structure of Adding to Debug

1. Reference DebugMod.dll from mods folder (Note: if you follow steps here, the mod won't fail to load if debug isn't present)
2. In the mod's initialize add this code to check if debug exists:
 ```
if (AppDomain.CurrentDomain.GetAssemblies().Select(theName => theName.GetName().Name).Contains("DebugMod"))
            {
                AddStuffToDebug();
            };
```
3. In the `AddStuffToDebug` function, add the code to make stuff appear in Debug. 

# How to add KeyBinds
1) Construct a public class that has a `public static` function for each of the keys you wanna add. These functions must be annotated with `[BindableMethod(name = "name", category = "cat")]`.
2) In the `AddStuffToDebug` function add this line: 
```
DebugMod.DebugMod.AddToKeyBindList(typeof(TheClassYouMadeinStep1));
```

# How to add MenuPanel
1. In `AddStuffToDebug` Add this line to register a button in the top menu that when pressed, will expand a panel
```
DebugMod.TopMenu.AddNewMenuToTopMenu(MenuName);
```
2. To add buttons to the panel, you have 2 options, Add Text buttons or add image buttons
  1. To add text buttons do `DebugMod.TopMenu.AddTextToMenuButton(MenuName, buttonName, Unity<string> OnClikedFunction, Y_Pos);`. For reference, all text panels in debug use Y_Pos 30, 60, 90.... and so on. Max shouldnt be bigger than 357
  2. To add image buttons call on `DebugMod.TopMenu.AddImageToMenuButton();`. For a guide on how to use this, see [how it is done in Debug Item Panel](https://github.com/TheMulhima/HollowKnight.DebugMod/blob/a0f342ce43c14b4084584990b708caf8a9b08fa5/Source/TopMenu.cs#L98-L114)
