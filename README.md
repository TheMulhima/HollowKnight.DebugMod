----------------------------------------------------------------------------------------
                                       FEATURES
----------------------------------------------------------------------------------------
* A completely new toggleable UI in game that provides the following functions:
* Cheats such as invincibility and noclip
* The ability to unlock all charms or repair broken ones
* Change which skills the player has
* Change which items the player has
* Give the player more of consumable resources such as geo and essence
* Respawn bosses
* Hold multiple dream gate positions
* Change the player's respawn point to anywhere in the current scene
* Recall to the set respawn point
* Kill all enemies
* Add HP bars to enemies
* Draw collision boxes for enemies
* Clone or delete any enemy
* Set an enemy's health to 9999
* Change the player's nail damage
* Damage the player
* Change the camera zoom level
* Disable the in game HUD
* Make the player invisible
* Disable the lighting around the player
* Disable the vignette drawn around the player
* Change the time scale of the game
----------------------------------------------------------------------------------------
                                  INSTALLATION (STEAM, WINDOWS)
----------------------------------------------------------------------------------------
1) Download the modding API from here: https://drive.google.com/open?id=0B_b9PFqx_PR9X1ZrWGFxUGdydTg
2) Right click Hollow Knight in Steam -> Properties -> Local Files -> Browse Local Files
3) Create a backup of the game files located here
4) Copy the contents of the modding API zip into this folder (Overwrite files when asked)
5) Copy the contents of this zip into the folder (Overwrite files when asked)
6) This mod should not affect saves negatively, but it is a good idea to back them up anyway.
   Saves are located at %AppData%\..\LocalLow\Team Cherry\Hollow Knight\
----------------------------------------------------------------------------------------
                                          CREDITS
----------------------------------------------------------------------------------------
Coding - Seanpr<br />
SaveStates/Old Current Patch - 56<br />
UI design and graphics - The Embraced One<br />
Assistance with canvas - KDT<br />
1.5 and A lot of Changes - Mulhima<br />
----------------------------------------------------------------------------------------
                                          Changelog from Debug v1.3.6
----------------------------------------------------------------------------------------
** 1221 => CP 
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