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
        [BindableMethod(name = "Give Pale Ore", category = "Consumables")]
        public static void GivePaleOre()
        {
            PlayerData.instance.ore = 6;
            Console.AddLine("Set player pale ore to 6");
        }

        [BindableMethod(name = "Give Simple Keys", category = "Consumables")]
        public static void GiveSimpleKey()
        {
            PlayerData.instance.simpleKeys = 3;
            Console.AddLine("Set player simple keys to 3");
        }

        [BindableMethod(name = "Give Rancid Eggs", category = "Consumables")]
        public static void GiveRancidEgg()
        {
            PlayerData.instance.rancidEggs += 10;
            Console.AddLine("Giving player 10 rancid eggs");
        }

        [BindableMethod(name = "Give Geo", category = "Consumables")]
        public static void GiveGeo()
        {
            HeroController.instance.AddGeo(1000);
            Console.AddLine("Giving player 1000 geo");
        }

        [BindableMethod(name = "Give Essence", category = "Consumables")]
        public static void GiveEssence()
        {
            PlayerData.instance.dreamOrbs += 100;
            Console.AddLine("Giving player 100 essence");
        }
        
        //yes i added dreamers here because its funny
        [BindableMethod(name = "Give Lurien", category = "Consumables")]
        public static void GiveLurien()
        {
            if (!PlayerData.instance.lurienDefeated)
            {
                PlayerData.instance.lurienDefeated = true;
                MakeDreamersWork("WATCHER");
                
            }
            Console.AddLine("Giving Lurien");
        }
        
        [BindableMethod(name = "Give Monomon", category = "Consumables")]
        public static void GiveMonomon()
        {
            if (!PlayerData.instance.monomonDefeated)
            {
                PlayerData.instance.monomonDefeated = true;
                MakeDreamersWork("TEACHER");
            }
            Console.AddLine("Giving Monomon");
        }
        
        [BindableMethod(name = "Give Herrah", category = "Consumables")]
        public static void GiveHerrah()
        {
            if (!PlayerData.instance.hegemolDefeated)
            {
                PlayerData.instance.hegemolDefeated = true;
                MakeDreamersWork("BEAST");
            }
            Console.AddLine("Giving Herrah");
        }

        private static void MakeDreamersWork(string AchievementToGive)
        {
            var pd = PlayerData.instance;
            
            pd.guardiansDefeated += 1;
            if (!pd.crossroadsInfected)
            {
                pd.crossroadsInfected = true;
                pd.visitedCrossroads = false;
            }

            if (pd.guardiansDefeated == 3)
            {
                pd.brettaState += 1;
                pd.mrMushroomState = 1;
                pd.corniferAtHome = true;
                pd.iseldaConvo1 = true;
                pd.dungDefenderSleeping = true;
                pd.corn_crossroadsLeft = true;
                
                pd.corn_fogCanyonLeft = true;
                pd.corn_fungalWastesLeft = true;
                pd.corn_cityLeft = true;
                pd.corn_waterwaysLeft = true;
                pd.corn_minesLeft = true;
                pd.corn_cliffsLeft = true;
                pd.corn_deepnestLeft = true;
                pd.corn_outskirtsLeft = true;
                pd.corn_royalGardensLeft = true;
                pd.corn_abyssLeft = true;
                pd.metIselda = true;
            }
            
            GameManager.instance.AwardAchievement(AchievementToGive);
        }

        [BindableMethod(name = "Add GrimmKin Flames", category = "Consumables")]
        public static void GrimmKinFlames()
        {
            if (PlayerData.instance.flamesCollected == 3) PlayerData.instance.flamesCollected = 0;
            else PlayerData.instance.flamesCollected += 1;
            Console.AddLine("Grimm kin flames incremented");
        } 
        
        
        private static string[] AllMapBools = new[]
        {
            "mapCrossroads",
            "mapGreenpath",
            "mapFogCanyon",
            "mapRoyalGardens",
            "mapFungalWastes",
            "mapCity",
            "mapWaterways",
            "mapMines",
            "mapDeepnest",
            "mapCliffs",
            "mapOutskirts",
            "mapRestingGrounds",
            "mapAbyss"
        };
        
        [BindableMethod(name = "Give All Maps", category = "Consumables")]
        public static void GiveAllMaps()
        {
            PlayerData.instance.hasMap = true;
            PlayerData.instance.mapAllRooms = true;
            PlayerData.instance.mapCrossroads = true;
            PlayerData.instance.mapGreenpath = true;
            PlayerData.instance.mapFogCanyon = true;
            PlayerData.instance.mapRoyalGardens = true;
            PlayerData.instance.mapFungalWastes = true;
            PlayerData.instance.mapCity = true;
            PlayerData.instance.mapWaterways = true;
            PlayerData.instance.mapMines = true;
            PlayerData.instance.mapDeepnest = true;
            PlayerData.instance.mapCliffs = true;
            PlayerData.instance.mapOutskirts = true;
            PlayerData.instance.mapRestingGrounds = true;
            PlayerData.instance.mapAbyss = true;
        }
    }
}