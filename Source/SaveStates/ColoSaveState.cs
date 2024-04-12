using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using DebugMod.Hitbox;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IL.HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    //Stored in separate class due to the amount of unique functionality
    internal static class ColoSaveState
    {

        #region Arena Dictionaries

        static Dictionary<string, (string Arena, bool Spikes, string Respawn)> BronzeArenas = new Dictionary<string, (string Arena, bool Spikes, string Respawn)>()
        {
            {"Wave 4", ("Arena 1", false, "Respawn 1")},
            //-----------------Arena 2------------------
            {"Wave 7", ("Arena 2", false, "Respawn 1")},
            {"Respawn 1", ("Arena 2", false, "Respawn 1")},
            {"Arena 3", ("Arena 2", false, "Respawn 1")},
            {"Wave 8", ("Arena 3", false, "Respawn 1")},
            {"Arena 4", ("Arena 3", true, "Respawn 1")},
            {"Wave 9", ("Arena 4", true, "Respawn 1")},
            {"Wave 10", ("Arena 4", true, "Respawn 1")},
            {"Reset 2", ("Arena 4", true, "Respawn 1")},
            {"Arena 6", ("Reset 2", true, "Respawn 1")},
            {"Wave 11", ("Arena 6", true, "Respawn 1")},
            {"Wave 12", ("Arena 6", true, "Respawn 1")},
            {"Wave 13", ("Arena 6", true, "Respawn 1")},
            {"Arena 7", ("Arena 6", true, "Respawn 1")},
            {"Wave 14", ("Arena 7", true, "Respawn 1")},
            {"Pause 1", ("Arena 7", true, "Respawn 1")},
            {"Wave 15", ("Arena 7", true, "Respawn 1")},
            {"Pause 2", ("Arena 7", true, "Respawn 1")},
            //-----------------Reset 3------------------
            //-----------------Arena 8------------------
            {"Wave 23", ("Arena 8", true, "Respawn 1")},
            {"Wave 24", ("Arena 8", true, "Respawn 1")},
            {"Wave 25", ("Arena 8", true, "Respawn 1")},
            {"Gruz Arena", ("Arena 8", true, "Respawn 1")},
            {"Wave 26", ("Gruz Arena", true, "Respawn 1")},
            {"Wave 27", ("Gruz Arena", true, "Respawn 1")},
            {"Wave 28", ("Gruz Arena", true, "Respawn 1")},

        };

        static Dictionary<string, (string Arena, bool Spikes, string Respawn)> SilverArenas = new Dictionary<string, (string Arena, bool Spikes, string Respawn)>()
        {
            {"Wave 4", ("Arena 1", false, "Respawn 1")},
            {"Respawn 1",("Arena 1", false, "Respawn 1")},
            {"Spikes",("Arena 1", false, "Respawn 1" )},
            {"Wave 5", ("Arena 1", true, "Respawn 1")},
            {"Arena 2",("Arena 1", true, "Respawn 1")},
            {"Wave 6", ("Arena 2", true, "Respawn 1")},
            {"Wave 7", ("Arena 2", true, "Respawn 1")},
            {"Arena 3",("Arena 2", true, "Respawn 2")},
            {"Wave 8", ("Arena 3", true, "Respawn 1")},
            {"Wave 9", ("Arena 3", true, "Respawn 1")},
            {"Wave 10", ("Arena 3", true, "Respawn 1")},
            {"Wave 11", ("Arena 3", true, "Respawn 1")},
            {"Arena 4",("Arena 3", true, "Respawn 1")},
            {"Wave 12", ("Arena 4", true, "Respawn 1")},
            {"Wave 13", ("Arena 4", true, "Respawn 1")},
            {"Wave 14", ("Arena 4", true, "Respawn 1")},
            {"Wave 15", ("Arena 4", true, "Respawn 1")},
            {"Reset 1", ("Arena 4", true, "Respawn 1")},
            {"Wave 16", ("Reset 1", true, "Respawn 1")},
            //--------------Hopper Arena----------------
            {"Wave 30", ("Hopper Arena", false, "Respawn 1")},
            {"Wave 31", ("Hopper Arena", false, "Respawn 1")},
            {"Hoppering",("Hopper Arena", true, "Respawn 1")},
            {"Hopper Check",("Hopper Arena", true, "Respawn 1")},
            {"Wave 32", ("Hopper Arena", false, "Respawn 1")},
            {"Arena 8",("Hopper Arena", true, "Respawn 1")},
            {"Wave 33", ("Arena 8", false, "Respawn 1")},
            {"Cheer 1",("Arena 8", true, "Respawn 1")},
            {"Arena 5",("Arena 8", true, "Respawn 1")},
            {"Wave 20", ("Arena 5", false, "Respawn 1")},
            {"Cheer",("Arena 5", true, "Respawn 1")},
            {"Arena 6",("Arena 5", true, "Respawn 1")},
            {"Wave 21", ("Arena 6", false, "Respawn 1")},
            //-----------------Reset 2------------------
            //-----------------Arena 7------------------
            {"Wave 25", ("Arena 7", false, "Respawn 1")},
            {"Wave 26", ("Arena 7", false, "Respawn 1")},
            {"Respawn 2",("Arena 7", false, "Respawn 1")},
            {"Spikes 2",("Arena 7", false, "Respawn 2")},
            {"Wave 27", ("Arena 7", true, "Respawn 2")},
            {"Wave 28", ("Arena 7", true, "Respawn 2")},
            {"Wave 29", ("Arena 7", true, "Respawn 2")},
            //-----------------Reset 3------------------
            //-----------------Arena 9------------------
            {"Respawn 3",("Arena 9", true, "Respawn 2")},
            {"Wave 34", ("Arena 9", true, "Respawn 3")},
            {"Wave 35", ("Arena 9", true, "Respawn 3")},
            //-----------------Reset 4------------------
            //--------------Arena 9 Obble---------------
            {"Wave 26 Obble", ("Arena 9 Obble", false, "Respawn 3")},
            {"Wave 27 Obble", ("Arena 9 Obble", false, "Respawn 3")},
            {"Respawn 2 Obble",("Arena 9 Obble", false, "Respawn 3")},
            {"Spikes Up Obble",("Arena 9 Obble", false, "Respawn 2 Obble")},
            {"Wave 28 Obble", ("Arena 9 Obble", true, "Respawn 2 Obble")},
            //--------------Reset 4 Obble---------------
            //-----------Arena 9 Final Obble------------
            {"Wave 30 Obble", ("Arena Final Obble", false, "Respawn 2 Obble")},

        };

        static Dictionary<string, (string Arena, bool Spikes, string Respawn)> GoldArenas = new Dictionary<string, (string Arena, bool Spikes, string Respawn)>()
        {
            //Arena 1
            {"Wave 5", ("Arena 1", false, "Respawn 1")},
            {"Ceiling", ("Arena 1", false, "Respawn 1")},
            //Arena 1, Ceiling 1
            {"Wave 6", ("Ceiling 1", true, "Respawn 1")},
            {"Wave 7", ("Ceiling 1", true, "Respawn 1")},
            {"Reset 1", ("Ceiling 1", true, "Respawn 1")},
            //Reset 1 (just set Ceiling 1)
            {"GC 6", ("Reset 1", false, "Respawn 1")},
            {"Wave 8", ("Reset 1", false, "Respawn 1")},
            //Reset 2, first loodles

            //Arena ColHopper
            {"Wave 10", ("Arena ColHopper", true, "Respawn 1")},
            //Reset 3, last loodles

            //Arena Totem
            {"Wave 12", ("Arena Totem", true, "Respawn 1")},
            {"Wave 13", ("Arena Totem", true, "Respawn 1")},
            {"Wave 14", ("Arena Totem", true, "Respawn 1")},
            {"Wave 15", ("Arena Totem", true, "Respawn 1")},
            {"Garpedes 1", ("Arena Totem", true, "Respawn 1")},
            {"Wave 16", ("Arena Totem", true, "Respawn 1")},
            //Reset 4

            //Arena Mage
            {"Wave 25", ("Arena Mage", true, "Respawn 1")},
            {"w26 Pause", ("Arena Mage", true, "Respawn 1")},
            {"Wave 26", ("Arena Mage", true, "Respawn 1")},
            {"w27 Pause", ("Arena Mage", true, "Respawn 1")},
            {"Wave 27", ("Arena Mage", true, "Respawn 1")},
            {"w28 Pause", ("Arena Mage", true, "Respawn 1")},
            {"Wave 28", ("Arena Mage", true, "Respawn 1")},
            //Reset 5

            //Wave 30 Arena (fix this later, current implementation would need more hardcoding

            //Walls In
            {"Garpedes 2", ("Walls In", false, "Respawn 1")},
            {"Garpedes 3", ("Walls In", false, "Respawn 1")},
            {"GC Pause 1", ("Walls In", false, "Respawn 1")},
            {"GC 7", ("Walls In", false, "Respawn 1")},
            //Spike Pit
            {"Respawn 2", ("Spike Pit", false, "Respawn 1")},
            {"Spikes 2", ("Spike Pit", false, "Respawn 2")},
            //Spike Pit, Spikes 2
            {"GC 3", ("Spike Pit", true, "Respawn 2")},
            {"Wave 34", ("Spike Pit", true, "Respawn 2")},
            {"Wave 35", ("Spike Pit", true, "Respawn 2")},
            {"Wave 36", ("Spike Pit", true, "Respawn 2")},
            {"Wave 37", ("Spike Pit", true, "Respawn 2")},
            {"Wave 38", ("Spike Pit", true, "Respawn 2")},
            {"Wave 39", ("Spike Pit", true, "Respawn 2")},
            {"Wave 40", ("Spike Pit", true, "Respawn 2")},
            {"Arena Barrage", ("Spike Pit", true, "Respawn 2")},
            //Spikes 2, Arena Barrage
            {"Wave 41", ("Arena Barrage", true, "Respawn 2")},
            //Reset 6


        };

        #endregion

        public static string[] coloScenes =
        {
            //Colos (0-2)
            "room_colosseum_bronze",
            "room_colosseum_silver",
            "room_colosseum_gold"
            //Panths ()
        };

        public static string[] ignoreWaves =
        {
            null,
            "Init",
            "Idle",
        };


        public static string SaveColoScene(string curScene)
        {
            switch (curScene)
            {
                case "room_colosseum_bronze": //0
                    return GrabCurrentWave("Bronze");

                case "room_colosseum_silver": //1
                    return GrabCurrentWave("Silver");

                case "room_colosseum_gold": //2
                    return GrabCurrentWave("Gold");
                default:
                    DebugMod.instance.LogError("Saved Scene has no Special Definition");
                    break;
            }
            return null;
        }
        public static void LoadColoScene(string curScene, string wave)
        {
            string startWave;
            bool isColoScene = coloScenes.Contains(curScene);
            if (isColoScene && (!ignoreWaves.Contains(wave)))
            {
                startWave = wave;
            }
            else { startWave = null; }
            switch (curScene)
            {
                case "room_colosseum_bronze": //0
                    ChangeColoWave("Bronze", startWave);
                    break;

                case "room_colosseum_silver": //1
                    ChangeColoWave("Silver", startWave);
                    break;

                case "room_colosseum_gold": //2
                    ChangeColoWave("Gold", startWave);
                    break;


                default:
                    DebugMod.instance.LogError("Saved Scene has no Special Definition");
                    break;
            }
        }
        private static string GrabCurrentWave(string coloLevel)
        {
            try
            {
                GameObject waveController = GameObject.Find("Colosseum Manager");
                PlayMakerFSM waveFSM = waveController.LocateMyFSM("Battle Control");
                string wave = waveFSM.ActiveStateName;
                Console.AddLine("Saving Colo " + coloLevel + ", " + wave);
                if (!ignoreWaves.Contains(wave)) return wave;
                
            }
            catch (Exception e)
            {
                Console.AddLine("Failed to grab colo wave: \n" + e);
            }
            return null;
        }
        private static void ChangeColoWave(string coloLevel, string startWave)
        {
            if (ignoreWaves.Contains(startWave)) return;
            GameObject waveController = GameObject.Find("Colosseum Manager");
            PlayMakerFSM fsm = waveController.LocateMyFSM("Battle Control");
            FsmState idle = fsm.FsmStates.First(t => t.Name == "Idle");
            FsmState newWave = fsm.FsmStates.First(t => t.Name == startWave);
            (FsmState waveSplit, string splitEvent) = (idle, "WAVES START");

            switch (coloLevel)
            {
                case ("Bronze"):
                    (waveSplit, splitEvent) = BroadcastBronzeColoArena(startWave, newWave, idle, fsm);
                    break;
                case ("Silver"):
                    (waveSplit, splitEvent) = BroadcastSilverColoArena(startWave, newWave, idle, fsm);
                    break;
                case ("Gold"):
                    (waveSplit, splitEvent) = BroadcastGoldColoArena(startWave, newWave, idle, fsm);
                    break;
                default:
                    DebugMod.instance.LogError("No Colo Level Found @ ChangeColoWave");
                    break;
            }
            waveSplit.Transitions.First(tr => tr.EventName == splitEvent).ToFsmState = newWave;
            Console.AddLine("Changing " + coloLevel + " to start at " + startWave);
            
        }

        #region Arena Broadcasts

        private static (FsmState, string) BroadcastBronzeColoArena(string startWave, FsmState newWave, FsmState idle, PlayMakerFSM fsm)
        {
            if (BronzeArenas.ContainsKey(startWave))
            {;
                string arenaName = BronzeArenas[startWave].Arena;
                FsmState currentArena = idle;
                FsmState reset2 = fsm.FsmStates.First(t => t.Name == "Reset 2");
                FsmState arena1 = fsm.FsmStates.First(t => t.Name == "Arena 1");
                FsmState arena2 = fsm.FsmStates.First(t => t.Name == "Arena 2");
                FsmState arena3 = fsm.FsmStates.First(t => t.Name == "Arena 3");
                FsmState arena4 = fsm.FsmStates.First(t => t.Name == "Arena 4");
                FsmState arena6 = fsm.FsmStates.First(t => t.Name == "Arena 6");
                FsmState arena7 = fsm.FsmStates.First(t => t.Name == "Arena 7");
                FsmState arena8 = fsm.FsmStates.First(t => t.Name == "Arena 8");
                FsmState gruzArena = fsm.FsmStates.First(t => t.Name == "Gruz Arena");
                FsmState respawn = fsm.FsmStates.First(t => t.Name == BronzeArenas[startWave].Respawn);
                bool doSpikes = BronzeArenas[startWave].Spikes;
                switch (arenaName)
                {
                    //Arena 1 Default
                    case ("Arena 1"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        currentArena = arena1;
                        break;

                    //Reset 1
                    case ("Arena 2"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena2;
                        currentArena = arena2;
                        break;
                    case ("Arena 3"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = respawn;
                        respawn.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        currentArena = arena3;
                        break;
                    case ("Arena 4"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = respawn;
                        respawn.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena4;
                        currentArena = arena4;
                        break;
                    case ("Reset 2"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = respawn;
                        respawn.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena4;
                        arena4.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = reset2;
                        currentArena = reset2;
                        break;
                    case ("Arena 6"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = respawn;
                        respawn.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena4;
                        arena4.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = reset2;
                        reset2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena6;
                        currentArena = arena6;
                        break;

                    case ("Arena 7"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = respawn;
                        respawn.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena4;
                        arena4.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = reset2;
                        reset2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena6;
                        arena6.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena7;
                        currentArena = arena7;

                        break;

                    //Reset 3
                    case ("Arena 8"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena8;
                        currentArena = arena8;
                        break;

                    //Reset 4
                    case ("Gruz Arena"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena8;
                        arena8.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = gruzArena;
                        currentArena = gruzArena;
                        break;
                }
                return (currentArena, "FINISHED");
            }
            return (idle, "WAVES START");
        }
        private static (FsmState, string) BroadcastSilverColoArena(string startWave, FsmState newWave, FsmState idle, PlayMakerFSM fsm)
        {
            if (SilverArenas.ContainsKey(startWave))
            {
                string arenaName = SilverArenas[startWave].Arena;
                FsmState currentArena;
                FsmState arena1 = fsm.FsmStates.First(t => t.Name == "Arena 1");
                FsmState arena2 = fsm.FsmStates.First(t => t.Name == "Arena 2");
                FsmState arena3 = fsm.FsmStates.First(t => t.Name == "Arena 3");
                FsmState arena4 = fsm.FsmStates.First(t => t.Name == "Arena 4");
                FsmState reset1 = fsm.FsmStates.First(t => t.Name == "Reset 1");
                FsmState hopperArena = fsm.FsmStates.First(t => t.Name == "Hopper Arena");
                FsmState arena8 = fsm.FsmStates.First(t => t.Name == "Arena 8");
                FsmState arena5 = fsm.FsmStates.First(t => t.Name == "Arena 5");
                FsmState arena6 = fsm.FsmStates.First(t => t.Name == "Arena 6");
                FsmState arena7 = fsm.FsmStates.First(t => t.Name == "Arena 7");
                FsmState arena9 = fsm.FsmStates.First(t => t.Name == "Arena 9");
                FsmState arena9Obble = fsm.FsmStates.First(t => t.Name == "Arena 9 Obble");
                FsmState arenaFinalObble = fsm.FsmStates.First(t => t.Name == "Arena Final Obble");
                FsmState respawn = fsm.FsmStates.First(t => t.Name == SilverArenas[startWave].Respawn);
                FsmState spikes = fsm.FsmStates.First(t => t.Name == "Spikes");
                bool doSpikes = SilverArenas[startWave].Spikes;
                switch (arenaName)
                {
                    //Arena 1 Default
                    case ("Arena 1"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        currentArena = arena1;
                        break;
                    case ("Arena 2"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        arena1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        currentArena = arena2;
                        break;
                    case ("Arena 3"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        arena1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        currentArena = arena3;
                        break;
                    case ("Arena 4"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        arena1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena4;
                        currentArena = arena4;
                        break;
                    case ("Reset 1"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        arena1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena2;
                        arena2.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena3;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena4;
                        arena3.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = reset1;
                        currentArena = reset1;
                        break;

                    //Reset 1
                    case ("Hopper Arena"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = hopperArena;
                        currentArena = hopperArena;
                        break;
                    case ("Arena 8"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = hopperArena;
                        hopperArena.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena8;
                        currentArena = arena8;
                        break;
                    case ("Arena 5"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = hopperArena;
                        hopperArena.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena8;
                        arena8.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena5;
                        currentArena = arena5;
                        break;
                    case ("Arena 6"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = hopperArena;
                        hopperArena.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena8;
                        arena8.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena5;
                        arena5.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = arena6;
                        currentArena = arena6;
                        break;

                    //Reset 2
                    case ("Arena 7"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena7;
                        currentArena = arena7;
                        break;

                    //Reset 3
                    case ("Arena 9"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena9;
                        currentArena = arena9;
                        break;

                    //Reset 4
                    case ("Arena 9 Obble"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena9Obble;
                        currentArena = arena9Obble;
                        break;

                    //Reset 4 Obble
                    case ("Arena Final Obble"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arenaFinalObble;
                        currentArena = arenaFinalObble;
                        break;
                    default:
                        currentArena = arena1;
                        DebugMod.instance.LogError("No Arena Found @ Colo 2 Arena Broadcaster");
                        break;
                }
                currentArena.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = respawn;
                    if (doSpikes)
                    {
                            respawn.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = spikes;
                            return (spikes, "FINISHED");
                    }
                    else return (respawn, "FINISHED");
            }
            return (idle, "WAVES START");
        }
        private static (FsmState, string) BroadcastGoldColoArena(string startWave, FsmState newWave, FsmState idle, PlayMakerFSM fsm)
        {
            if (GoldArenas.ContainsKey(startWave))
            {
                string arenaName = GoldArenas[startWave].Arena;
                FsmState currentArena = idle;
                FsmState arena1 = fsm.FsmStates.First(t => t.Name == "Arena 1");
                FsmState ceiling1 = fsm.FsmStates.First(t => t.Name == "Ceiling 1");
                FsmState reset1 = fsm.FsmStates.First(t => t.Name == "Reset 1");
                FsmState arenaColHopper = fsm.FsmStates.First(t => t.Name == "Arena ColHopper");
                FsmState arenaTotem = fsm.FsmStates.First(t => t.Name == "Arena Totem");
                FsmState arenaMage = fsm.FsmStates.First(t => t.Name == "Arena Mage");
                FsmState wallsIn = fsm.FsmStates.First(t => t.Name == "Walls In");
                FsmState spikePit = fsm.FsmStates.First(t => t.Name == "Spike Pit");
                FsmState arenaBarrage = fsm.FsmStates.First(t => t.Name == "Arena Barrage");
                FsmState respawn = fsm.FsmStates.First(t => t.Name == GoldArenas[startWave].Respawn);
                bool doSpikes = GoldArenas[startWave].Spikes;
                switch (arenaName)
                {
                    //Arena 1 Default
                    case ("Arena 1"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        currentArena = arena1;
                        break;
                    case ("Ceiling 1"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        arena1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = ceiling1;
                        currentArena = ceiling1;

                        break;
                    case ("Reset 1"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arena1;
                        arena1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = ceiling1;
                        ceiling1.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = reset1;
                        currentArena = reset1;
                        break;
                    case ("Arena ColHopper"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arenaColHopper;
                        currentArena = arenaColHopper;
                        break;
                    case ("Arena Totem"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arenaTotem;
                        currentArena = arenaTotem;
                        break;
                    case ("Arena Mage"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arenaMage;
                        currentArena = arenaMage;
                        break;
                    case ("Walls In"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = wallsIn;
                        currentArena = wallsIn;
                        break;
                    case ("Spike Pit"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = spikePit;
                        currentArena = spikePit;
                        break;
                    case ("Arena Barrage"):
                        idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = arenaBarrage;
                        currentArena = arenaBarrage;
                        break;
                    default:
                        currentArena = arena1;
                        DebugMod.instance.LogError("No Arena Found @ Colo 3 Arena Broadcaster");
                        break;
                }
                currentArena.Transitions.First(tr => tr.EventName == "FINISHED").ToFsmState = respawn;
                if (doSpikes)
                {
                    GameManager.instance.StartCoroutine(SpikeSetterCoro(startWave, fsm));
                }
                return (respawn, "FINISHED");
            }
            return (idle, "WAVES START");
        }

        private static IEnumerator SpikeSetterCoro(string wave, PlayMakerFSM coloFsm)
        {
            yield return new WaitUntil(() => coloFsm.ActiveStateName == wave);
            GameObject spikeObj = GameObject.Find("GO UP Message");
            PlayMakerFSM spikeFsm = spikeObj.LocateMyFSM("Control");
            spikeFsm.SendEvent("EXPAND");
            PlayMakerFSM.BroadcastEvent("EXPAND");
        }
        

        #endregion
    }
}
