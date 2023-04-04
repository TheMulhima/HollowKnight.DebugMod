using System;
using System.Collections;
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
    internal static class SpecialSavestate
    {
        public static string[] specialScenes =
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


        public static void SaveSpecialScene(string curScene, SaveState.SaveStateData _data)
        {
            switch (curScene)
            {
                case "Room_Colosseum_Bronze": //0
                    _data.isColoScene = true;
                    _data.roomSpecificData = GrabCurrentWave("Bronze", _data);
                    break;

                case "Room_Colosseum_Silver": //1
                    _data.isColoScene = true;
                    _data.roomSpecificData = GrabCurrentWave("Silver", _data);
                    break;

                case "Room_Colosseum_Gold": //2
                    _data.isColoScene = true;
                    _data.roomSpecificData = GrabCurrentWave("Gold", _data);
                    break;


                default:
                    Console.AddLine("Saved Scene has no Special Definition");
                    break;


            }
        }

        public static void LoadSpecialScene(string curScene, SaveState.SaveStateData _data)
        {
            string startWave;
            if (_data.isColoScene && (!specialScenes.Contains(_data.roomSpecificData)))
            {
                startWave = _data.roomSpecificData;
            }
            else { startWave = null; }
            switch (curScene)
            {
                case "Room_Colosseum_Bronze": //0
                    OverrideroomSpecificData("Bronze", startWave, _data);
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

        private static string GrabCurrentWave(string coloLevel, SaveState.SaveStateData _data)
        {

            GameObject waveController = GameObject.Find("Colosseum Manager");
            PlayMakerFSM waveFSM = waveController.LocateMyFSM("Battle Control");
            string wave = waveFSM.ActiveStateName;
            if (!ignoreWaves.Contains(wave)) return wave;
            else return null;

        }

        private static void OverrideroomSpecificData(string coloLevel, string startWave, SaveState.SaveStateData _data)
        {
            if (ignoreWaves.Contains(startWave)) return;
            GameObject waveController = GameObject.Find("Colosseum Manager");
            PlayMakerFSM fsm = waveController.LocateMyFSM("Battle Control");
            FsmState idle = fsm.FsmStates.First(t => t.Name == "Idle");
            FsmState newWave = fsm.FsmStates.First(t => t.Name == startWave);
            idle.Transitions.First(tr => tr.EventName == "WAVES START").ToFsmState = newWave;
            BroadcastColoArena(coloLevel, startWave, _data);

        }

        private static void BroadcastColoArena(string coloLevel, string startWave, SaveState.SaveStateData _data)
        {

        }
    }
}
