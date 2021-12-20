using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMod
{
    public static class BossHandler
    {
        public static bool bossSub;
        private static bool fsmToggle = false;
        public static Dictionary<string, KeyValuePair<bool, string>> bossData;
        public static Dictionary<string, string> ghostData;
        public static bool bossFound;
        public static bool ghostFound;

        public static void LookForBoss(string sceneName)
        {
            bossFound = false;
            ghostFound = false;
            if (bossData != null && bossData.ContainsKey(sceneName))
            {
                Console.AddLine("Found stored Boss in this scene, respawn available");
                bossFound = true;
            }

            if (ghostData != null && ghostData.ContainsKey(sceneName))
            {
                Console.AddLine("Found stored Ghost Boss in this scene, respawn available");
                ghostFound = true;
            }
        }

        public static void PopulateBossLists()
        {
            if (bossData == null)
            {
                bossData = new Dictionary<string, KeyValuePair<bool, string>>(16);
            }

            if (ghostData == null)
            {
                ghostData = new Dictionary<string, string>(7);
            }

            bossData.Clear();
            ghostData.Clear();
            bossData.Add("Ruins2_03", new KeyValuePair<bool, string>(true, "Battle Control"));
            bossData.Add("Crossroads_09", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Crossroads_04", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Fungus1_04", new KeyValuePair<bool, string>(false, "hornet1Defeated"));
            bossData.Add("Crossroads_10", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Fungus3_archive_02", new KeyValuePair<bool, string>(false, "defeatedMegaJelly"));
            bossData.Add("Fungus2_15", new KeyValuePair<bool, string>(false, "defeatedMantisLords"));
            bossData.Add("Waterways_12", new KeyValuePair<bool, string>(false, "flukeMotherDefeated"));
            bossData.Add("Waterways_05", new KeyValuePair<bool, string>(false, "defeatedDungDefender"));
            bossData.Add("Ruins1_24", new KeyValuePair<bool, string>(false, "mageLordDefeated"));
            bossData.Add("Deepnest_32", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Mines_18", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Mines_32", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Fungus3_23", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Ruins2_11", new KeyValuePair<bool, string>(true, "Battle Scene"));
            bossData.Add("Deepnest_East_Hornet", new KeyValuePair<bool, string>(false, "hornetOutskirtsDefeated"));
            bossData.Add("Grimm_Main_Tent", new KeyValuePair<bool, string>(false, "killedGrimm"));
            ghostData.Add("RestingGrounds_02", "xeroDefeated");
            ghostData.Add("Fungus1_35", "noEyesDefeated");
            ghostData.Add("Fungus2_32", "elderHuDefeated");
            ghostData.Add("Deepnest_East_10", "markothDefeated");
            ghostData.Add("Deepnest_40", "galienDefeated");
            ghostData.Add("Fungus3_40", "mumCaterpillarDefeated");
            ghostData.Add("Cliffs_02", "aladarSlugDefeated");
        }

        public static void RespawnBoss()
        {
            if (bossFound)
            {
                if (bossData[DebugMod.GetSceneName()].Key)
                {
                    SceneAdditiveLoadConditional bossLoader =
                        GameObject.Find("BossLoader")?.GetComponent<SceneAdditiveLoadConditional>();

                    if (bossLoader != null)
                    {
                        Console.AddLine(bossLoader.ToString());
                        Console.AddLine(bossLoader.sceneNameToLoad);
                    }

                    IEnumerator ResetBoss(string scene)
                    {
                        if (bossLoader != null && scene != null)
                        {
                            yield return null;
                            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene,
                                LoadSceneMode.Additive);

                            FieldInfo fi = typeof(SceneAdditiveLoadConditional).GetField("sceneLoaded",
                                BindingFlags.Instance | BindingFlags.NonPublic);

                            fi.SetValue(bossLoader, true);
                            yield return null;
                            GameManager.instance.LoadedBoss();
                        }

                        PlayMakerFSM[] components = GameObject.Find(bossData[DebugMod.GetSceneName()].Value)
                            .GetComponents<PlayMakerFSM>();

                        if (components != null)
                        {
                            foreach (PlayMakerFSM playMakerFSM in components)
                            {
                                if (playMakerFSM.FsmVariables.GetFsmBool("Activated") != null)
                                {
                                    playMakerFSM.FsmVariables.GetFsmBool("Activated").Value = false;
                                    Console.AddLine("Boss control for this scene was reset, re-enter scene or warp");
                                }
                            }
                        }
                        else
                        {
                            Console.AddLine("GO does not exist or no FSM on it");
                        }
                    }

                    GameManager.instance.StartCoroutine(
                        ResetBoss(bossLoader != null ? bossLoader.sceneNameToLoad : null));
                }
                else
                {
                    if (bossData[DebugMod.GetSceneName()].Value == "killedGrimm")
                    {
                        PlayerData.instance.SetIntInternal("grimmChildLevel", 2);
                        PlayerData.instance.SetIntInternal("flamesCollected", 3);
                        PlayerData.instance.SetBoolInternal("grimmChildAwoken", false);
                        PlayerData.instance.SetBoolInternal("foughtGrimm", false);
                        PlayerData.instance.SetBoolInternal("killedGrimm", false);
                    }
                    else
                    {
                        PlayerData.instance.GetType().GetField(bossData[DebugMod.GetSceneName()].Value)
                            .SetValue(PlayerData.instance, false);
                    }

                    Console.AddLine("Boss control for this scene was reset, re-enter scene or warp");
                }
            }
            else
            {
                Console.AddLine("No boss in this scene to respawn");
            }
        }

        public static void RespawnGhost()
        {
            if (ghostFound)
            {
                PlayerData.instance.GetType().GetField(ghostData[DebugMod.GetSceneName()])
                    .SetValue(PlayerData.instance, 0);
                Console.AddLine("Ghost Boss for this scene was reset, re-enter scene or warp");
            }
            else
            {
                Console.AddLine("No ghost in this scene to respawn");
            }
        }

        public static void UumuuExtra()
        {
            if (!fsmToggle)
            {
                SetUumuuExtra(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SetUumuuExtra;
                Console.AddLine("Uumuu forced extra attack ON");
                fsmToggle = true;
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SetUumuuExtra;
                fsmToggle = false;
                Console.AddLine("Uumuu forced extra attack OFF");
            }
        }

        private static void SetUumuuExtra(Scene sceneFrom, Scene sceneTo) => SetUumuuExtra(sceneTo.name);
        private static void SetUumuuExtra(string NextSceneName)
        {
            if (NextSceneName == "Fungus3_archive_02")
            {
                try
                {
                    if (DebugMod.GM == null)
                    {
                        throw new Exception("StartUumuuCoro(  ) - gamemanager is null");
                    }

                    DebugMod.GM.StartCoroutine(UumuuExtraCoro(NextSceneName));
                }
                catch (Exception e)
                {
                    DebugMod.instance.Log(e.Message);
                }
            }
        }

        private static IEnumerator UumuuExtraCoro(string activeScene)
        {
            Console.AddLine("Uumuu set extra attack coro launched");
            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != activeScene)
            {
                yield return null;
            }

            // Find Uumuu, their FSM, the specific State, and the Action that dictates the possibility of extra attacks
            GameObject uumuu = GameObject.Find("Mega Jellyfish");
            PlayMakerFSM fsm = uumuu.LocateMyFSM("Mega Jellyfish");
            FsmState fsmState = fsm.FsmStates.First(t => t.Name == "Idle");
            WaitRandom waitRandom = (WaitRandom) fsmState.Actions.OfType<WaitRandom>().First();
            waitRandom.timeMax.Value = 1.6f;
            yield break;
        }
    }
}
