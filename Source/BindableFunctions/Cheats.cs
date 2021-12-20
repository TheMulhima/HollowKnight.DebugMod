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
        [BindableMethod(name = "Kill All", category = "Cheats")]
        public static void KillAll()
        {
            int ctr = 0;

            foreach (HealthManager hm in Object.FindObjectsOfType<HealthManager>())
            {
                if (!hm.isDead)
                {
                    hm.Die(null, AttackTypes.Generic, true);
                    ctr++;
                }
            }
            Console.AddLine($"Killing {ctr} HealthManagers in scene!");
        }

        [BindableMethod(name = "Infinite Jump", category = "Cheats")]
        public static void ToggleInfiniteJump()
        {
            PlayerData.instance.infiniteAirJump = !PlayerData.instance.infiniteAirJump;
            Console.AddLine("Infinite Jump set to " + PlayerData.instance.infiniteAirJump.ToString().ToUpper());
        }

        [BindableMethod(name = "Infinite Soul", category = "Cheats")]
        public static void ToggleInfiniteSoul()
        {
            DebugMod.infiniteSoul = !DebugMod.infiniteSoul;
            Console.AddLine("Infinite SOUL set to " + DebugMod.infiniteSoul.ToString().ToUpper());
        }

        [BindableMethod(name = "Infinite HP", category = "Cheats")]
        public static void ToggleInfiniteHP()
        {
            DebugMod.infiniteHP = !DebugMod.infiniteHP;
            Console.AddLine("Infinite HP set to " + DebugMod.infiniteHP.ToString().ToUpper());
        }

        [BindableMethod(name = "Invincibility", category = "Cheats")]
        public static void ToggleInvincibility()
        {
            PlayerData.instance.isInvincible = !PlayerData.instance.isInvincible;
            Console.AddLine("Invincibility set to " + PlayerData.instance.isInvincible.ToString().ToUpper());

            DebugMod.playerInvincible = PlayerData.instance.isInvincible;
        }

        [BindableMethod(name = "Noclip", category = "Cheats")]
        public static void ToggleNoclip()
        {
            DebugMod.noclip = !DebugMod.noclip;

            if (DebugMod.noclip)
            {
                Console.AddLine("Enabled noclip");
                DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
            }
            else
            {
                if (DebugMod.RefKnight.LocateMyFSM("Surface Water").ActiveStateName != "Inactive")
                {
                    DebugMod.RefKnight.LocateMyFSM("Surface Water").SendEvent("JUMP");
                }
                Console.AddLine("Disabled noclip");
            }
        }

        [BindableMethod(name = "Toggle Hero Collider", category = "Cheats")]
        public static void ToggleHeroCollider()
        {
            if (!DebugMod.RefHeroCollider.enabled)
            {
                DebugMod.RefHeroCollider.enabled = true;
                DebugMod.RefHeroBox.enabled = true;
                Console.AddLine("Enabled hero collider" + (DebugMod.noclip ? " and disabled noclip" : ""));
                DebugMod.noclip = false;
            }
            else
            {
                DebugMod.RefHeroCollider.enabled = false;
                DebugMod.RefHeroBox.enabled = false;
                Console.AddLine("Disabled hero collider" + (DebugMod.noclip ? "" : " and enabled noclip"));
                DebugMod.noclip = true;
                DebugMod.noclipPos = DebugMod.RefKnight.transform.position;
            }
        }

        [BindableMethod(name = "Kill Self", category = "Cheats")]
        public static void KillSelf()
        {
            if (DebugMod.GM.isPaused) UIManager.instance.TogglePauseGame();
            HeroController.instance.TakeHealth(9999);

            HeroController.instance.heroDeathPrefab.SetActive(true);
            DebugMod.GM.ReadyForRespawn(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        }

        [BindableMethod(name = "Toggle Bench Storage", category = "Cheats")]
        public static void ToggleBenchStorage()
        {
            PlayerData.instance.atBench = !PlayerData.instance.atBench;
            Console.AddLine($"{(PlayerData.instance.atBench ? "Given" : "Taken away")} bench storage");
        }

        [BindableMethod(name = "Toggle Collision", category = "Cheats")]
        public static void ToggleCollision()
        {
            var rb2d = HeroController.instance.GetComponent<Rigidbody2D>();
            rb2d.isKinematic = !rb2d.isKinematic;
            Console.AddLine($"{(rb2d.isKinematic ? "Enabled" : "Disabled")} collision");
        }

        [BindableMethod(name = "Dreamgate Invulnerability", category = "Cheats")]
        public static void GiveDgateInvuln()
        {
            PlayerData.instance.isInvincible = true;
            Object.FindObjectOfType<HeroBox>().gameObject.SetActive(false);
            HeroController.instance.gameObject.LocateMyFSM("Roar Lock").FsmVariables.FindFsmBool("No Roar").Value =
                true;
            Console.AddLine("Given dreamgate invulnerability");
        }
    }
}