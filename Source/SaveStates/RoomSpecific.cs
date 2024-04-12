using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IL.HutongGames.PlayMaker.Actions;
using HutongGames;
using TeamCherry;
using UnityEngine;
using Modding.Utils;
using System.Drawing.Text;

namespace DebugMod
{
    public static class RoomSpecific
    {
        //This class is intended to recreate some scenarios, with more accuracy than that of the savestate class. 
        //This should be eventually included to compatible with savestates, stored in the same location for easier access.
        #region Rooms
        private static void EnterSpiderTownTrap(int index) //Deepnest_Spider_Town
        {
            string goName = "RestBench Spider";
            string websFsmName = "Fade";
            string benchFsmName = "Bench Control Spider";
            PlayMakerFSM websFSM = FindFsmGlobally(goName, websFsmName);
            PlayMakerFSM benchFSM = FindFsmGlobally(goName, benchFsmName);
            benchFSM.SetState("Start Rest");
            benchFSM.SendEvent("WAIT");
            benchFSM.SendEvent("FINISHED");
            benchFSM.SendEvent("STRUGGLE");
            websFSM.SendEvent("FIRST STRUGGLE");
            websFSM.SendEvent("FINISHED");
            websFSM.SendEvent("FINISHED");
            websFSM.SendEvent("FINISHED");
            websFSM.SendEvent("LAND");
            websFSM.SendEvent("FINISHED");
            if (index == 2)
            {
                websFSM.SendEvent("FINISHED");
            }
        }
        private static void BreakTHKChains(int index)
        {
            if (index == 1)
            {
                string fsmName = "Control";
                string goName1 = "hollow_knight_chain_base";
                string goName2 = "hollow_knight_chain_base 2";
                string goName3 = "hollow_knight_chain_base 3";
                string goName4 = "hollow_knight_chain_base 4";
                PlayMakerFSM fsm1 = FindFsmGlobally(goName1, fsmName);
                PlayMakerFSM fsm2 = FindFsmGlobally(goName2, fsmName);
                PlayMakerFSM fsm3 = FindFsmGlobally(goName3, fsmName);
                PlayMakerFSM fsm4 = FindFsmGlobally(goName4, fsmName);
                fsm1.SetState("Break");
                fsm2.SetState("Break");
                fsm3.SetState("Break");
                fsm4.SetState("Break");
            }

            //alternative method of radiance reload that doesn't softlock on game pause, sets shade correctly, and a couple other minor benefits related to timing
            if (index == 2)
            {
                PlayMakerFSM controlFSM = FindFsmGlobally("Boss Control", "Battle Start");

                controlFSM.SetState("Init");
                controlFSM.SendEvent("Revisit");
                controlFSM.SetState("Fight Start");

                string thkName = "Hollow Knight Boss";

                GameObject thk = GameObject.Find(thkName);
                thk.SetActiveChildren(true);

                GameObject dream = GameObject.Find("Dream Enter");

                PlayMakerFSM thkFSM = thk.LocateMyFSM("Control");
                PlayMakerFSM dreamControlFSM = FindFsmGlobally("Dream Enter", "Control");

                thk.SetActiveChildren(false);
                dream.SetActive(true);
                thkFSM.SetState("Long Roar End");
                thkFSM.SendEvent("Hornet Start");

                dreamControlFSM.SetState("Take Control");
            }

        } //Room_Final_Boss
        private static void ObtainDreamNail(int index)
        {
            string goName = "Witch Control";
            string fsmName = "Control";
            PlayMakerFSM fsm = FindFsmGlobally(goName, fsmName);
            fsm.SetState("Pause");
            fsm.SendEvent("FINISHED");
            fsm.SendEvent("DREAM WAKE");
            fsm.SendEvent("FINISHED");
            fsm.SendEvent("FINISHED");
            fsm.SendEvent("ZONE 1");
            fsm.SendEvent("ZONE 2");
            fsm.SendEvent("ZONE 3");
            fsm.SendEvent("FINISHED");
            DebugMod.HC.transform.position = new Vector3(263.1f, 52.406f);
        }
        private static void FastSoulMaster(int index)
        {
            if (index == 1)
            {
                //start phase 1
                DebugMod.HC.transform.position = new Vector3(19.5810f, 29.41113f); //make sure youre at the right spot ig?
                string goName = "Mage Lord"; //soul master gameobject
                string fsmName = "Mage Lord";//soul master fsm
                PlayMakerFSM fsm = FindFsmGlobally(goName, fsmName);
                fsm.SetState("Init");
            }
            else if (index == 2)
            {
                //start phase 1
                DebugMod.HC.transform.position = new Vector3(19.5810f, 29.41113f); //make sure youre at the right spot ig?
                string goName = "Mage Lord"; //soul master gameobject
                string fsmName = "Mage Lord";//soul master fsm
                string quakegoname = "Quake Fake Parent";
                string quakefsmname = "Appear";
                PlayMakerFSM fsm = FindFsmGlobally(goName, fsmName);
                PlayMakerFSM quakeFakeFSM = FindFsmGlobally(quakegoname, quakefsmname);
                fsm.SetState("Init"); //to close gate and avoid save shenanigans
                GameObject.Destroy(GameObject.Find("Mage Lord"));
                quakeFakeFSM.SendEvent("QUAKE FAKE APPEAR");
            }
        }

        #endregion

        //TODO: Add functionality for checking ALL room specifics :(
        internal static string SaveRoomSpecific(string scene)
        {
            scene = scene.ToLower();
            if (ColoSaveState.coloScenes.Contains(scene)) return ColoSaveState.SaveColoScene(scene);
            //insert other room specifics here
            return null;
        }
        internal static void DoRoomSpecific(string scene, string options)//index only used if multiple functionallities in one room, safe to ignore for now.
        {
            // caps in scene names change across versions
            int index = 0;
            scene = scene.ToLower();
            Console.AddLine(scene + " is lowercase");
            if (ColoSaveState.coloScenes.Contains(scene))
            {
                Console.AddLine("Starting Colo Wave Room Specific");
                ColoSaveState.LoadColoScene(scene, options);
                return;
            }
            try 
            {
                index = int.Parse(options); 
            }
            catch (Exception e)
            {
                Console.AddLine("Invalid Room Specific: \n" + e);
            }
            switch (scene)
            {
                case "deepnest_spider_town":
                    EnterSpiderTownTrap(index);
                    break;
                case "room_final_boss_core":
                    BreakTHKChains(index);
                    break;
                case "dream_nailcollection":
                    ObtainDreamNail(index);
                    break;
                case "ruins1_24":
                    FastSoulMaster(index);
                    break;
                default:
                    Console.AddLine("No Room Specific Function Found In: " + scene);
                    break;
            }
            }
        private static PlayMakerFSM FindFsmGlobally(string gameObjectName, string fsmName)
        {
            return GameObject.Find(gameObjectName).LocateMyFSM(fsmName);
        }
    }
}
