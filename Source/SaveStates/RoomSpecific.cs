using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;

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

        public static void DoRoomSpecific(string scene, int index)//index only used if multiple functionallities in one room, safe to ignore for now.
        {
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
