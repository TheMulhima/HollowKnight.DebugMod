using System;
using System.Collections.Generic;
using DebugMod.Canvas;
using UnityEngine;
using InControl;
using JetBrains.Annotations;

namespace DebugMod.InfoPanels
{
    public class CustomInfoPanel : TogglableInfoPanel
    {
        public bool ShowSprite;
        public CustomInfoPanel(string Name, bool ShowSprite) : base(Name)
        {
            this.ShowSprite = ShowSprite;
        }

        protected List<(float xLabel, float xInfo, float y, string label, Func<string> textFunc)> PanelBuildInfo = new();
        private Dictionary<string, Func<string>> UpdateActions;
        public override void BuildPanel(GameObject canvas)
        {
            if (ShowSprite)
            {
                panel = new CanvasPanel(
                    canvas,
                    GUIController.Instance.images["StatusPanelBG"],
                    new Vector2(0f, 223f),
                    Vector2.zero,
                    new Rect(
                        0f,
                        0f,
                        GUIController.Instance.images["StatusPanelBG"].width,
                        GUIController.Instance.images["StatusPanelBG"].height));
            }
            else
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadRawTextureData(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                tex.Apply();

                // Puke
                panel = new CanvasPanel(
                    canvas,
                    tex,
                    new Vector2(130f, 230f),
                    Vector2.zero,
                    new Rect(
                        0f,
                        0f,
                        1f,
                        1f));
            }


            UpdateActions = new();
            int counter = 0;

            foreach ((float xLabel, float xInfo, float y, string label, Func<string> textFunc) in PanelBuildInfo)
            {
                panel.AddText($"Label-{counter++}", label, new Vector2(xLabel, y), Vector2.zero, GUIController.Instance.arial, 15);
                panel.AddText($"Info-{counter}", "", new Vector2(xInfo, y + 4f), Vector2.zero, GUIController.Instance.trajanNormal);
                UpdateActions.Add($"Info-{counter}", textFunc);
            }

            panel.FixRenderOrder();
        }
        public override void UpdatePanel()
        {
            if (panel == null) return;

            foreach (var kvp in UpdateActions)
            {
                panel.GetText(kvp.Key).UpdateText(kvp.Value.Invoke());
            }
        }

        public void AddInfo(float xLabel, float xInfo, float y, string label, Func<string> textFunc)
            => PanelBuildInfo.Add((xLabel, xInfo, y, label, textFunc));

