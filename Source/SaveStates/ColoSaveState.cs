using System;
using System.Collections;
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
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace DebugMod
{
    //Stored in separate class due to the amount of unique functionality
    internal static class ColoSaveState
    {
        public static string[] coloScenes =
        {
            //Colos (0-2)
            "Room_Colosseum_Bronze",
            "Room_Colosseum_Silver",
            "Room_Colosseum_Gold"
            //Panths ()


        };

        public static string[] ignoreWaves =
        {
            null,
            "Init",
            "Idle"
        };


        public static string SaveColoScene(string curScene)
        {
            switch (curScene)
            {
                case "Room_Colosseum_Bronze": //0
                    return GrabCurrentWave("Bronze");

                case "Room_Colosseum_Silver": //1
                    return GrabCurrentWave("Silver");

                case "Room_Colosseum_Gold": //2
                    return GrabCurrentWave("Gold");
                default:
                    Console.AddLine("Saved Scene has no Special Definition");
                    break;
            }
            return null;
        }

        public static void LoadSpecialScene(string curScene, SaveState.SaveStateData _data)
        {
            string startWave;
            bool isColoScene = coloScenes.Contains(curScene);
            if (isColoScene && (!ignoreWaves.Contains(_data.roomSpecificOptions)))
            {
                startWave = _data.roomSpecificOptions;
            }
            else { startWave = null; }
            switch (curScene)
            {
                case "Room_Colosseum_Bronze": //0
                    ChangeColoWave("Bronze", startWave);
                    break;

                case "Room_Colosseum_Silver": //1
                    break;

                case "Room_Colosseum_Gold": //2
                    break;


                default:
                    Console.AddLine("Saved Scene has no Special Definition");
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
            idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = newWave;
            BroadcastColoArena(coloLevel, startWave);
        }
        private static void BroadcastColoArena(string coloLevel, string startWave)
        {

        }
    }
}
