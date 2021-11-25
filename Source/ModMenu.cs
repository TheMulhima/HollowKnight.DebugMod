using System;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod
{
    public static class ModMenu
    {
        public static MenuBuilder CreateMenuScreen(MenuScreen modListMenu)
        {
            Action<MenuSelectable> CancelAction = selectable => UIManager.instance.UIGoToDynamicMenu(modListMenu); 
            return new MenuBuilder(UIManager.instance.UICanvas.gameObject, "DebugMod Menu")
                .CreateTitle("DebugMod Menu", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new GridNavGraph(1))
                .AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c =>
                    {
                        c.AddMenuButton(
                                "Show/Hide Debug UI",
                                new MenuButtonConfig
                                {
                                    CancelAction = CancelAction,
                                    SubmitAction = _ => BindableFunctions.ToggleAllPanels(),
                                    Description = new DescriptionInfo
                                    {
                                        Text = "Click to show/hide DebugMod UI. Doesnt work in main menu"
                                    },
                                    Label = "Show/Hide Debug UI",
                                    Proceed = false
                                })
                            .AddMenuButton(
                                "Reset Settings Changed by Debug",
                                new MenuButtonConfig
                                {
                                    CancelAction = CancelAction,
                                    SubmitAction = _ => BindableFunctions.Reset(),
                                    Description = new DescriptionInfo
                                    {
                                        Text = "Changes such as nail damage and time scale are reset"
                                    },
                                    Label = "Reset Settings Changed by Debug",
                                    Proceed = false
                                })
                            .AddMenuButton(
                                "Reset KeyBinds to Default",
                                new MenuButtonConfig
                                {
                                    CancelAction = CancelAction,
                                    SubmitAction = _ => DebugMod.ResetKeyBinds(),
                                    Description = new DescriptionInfo
                                    {
                                        Text = "Current keybinds are removed and the default keybinds are loaded",
                                    },
                                    Label = "Reset KeyBinds to Default",
                                    Proceed = false   
                                });
                    })
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )), c => c.AddMenuButton(
                        "BackButton",
                        new MenuButtonConfig
                        {
                            Label = "Back",
                            CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            Style = MenuButtonStyle.VanillaStyle,
                            Proceed = true
                        }));;
        }
    }
}