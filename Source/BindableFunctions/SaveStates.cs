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
         [BindableMethod(name = "Quickslot (save)", category = "Savestates")]
        public static void SaveState()
        {
            DebugMod.saveStateManager.SaveState(SaveStateType.Memory);
        }

        [BindableMethod(name = "Quickslot (load)", category = "Savestates")]
        public static void LoadState()
        {
            DebugMod.saveStateManager.LoadState(SaveStateType.Memory);
        }

        [BindableMethod(name = "Quickslot save to file", category = "Savestates")]
        public static void CurrentSaveStateToFile()
        {
            DebugMod.saveStateManager.SaveState(SaveStateType.File);
        }

        [BindableMethod(name = "Load file to quickslot", category = "Savestates")]
        public static void CurrentSlotToSaveMemory()
        {
            DebugMod.saveStateManager.LoadState(SaveStateType.File);
        }

        [BindableMethod(name = "Save new state to file", category = "Savestates")]
        public static void NewSaveStateToFile()
        {
            DebugMod.saveStateManager.SaveState(SaveStateType.SkipOne);

        }

        [BindableMethod(name = "Load new state from file", category = "Savestates")]
        public static void LoadFromFile()
        {
            DebugMod.saveStateManager.LoadState(SaveStateType.SkipOne);
        }

        [BindableMethod(name = "Next Save Page", category = "Savestates")]
        public static void NextStatePage()
        {
            SaveStateManager.currentStateFolder++;
            if (SaveStateManager.currentStateFolder >= SaveStateManager.savePages)
            {
                SaveStateManager.currentStateFolder = 0;
            } //rollback to 0 if higher than max

            SaveStateManager.path = Path.Combine(
                SaveStateManager.saveStatesBaseDirectory,
                SaveStateManager.currentStateFolder.ToString()); //change path
            DebugMod.saveStateManager.RefreshStateMenu(); // update menu
        }

        [BindableMethod(name = "Prev Save Page", category = "Savestates")]
        public static void PrevStatePage()
        {
            SaveStateManager.currentStateFolder--;
            if (SaveStateManager.currentStateFolder < 0)
            {
                SaveStateManager.currentStateFolder = SaveStateManager.savePages - 1;
            } //rollback to max if we go below page 0

            SaveStateManager.path = Path.Combine(
                SaveStateManager.saveStatesBaseDirectory,
                SaveStateManager.currentStateFolder.ToString()); //change path
            DebugMod.saveStateManager.RefreshStateMenu(); // update menu
        }

        /*
        [BindableMethod(name = "Toggle auto slot", category = "Savestates")]
        public static void ToggleAutoSlot()
        {
            DebugMod.saveStateManager.ToggleAutoSlot();
        }
        
        
        [BindableMethod(name = "Refresh state menu", category = "Savestates")]
        public static void RefreshSaveStates()
        {
            DebugMod.saveStateManager.RefreshStateMenu();
        }
        */
    }
}