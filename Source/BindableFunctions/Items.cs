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
        [BindableMethod(name = "Give Lantern", category = "Items")]
        public static void ToggleLantern()
        {
            if (!PlayerData.instance.hasLantern)
            {
                PlayerData.instance.hasLantern = true;
                Console.AddLine("Giving player lantern");
            }
            else
            {
                PlayerData.instance.hasLantern = false;
                Console.AddLine("Taking away lantern");
            }
        }

        [BindableMethod(name = "Give Tram Pass", category = "Items")]
        public static void ToggleTramPass()
        {
            if (!PlayerData.instance.hasTramPass)
            {
                PlayerData.instance.hasTramPass = true;
                Console.AddLine("Giving player tram pass");
            }
            else
            {
                PlayerData.instance.hasTramPass = false;
                Console.AddLine("Taking away tram pass");
            }
        }

        [BindableMethod(name = "Give Map & Quill", category = "Items")]
        public static void ToggleMapQuill()
        {
            if (!PlayerData.instance.hasQuill || !PlayerData.instance.hasMap)
            {
                PlayerData.instance.hasQuill = true;
                PlayerData.instance.hasMap = true;
                PlayerData.instance.mapDirtmouth = true;
                Console.AddLine("Giving player map & quill");
            }
            else
            {
                PlayerData.instance.hasQuill = false;
                PlayerData.instance.hasMap = false;
                Console.AddLine("Taking away map & quill");
            }
        }

        [BindableMethod(name = "Give City Crest", category = "Items")]
        public static void ToggleCityKey()
        {
            if (!PlayerData.instance.hasCityKey)
            {
                PlayerData.instance.hasCityKey = true;
                Console.AddLine("Giving player city crest");
            }
            else
            {
                PlayerData.instance.hasCityKey = false;
                Console.AddLine("Taking away city crest");
            }
        }

        [BindableMethod(name = "Give Shopkeeper's Key", category = "Items")]
        public static void ToggleSlyKey()
        {
            if (!PlayerData.instance.hasSlykey)
            {
                PlayerData.instance.hasSlykey = true;
                Console.AddLine("Giving player shopkeeper's key");
            }
            else
            {
                PlayerData.instance.hasSlykey = false;
                Console.AddLine("Taking away shopkeeper's key");
            }
        }

        [BindableMethod(name = "Give Elegant Key", category = "Items")]
        public static void ToggleElegantKey()
        {
            if (!PlayerData.instance.hasWhiteKey)
            {
                PlayerData.instance.hasWhiteKey = true;
                PlayerData.instance.usedWhiteKey = false;
                Console.AddLine("Giving player elegant key");
            }
            else
            {
                PlayerData.instance.hasWhiteKey = false;
                Console.AddLine("Taking away elegant key");
            }
        }

        [BindableMethod(name = "Give Love Key", category = "Items")]
        public static void ToggleLoveKey()
        {
            if (!PlayerData.instance.hasLoveKey)
            {
                PlayerData.instance.hasLoveKey = true;
                Console.AddLine("Giving player love key");
            }
            else
            {
                PlayerData.instance.hasLoveKey = false;
                Console.AddLine("Taking away love key");
            }
        }

        [BindableMethod(name = "Give Kingsbrand", category = "Items")]
        public static void ToggleKingsbrand()
        {
            if (!PlayerData.instance.hasKingsBrand)
            {
                PlayerData.instance.hasKingsBrand = true;
                Console.AddLine("Giving player kingsbrand");
            }
            else
            {
                PlayerData.instance.hasKingsBrand = false;
                Console.AddLine("Taking away kingsbrand");
            }
        }

        [BindableMethod(name = "Give Delicate Flower", category = "Items")]
        public static void ToggleXunFlower()
        {
            if (!PlayerData.instance.hasXunFlower || PlayerData.instance.xunFlowerBroken)
            {
                PlayerData.instance.hasXunFlower = true;
                PlayerData.instance.xunFlowerBroken = false;
                Console.AddLine("Giving player delicate flower");
            }
            else
            {
                PlayerData.instance.hasXunFlower = false;
                Console.AddLine("Taking away delicate flower");
            }
        }

        [BindableMethod(name = "Open All Stags", category = "Items")]
        public static void AllStags()
        {
            PlayerData playerData = PlayerData.instance;
            playerData.SetBool("openedTown",true);
            playerData.SetBool("openedTownBuilding", true);
            playerData.SetBool("openedCrossroads", true);
            playerData.SetBool("openedGreenpath", true);
            playerData.SetBool("openedRuins1", true);
            playerData.SetBool("openedRuins2", true);
            playerData.SetBool("openedFungalWastes", true);
            playerData.SetBool("openedRoyalGardens", true);
            playerData.SetBool("openedRestingGrounds", true);
            playerData.SetBool("openedDeepnest", true);
            playerData.SetBool("openedStagNest", true);
            playerData.SetBool("openedHiddenStation", true);
            playerData.SetBool("gladeDoorOpened", true);
            playerData.SetBool("troupeInTown", true);
            
            Console.AddLine("Unlocked all stags");
        }
    }
}