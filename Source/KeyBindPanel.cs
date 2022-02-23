using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DebugMod.Canvas;
using Modding;
using UnityEngine;

namespace DebugMod
{
    public static class KeyBindPanel
    {
        private class CategoryInfo
        {
            public string Name;
            public List<string> Functions = new();
            
            public CategoryInfo(string Name)
            {
                this.Name = Name;
            }

            public void Add(string method) => Functions.Add(method);

            public int NumPages => (Functions.Count + ItemsPerPage - 1) / (ItemsPerPage); // Ceiling division

            public IEnumerable<string> ItemsOnPage(int index)
            {
                for (int i = ItemsPerPage * index; i < ItemsPerPage * (index + 1) && i < Functions.Count; i++)
                {
                    yield return Functions[i];
                }
            }


            public static List<string> Categories = new()
            {
                "GamePlay Altering".Localize(),
                "Savestates".Localize(),
                "Misc".Localize(),
                "Visual".Localize(),
                "Mod UI".Localize(),
                "Enemy Panel".Localize(),
                "Cheats".Localize(),
                "Charms".Localize(),
                "Skills".Localize(),
                "Spells".Localize(),
                "Bosses".Localize(),
                "Items".Localize(),
                "Mask & Vessels".Localize(),
                "Consumables".Localize(),
                "Dreamgate".Localize(),
            };
            public static Dictionary<string, CategoryInfo> CategoryInfos = new();
            public static int TotalPages => CategoryInfos.Select(x => x.Value.NumPages).Sum();

            public static List<(string categoryName, int indexInCategory)> pageData = new();
            public static void GeneratePageData()
            {
                pageData.Clear();
                foreach (string categoryName in Categories)
                {
                    for (int i = 0; i < CategoryInfos[categoryName].NumPages; i++)
                    {
                        pageData.Add((categoryName, i));
                    }
                }
            }
            
            public static int currentPage = 0;

            public static void AddFunction(string category, string name)
            {
                if (!CategoryInfos.TryGetValue(category, out CategoryInfo info))
                {
                    info = new CategoryInfo(category);

                    if (!Categories.Contains(category)) Categories.Add(category);
                    CategoryInfos.Add(category, info);
                }

                info.Add(name);
            }

            public static List<string> FunctionsOnCurrentPage()
            {
                (string categoryName, int indexInCategory) = pageData[currentPage];

                return CategoryInfos[categoryName].ItemsOnPage(indexInCategory).ToList();
            }
            public static string CurrentCategory => pageData[currentPage].categoryName;
        }

        public const int ItemsPerPage = 11;

        private static CanvasPanel panel;
        
        public static KeyCode keyWarning = KeyCode.None;

        // TODO: Refactor to allow rotating images
        public static void BuildMenu(GameObject canvas)
        {
            panel = new CanvasPanel(canvas, GUIController.Instance.images["HelpBG"], new Vector2(1123, 456),
                Vector2.zero,
                new Rect(0, 0, GUIController.Instance.images["HelpBG"].width,
                    GUIController.Instance.images["HelpBG"].height));
            panel.AddText("Label", "Binds".Localize(), new Vector2(130f, -25f), Vector2.zero, GUIController.Instance.trajanBold,
                30);

            panel.AddText("Category", "", new Vector2(25f, 25f), Vector2.zero, GUIController.Instance.trajanNormal, 20);
            panel.AddText("Help", "", new Vector2(25f, 50f), Vector2.zero, GUIController.Instance.arial, 15);
            panel.AddButton("Page", GUIController.Instance.images["ButtonRect"], new Vector2(125, 250), Vector2.zero,
                NextClicked,
                new Rect(0, 0, GUIController.Instance.images["ButtonRect"].width,
                    GUIController.Instance.images["ButtonRect"].height), GUIController.Instance.trajanBold, "# / #");


            panel.AddButton(
                "NextPage",
                GUIController.Instance.images["ScrollBarArrowRight"],
                new Vector2(223, 254),
                Vector2.zero,
                NextClicked,
                new Rect(
                    0,
                    0,
                    GUIController.Instance.images["ScrollBarArrowRight"].width,
                    GUIController.Instance.images["ScrollBarArrowRight"].height)
            );
            panel.AddButton(
                "PrevPage",
                GUIController.Instance.images["ScrollBarArrowLeft"],
                new Vector2(95, 254),
                Vector2.zero,
                NextClicked,
                new Rect(
                    0,
                    0,
                    GUIController.Instance.images["ScrollBarArrowLeft"].width,
                    GUIController.Instance.images["ScrollBarArrowLeft"].height)
            );

            for (int i = 0; i < ItemsPerPage; i++)
            {
                panel.AddButton(i.ToString(), GUIController.Instance.images["Scrollbar_point"],
                    new Vector2(290f, 45f + 17.5f * i), Vector2.zero, ChangeBind,
                    new Rect(0, 0, GUIController.Instance.images["Scrollbar_point"].width,
                        GUIController.Instance.images["Scrollbar_point"].height));
                panel.AddButton($"run{i}", GUIController.Instance.images["ButtonRun"],
                    new Vector2(308f, 51f + 17.5f * i), new Vector2(12f, 12f), RunBind,
                    new Rect(0, 0, GUIController.Instance.images["ButtonRun"].width,
                        GUIController.Instance.images["ButtonRun"].height));
            }
        
            //Build pages based on categories
            foreach (var bindable in DebugMod.bindMethods)
            {
                string name = bindable.Key;
                string cat = bindable.Value.category;

                CategoryInfo.AddFunction(cat, name);
            }
            foreach (var bindable in DebugMod.AdditionalBindMethods)
            {
                string name = bindable.Key;
                string cat = bindable.Value.category;

                CategoryInfo.AddFunction(cat, name);
            }
            CategoryInfo.GeneratePageData();

            panel.GetText("Category").UpdateText(CategoryInfo.CurrentCategory);
            panel.GetButton("Page").UpdateText((CategoryInfo.currentPage + 1) + " / " + CategoryInfo.TotalPages);
            UpdateHelpText();
        }
        
