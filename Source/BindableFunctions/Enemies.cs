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

        //probably should delete this as it has become obsolete becuase of the new Show Hitboxes in the visuals page
        [BindableMethod(name = "Toggle Enemy Hitboxes (Redundant)", category = "Enemy Panel")]
        public static void ToggleEnemyCollision()
        {
            EnemiesPanel.hitboxes = !EnemiesPanel.hitboxes;

            if (EnemiesPanel.hitboxes)
            {
                Console.AddLine("Enabled hitboxes");
            }
            else
            {
                Console.AddLine("Disabled hitboxes");
            }
        }

        [BindableMethod(name = "Toggle HP Bars", category = "Enemy Panel")]
        public static void ToggleEnemyHPBars()
        {
            EnemiesPanel.hpBars = !EnemiesPanel.hpBars;

            if (EnemiesPanel.hpBars) EnemiesPanel.autoUpdate = true;

            if (EnemiesPanel.hpBars)
            {
                Console.AddLine("Enabled HP bars");
            }
            else
            {
                Console.AddLine("Disabled HP bars");
            }
        }

        [BindableMethod(name = "Toggle Enemy Scan", category = "Enemy Panel")]
        public static void ToggleEnemyAutoScan()
        {
            EnemiesPanel.autoUpdate = !EnemiesPanel.autoUpdate;

            if (EnemiesPanel.autoUpdate)
            {
                Console.AddLine("Enabled auto-scan (May impact performance)");
            }
            else
            {
                Console.AddLine("Disabled auto-scan");
            }
        }

        [BindableMethod(name = "Enemy Scan", category = "Enemy Panel")]
        public static void EnemyScan()
        {
            EnemiesPanel.EnemyUpdate(200f);
            Console.AddLine("Refreshing collider data...");
        }

        [BindableMethod(name = "Self Damage", category = "Enemy Panel")]
        public static void SelfDamage()
        {
            if (PlayerData.instance.health <= 0 || HeroController.instance.cState.dead || !GameManager.instance.IsGameplayScene() || GameManager.instance.IsGamePaused() || HeroController.instance.cState.recoiling || HeroController.instance.cState.invulnerable)
            {
                Console.AddLine("Unacceptable conditions for selfDamage(" + PlayerData.instance.health + "," + DebugMod.HC.cState.dead + "," + DebugMod.GM.IsGameplayScene() + "," + DebugMod.HC.cState.recoiling + "," + DebugMod.GM.IsGamePaused() + "," + DebugMod.HC.cState.invulnerable + ")." + " Pressed too many times at once?");
                return;
            }
            //GameManager.instance.gameObject.AddComponent<SelfDamage>();
            HeroController.instance.TakeDamage(new GameObject(),CollisionSide.left,1,(int)HazardType.NON_HAZARD);

            Console.AddLine("Attempting self damage");
        }
    }
}