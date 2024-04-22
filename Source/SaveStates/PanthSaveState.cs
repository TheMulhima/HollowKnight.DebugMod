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
using TeamCherry;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using On.HutongGames.PlayMaker.Actions;
using static DebugMod.SaveState;

namespace DebugMod
{
    //Stored in separate class due to the amount of unique functionality
    internal static class PanthSaveState
    {

        public static string[] panthSequences =
        {
            "Boss Sequence Tier 1 (BossSequence)",
            "Boss Sequence Tier 2 (BossSequence)",
            "Boss Sequence Tier 3 (BossSequence)",
            "Boss Sequence Tier 4 (BossSequence)",
            "Boss Sequence Tier 5 (BossSequence)",
        };     
        public static (string SequenceName, int BossIndex) SavePanthScene(string scene)
        {
            int BossIndex = BossSequenceController.BossIndex;
            BossSequence sequence = ReflectionHelper.GetField<BossSequence>(typeof(BossSequenceController), "currentSequence");
            return (sequence.ToString(), BossIndex);
        }

        public static void LoadPanthScene(string SequenceName, int BossIndex)
        {
            GameObject doorPrefab;
            BossSequenceController.Reset();
            switch (SequenceName)
            {
                case "Boss Sequence Tier 1 (BossSequence)":
                    doorPrefab = DebugMod.Panth1Prefab;
                    break;
                case "Boss Sequence Tier 2 (BossSequence)":
                    doorPrefab = DebugMod.Panth2Prefab;
                    break;
                case "Boss Sequence Tier 3 (BossSequence)":
                    doorPrefab = DebugMod.Panth3Prefab;
                    break;
                case "Boss Sequence Tier 4 (BossSequence)":
                    doorPrefab = DebugMod.Panth4Prefab;
                    break;
                case "Boss Sequence Tier 5 (BossSequence)":
                    doorPrefab = DebugMod.Panth5Prefab;
                    break;
                default:
                    return;
            }
            BossSequence sequence = GameObject.Instantiate(doorPrefab).GetComponent<BossSequenceDoor>().bossSequence;
            BossSequenceController.SetupNewSequence(sequence, BossSequenceController.ChallengeBindings.None, HeroController.instance.playerData.ToString());
            ReflectionHelper.SetField<int>(typeof(BossSequenceController), "bossIndex", BossIndex);
            ReflectionHelper.CallMethod(typeof(BossSequenceController), "SetupBossScene");
            isPanthState = true;
        }
        //TODO: Set this up to be able to avoid double loads
        private static void SetupCurrentBossScene()
        {

        }
        public static IEnumerator SetupPanthTransition()
        {
            if (BossSequenceController.BossIndex == 0)
            {
                GameObject dreamEntry = GameObject.Find("Dream Entry");
                PlayMakerFSM entryFSM = dreamEntry.LocateMyFSM("Control");
                yield return new WaitUntil(() => entryFSM.ActiveStateName == "Start Fade");
                GameManager.instance.StartCoroutine(SetupLoadDelay(dreamEntry, entryFSM));
            }
            isPanthState = false;
        }

        public static IEnumerator SetupLoadDelay(GameObject dreamEntry, PlayMakerFSM entryFSM)
        {
            dreamEntry.SetActive(false);
            yield return new WaitForSeconds(DebugMod.settings.PanthLoadDelay);
            dreamEntry.SetActive(true);
            entryFSM.SetState("Start Fade");
        }
    }
}
