----------------------------------------------------------------------------------------
                 Changelog from Debug v1.3.6 (Previous Major 1.4.3.2 Version)
----------------------------------------------------------------------------------------
** 1221 => CP. Credits: cerpin
    * Added multiple savestate and loading/saving savestates from files (took someone long enough lol. 1221 debug had it since forever)
    * Added a minimal info panel
    * Added a front and back button on keybinds panel
    * Greatly improved hitbox viewer in debug. Credits: DemoJameson#1195
* Improved Timescale feature and made it not reset timescale after getting hit or pausing
* Made sure Timescale increments by 0.1 on each press after timescale > 10.
* Increased accuracy of Player Position on lower info panel.
* Removed ability to bind mouse1
* Pressing Escape when rebinding a key now unbinds it (basically addds ability to unbind keys ingame).
* Fixed voidheart (the issue of hornet not showing up in thk phase 4 if given void heart using the mod)
* Fixed delicate flower
** Added a new binds:
    * Reset Settings
    * Move Left/Right/Up/Down by a specific amount (default 0.1)
    * Face Left/Right
    * Remove all charms, Give all stags, Add/remove mask/vessel
    * Give dreamers, Give/take soul/health
    * shade spawn location and max travel range
    * Save/Load Keybinds to file
    * "freeze game" that pauses game without showing UI
* Made a new options in cheats menu that doesnt allow keybinds to be used (except for the toggle all ui key bind)
* Made self damage work without enemies in scene and panel open
* A secret EasterEgg
* Add ability for other mods to easily add to debug's menus
* Make 1.4 and 1.5 debug have same features
* Changes from pseudorandomhk
    * Add savestate support for MMS-style noclip
    * Added bindable functions for:
        * toggling bench storage
        * toggling vanilla noclip,
        * giving dreamgate invulnerability (invincibility + can't pick up geo + no roar stun)
        * Clear white screen
* Add options in top menu to open:
    * Saves folder
    * Mods Folder
* Incrementing GrimmChild now spawns the new and correct grimmchild. also changes charm cost if goes to carefree
* Incrementing Kingsoul changes charmcost to the correctvalue
----------------------------------------------------------------------------------------
                            Debug v1.4.8
----------------------------------------------------------------------------------------
* Multiple save state pages Credits: Magnetic Pizza (and jhearom for porting to cp)
* New Bindable Functions. Credit:Flib 
  * HookResetCurrentScene: Reset all scene data in this scene (geo rocks unbroken, breakable walls unbroken, levers unflipped, elevators at default positions, etc).
  * HookBlockCurrentSceneChanges: Any scenedata changes made since entering the current scene won't be saved.
* Add buttons to directly run bindable actions. Credits: Flukeball
* Changes to respawn point hitbox color (Credits: Dandy)
* New bindable function to recover shade (Credits: Flib)
* Fix shade spawn point not disappearing
* Fix Kill All, Hazard Respawn to work while the game is paused and Hitbox Viewer's circle colliders and objects with a nonzero z coordinate (Credits: Flib)
* Revamp TopMenu Addition code, 
* Add Frame by frame advance bindable function (Credits: SFGrenade)
* Update sanic's code to make it better (Credits: 56)
