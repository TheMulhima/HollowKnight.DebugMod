# Basic Structure of Adding to Debug

1. Reference DebugMod.dll from mods folder (Note: if you follow steps here, the mod won't fail to load if debug isn't present)
2. In the mod's initialize add this code to check if debug exists:
 ```
if (ModHooks.GetMod("DebugMod") is Mod)
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
1. In `AddStuffToDebug` call the function `DebugMod.DebugMod.AddTopMenuContent`. It takes in 2 parameters, the menu name and the List of `TopMenuButton`. This list will contain the buttons that will be added to the menu.
2. In the list you have you have 2 options of buttons, a text buttons or an image button
  1. To add a text button to the menu, call new TextButton to create a text button. The parameters it takes is the string MenuName and the UnityAction<string> which is the action that will run when the button is clicked
  2. To add an image button to the menu, call new ImageButton to create an image button. The parameters it takes is the Texture2D which will be the image shown to user and the UnityAction<string> which is the action that will run when the button is clicked

so in the end the code should look something like this
```
DebugMod.DebugMod.AddTopMenuContent("TestMenu",
	new List<TopMenuButton>()
	{
		 new TextButton("Button 1", _ => { DebugMod.Console.AddLine("Run any code here"); }),
		 new ImageButton(Image, _ => { DebugMod.Console.AddLine("Run any code here"); }),
	});
```

For How the menu will be structured, Debug willtake reach entry in the list in the order it was added and place it into the menu as per the following rules:1 text button per row and 2 image buttons per row. note that for 2 images to appear in the same row, they'll need to be added next to each other in the list
			