using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DebugMod.Hitbox;
using DebugMod.MonoBehaviours;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Increment Scream", category = "Spells")]
        public static void IncreaseScreamLevel()
        {
            if (PlayerData.instance.screamLevel >= 2)
            {
                PlayerData.instance.screamLevel = 0;
            }
            else
            {
                PlayerData.instance.screamLevel++;
            }
        }

        [BindableMethod(name = "Increment Fireball", category = "Spells")]
        public static void IncreaseFireballLevel()
        {
            if (PlayerData.instance.fireballLevel >= 2)
            {
                PlayerData.instance.fireballLevel = 0;
            }
            else
            {
                PlayerData.instance.fireballLevel++;
            }
        }

        [BindableMethod(name = "Increment Quake", category = "Spells")]
        public static void IncreaseQuakeLevel()
        {
            if (PlayerData.instance.quakeLevel >= 2)
            {
                PlayerData.instance.quakeLevel = 0;
            }
            else
            {
                PlayerData.instance.quakeLevel++;
            }
        }
    }
}