        private static void RunBind(string buttonName) {
            int bindIndex = Convert.ToInt32(buttonName.Substring(3)); // strip leading "run"
            string bindName = CategoryInfo.FunctionsOnCurrentPage()[bindIndex];

            if (!DebugMod.bindMethods.TryGetValue(bindName, out var pair) && !DebugMod.AdditionalBindMethods.TryGetValue(bindName, out pair))
            {
                DebugMod.instance.LogError("Error running bind: not found");
            }
            pair.method.Invoke();
        }

        public static void UpdateHelpText()
        {
            if (CategoryInfo.currentPage < 0 || CategoryInfo.currentPage >= CategoryInfo.TotalPages) return;

            string cat = CategoryInfo.CurrentCategory;
            List<string> helpPage = CategoryInfo.FunctionsOnCurrentPage();

            string updatedText = "";

            foreach (string bindStr in helpPage)
            {
                updatedText += bindStr + " - ";

                if (DebugMod.settings.binds.ContainsKey(bindStr))
                {
                    KeyCode code = ((KeyCode)DebugMod.settings.binds[bindStr]);

                    if (code != KeyCode.None)
                    {
                        updatedText += ((KeyCode)DebugMod.settings.binds[bindStr]).ToString();
                    }
                    else
                    {
                        updatedText += "WAITING";
                    }
                }
                else
                {
                    updatedText += "UNBOUND";
                }

                updatedText += "\n";
            }

            panel.GetText("Help").UpdateText(updatedText);
        }

        private static void NextClicked(string buttonName)
        {
            if (buttonName.StartsWith("Prev"))
            {
                CategoryInfo.currentPage--;
                if (CategoryInfo.currentPage < 0) CategoryInfo.currentPage = CategoryInfo.TotalPages - 1;
            }
            else
            {
                CategoryInfo.currentPage++;
                if (CategoryInfo.currentPage >= CategoryInfo.TotalPages) CategoryInfo.currentPage = 0;
            }

            panel.GetText("Category").UpdateText(CategoryInfo.CurrentCategory);
            panel.GetButton("Page").UpdateText((CategoryInfo.currentPage + 1) + " / " + CategoryInfo.TotalPages);
            UpdateHelpText();
        }

        private static void ChangeBind(string buttonName)
        {
            int num = Convert.ToInt32(buttonName);

            if (num < 0 || num >= CategoryInfo.FunctionsOnCurrentPage().Count)
            {
                DebugMod.instance.LogWarn("Invalid bind change button clicked. Should not be possible");
                return;
            }

            string bindName = CategoryInfo.FunctionsOnCurrentPage()[num];

            if (DebugMod.settings.binds.ContainsKey(bindName))
            {
                DebugMod.settings.binds[bindName] = (int)KeyCode.None;
            }
            else
            {
                DebugMod.settings.binds.Add(bindName, (int)KeyCode.None);
            }

            UpdateHelpText();
        }

        public static void Update()
        {
            if (panel == null)
            {
                return;
            }

            if (GUIController.ForceHideUI())
            {
                if (panel.active)
                {
                    panel.SetActive(false, true);
                }

                return;
            }

            if (DebugMod.settings.HelpPanelVisible && !panel.active)
            {
                panel.SetActive(true, false);
            }
            else if (!DebugMod.settings.HelpPanelVisible && panel.active)
            {
                panel.SetActive(false, true);
            }

            if (panel.active && CategoryInfo.currentPage >= 0 && CategoryInfo.currentPage < CategoryInfo.TotalPages)
            {
                for (int i = 0; i < ItemsPerPage; i++)
                {
                    panel.GetButton(i.ToString()).SetActive(CategoryInfo.FunctionsOnCurrentPage().Count > i);
                    panel.GetButton($"run{i}").SetActive(CategoryInfo.FunctionsOnCurrentPage().Count > i);
                }
            }
        }
    }
}