        internal static CustomInfoPanel GetMainInfoPanel()
        {
            CustomInfoPanel MainInfoPanel = new CustomInfoPanel(InfoPanel.MainInfoPanelName, true);

            float y = 0f;

            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Hero State", () => HeroController.instance.hero_state.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Velocity", () => HeroController.instance.current_velocity.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Naildmg", () => DebugMod.RefKnightSlash.FsmVariables.GetFsmInt("damageDealt").Value + " (Flat " + PlayerData.instance.nailDamage + ", x" + DebugMod.RefKnightSlash.FsmVariables.GetFsmFloat("Multiplier").Value + ")");
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "HP", () => PlayerData.instance.health + " / " + PlayerData.instance.maxHealth);
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "MP", () => (PlayerData.instance.MPCharge + PlayerData.instance.MPReserve).ToString());
            y += 58f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Completion", () => PlayerData.instance.completionPercentage.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Grubs", () => PlayerData.instance.grubsCollected + " / 46");
            y += 58f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "isInvuln", () => GetStringForBool(HeroController.instance.cState.invulnerable));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Invincible", () => GetStringForBool(PlayerData.instance.isInvincible));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "invinciTest", () => GetStringForBool(PlayerData.instance.invinciTest));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Damage State", () => HeroController.instance.damageMode.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Dead State", () => GetStringForBool(HeroController.instance.cState.dead));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Hazard Death", () => HeroController.instance.cState.hazardDeath.ToString());
            y += 58f;
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Scene Name", () => DebugMod.GetSceneName());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Transition", () => GetStringForBool(HeroController.instance.cState.transitioning));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Trans State", () => GetTransState());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Is Gameplay", () => GetStringForBool(DebugMod.GM.IsGameplayScene()));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Game State", () => GameManager.instance.gameState.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "UI State", () => UIManager.instance.uiState.ToString());
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Hero Paused", () => GetStringForBool(HeroController.instance.cState.isPaused));
            MainInfoPanel.AddInfo(10f, 150f, y += 20, "Camera Mode", () => DebugMod.RefCamera.mode.ToString());

            y = 10f;

            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Accept Input", () => GetStringForBool(HeroController.instance.acceptingInput));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Relinquished", () => GetStringForBool(HeroController.instance.controlReqlinquished));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "atBench", () => GetStringForBool(PlayerData.instance.atBench));
            y += 30f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Dashing", () => GetStringForBool(HeroController.instance.cState.dashing));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Jumping", () => GetStringForBool((HeroController.instance.cState.jumping || HeroController.instance.cState.doubleJumping)));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Superdashing", () => GetStringForBool(HeroController.instance.cState.superDashing));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Falling", () => GetStringForBool(HeroController.instance.cState.falling));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Hardland", () => GetStringForBool(HeroController.instance.cState.willHardLand));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Swimming", () => GetStringForBool(HeroController.instance.cState.swimming));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Recoiling", () => GetStringForBool(HeroController.instance.cState.recoiling));
            y += 30f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall lock", () => GetStringForBool(HeroController.instance.wallLocked));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall jumping", () => GetStringForBool(HeroController.instance.cState.wallJumping));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall touching", () => GetStringForBool(HeroController.instance.cState.touchingWall));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall sliding", () => GetStringForBool(HeroController.instance.cState.wallSliding));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall left", () => GetStringForBool(HeroController.instance.touchingWallL));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Wall right", () => GetStringForBool(HeroController.instance.touchingWallR));
            y += 30f;
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "Attacking", () => GetStringForBool(HeroController.instance.cState.attacking));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "canCast", () => GetStringForBool(HeroController.instance.CanCast()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "canSuperdash", () => GetStringForBool(HeroController.instance.CanSuperDash()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "canQuickmap", () => GetStringForBool(HeroController.instance.CanQuickMap()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "canInventory", () => GetStringForBool(HeroController.instance.CanOpenInventory()));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "canWarp", () => GetStringForBool(DebugMod.RefDreamNail.FsmVariables.GetFsmBool("Dream Warp Allowed").Value));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "canDGate", () => GetStringForBool(DebugMod.RefDreamNail.FsmVariables.GetFsmBool("Can Dream Gate").Value));
            MainInfoPanel.AddInfo(300f, 440f, y += 20, "gateAllow", () => GetStringForBool(DebugMod.RefDreamNail.FsmVariables.GetFsmBool("Dream Gate Allowed").Value));

            return MainInfoPanel;
        }

        internal static CustomInfoPanel GetMinimalInfoPanel()
        {
            CustomInfoPanel MinimalInfoPanel = new CustomInfoPanel(InfoPanel.MinimalInfoPanelName, false);

            MinimalInfoPanel.AddInfo(10, 40, 10, "Vel", () => HeroController.instance.current_velocity.ToString());
            MinimalInfoPanel.AddInfo(110, 140, 10, "Pos", () => GetHeroPos());
            
            MinimalInfoPanel.AddInfo(10, 40, 30, "MP", () => (PlayerData.instance.MPCharge + PlayerData.instance.MPReserve).ToString());
            MinimalInfoPanel.AddInfo(100, 190, 30, "CanCdash", () => GetStringForBool(HeroController.instance.CanSuperDash()));
            
            MinimalInfoPanel.AddInfo(10, 100, 50, "NailDmg", () => DebugMod.RefKnightSlash.FsmVariables.GetFsmInt("damageDealt").Value + " (Flat " + PlayerData.instance.nailDamage + ", x" + DebugMod.RefKnightSlash.FsmVariables.GetFsmFloat("Multiplier").Value + ")");
            
            MinimalInfoPanel.AddInfo(10, 95, 70, "Completion", () => PlayerData.instance.completionPercentage.ToString() + "%");
            MinimalInfoPanel.AddInfo(140, 195, 70, "Grubs", () => PlayerData.instance.grubsCollected + " / 46");
            
            MinimalInfoPanel.AddInfo(10, 140, 90, "Scene Name", () => DebugMod.GetSceneName());
            MinimalInfoPanel.AddInfo(10, 140, 110, "Current SaveState", () => SaveStateManager.quickState.IsSet() ? SaveStateManager.quickState.GetSaveStateID() : "No savestate");
            MinimalInfoPanel.AddInfo(110, 200, 130, "Current slot", GetCurrentSlotString);
            MinimalInfoPanel.AddInfo(10, 80, 130, "Hardfall", () => GetStringForBool(HeroController.instance.cState.willHardLand));

            return MinimalInfoPanel;
        }

        private static string GetCurrentSlotString()
        {
            string slotSet = SaveStateManager.GetCurrentSlot().ToString();
            if (slotSet == "-1")
            {
                slotSet = "unset";
            }
            return slotSet;
        }
    }
}
