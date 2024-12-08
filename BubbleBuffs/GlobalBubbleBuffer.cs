using BubbleBuffs.Config;
using BubbleBuffs.Utilities;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Dungeon;
using Kingmaker.Dungeon.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.IngameMenu;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleBuffs {

    class GlobalBubbleBuffer {
        public BubbleBuffSpellbookController SpellbookController;
        private ButtonSprites applyBuffsSprites;
        private ButtonSprites applyBuffsShortSprites;
        private ButtonSprites showMapSprites;
        private ButtonSprites applyBuffsImportantSprites;
        private GameObject buttonsContainer;
        public GameObject bubbleHud;
        public GameObject hudLayout;

        public static Sprite[] UnitFrameSprites = new Sprite[2];

        public List<OwlcatButton> Buttons = new();

        public static void TryAddFeature(UnitEntityData u, string feature) {
            var bp = Resources.GetBlueprint<BlueprintFeature>(feature);
            Main.Log("trying to add feature: " + bp.name);
            if (!u.Progression.Features.HasFact(bp)) {
                Main.Log("ADDING");
                u.Progression.Features.AddFeature(bp);
            }
        }

        internal void TryInstallUI() {

            //var u = Game.Instance.Player.ActiveCompanions.First(c => c.CharacterName == "Ember");
            //Main.Log("Got character: " + u.CharacterName);
            //TryAddFeature(u, "2f206e6d292bdfb4d981e99dcf08153f");
            //TryAddFeature(u, "13f9269b3b48ae94c896f0371ce5e23c");

            try {
                //var symbol = Resources.GetBlueprint<BlueprintItemEquipmentUsable>("18f7924f803793a4a9f60495fd88a73b");
                //Game.Instance.Player.MainCharacter.Value.Inventory.Add(symbol);
                
                Main.Verbose("Installing ui");
                Main.Verbose($"spellscreennull: {UIHelpers.SpellbookScreen == null}");
                var spellScreen = UIHelpers.SpellbookScreen?.gameObject;
                if (spellScreen != null) {
                    Main.Verbose("got spell screen");

                    UnitFrameSprites[0] = AssetLoader.LoadInternal("icons", "UI_HudCharacterFrameBorder_Default.png", new Vector2Int(31, 80));
                    UnitFrameSprites[1] = AssetLoader.LoadInternal("icons", "UI_HudCharacterFrameBorder_Hover.png", new Vector2Int(31, 80));

#if DEBUG
                    RemoveOldController(spellScreen);
#endif

                    if (spellScreen.transform.root.GetComponent<BubbleBuffGlobalController>() == null) {
                        Main.Verbose("Creating new global controller");
                        spellScreen.transform.root.gameObject.AddComponent<BubbleBuffGlobalController>();
                    }

                    if (spellScreen.GetComponent<BubbleBuffSpellbookController>() == null) {
                        Main.Verbose("Creating new controller");
                        SpellbookController = spellScreen.AddComponent<BubbleBuffSpellbookController>();
                        SpellbookController.CreateBuffstate();
                    }

                    Main.Verbose("loading sprites");
                    if (applyBuffsSprites == null)
                        applyBuffsSprites = ButtonSprites.Load("apply_buffs", new Vector2Int(95, 95));
                    if (applyBuffsShortSprites == null)
                        applyBuffsShortSprites = ButtonSprites.Load("apply_buffs_short", new Vector2Int(95, 95));
                    if (applyBuffsImportantSprites == null)
                        applyBuffsImportantSprites = ButtonSprites.Load("apply_buffs_important", new Vector2Int(95, 95));
                    if (showMapSprites == null)
                        showMapSprites = ButtonSprites.Load("show_map", new Vector2Int(95, 95));

                    var staticRoot = Game.Instance.UI.Canvas.transform;
                    Main.Verbose("got static root");
                    hudLayout = staticRoot.Find("NestedCanvas1/").gameObject;
                    Main.Verbose("got hud layout");

                    Main.Verbose("Removing old bubble root");
                    var oldBubble = hudLayout.transform.parent.Find("BUBBLEMODS_ROOT");
                    if (oldBubble != null) {
                        GameObject.Destroy(oldBubble.gameObject);
                    }

                    bubbleHud = GameObject.Instantiate(hudLayout, hudLayout.transform.parent);
                    Main.Verbose("instantiated root");
                    bubbleHud.name = "BUBBLEMODS_ROOT";
                    var rect = bubbleHud.transform as RectTransform;
                    rect.anchoredPosition = new Vector2(0, 96);
                    rect.SetSiblingIndex(hudLayout.transform.GetSiblingIndex() + 1);
                    Main.Verbose("set sibling index");

                    bubbleHud.DestroyComponents<UISectionHUDController>();

                    Main.Verbose("destroyed components");

                    //GameObject.Destroy(rect.Find("CombatLog_New").gameObject);
                    //Main.Verbose("destroyed combatlog_new");

                    //GameObject.Destroy(rect.Find("Console_InitiativeTrackerHorizontalPC").gameObject);
                    //Main.Verbose("destroyed horizontaltrack");

                    GameObject.Destroy(rect.Find("IngameMenuView/CompassPart").gameObject);
                    Main.Verbose("destroyed compasspart");

                    List<GameObject> toDestroy = new();
                    for (int rectChildIndex = 0; rectChildIndex < rect.childCount; rectChildIndex++) {
                        var rectChild = rect.GetChild(rectChildIndex);
                        if (rect.GetChild(rectChildIndex).name != "IngameMenuView")
                            toDestroy.Add(rectChild.gameObject);
                    }

                    foreach (var obj in toDestroy) {
                        var name = obj.name;
                        GameObject.Destroy(obj);
                    }

                    bubbleHud.ChildObject("IngameMenuView").DestroyComponents<IngameMenuPCView>();

                    Main.Verbose("destroyed old stuff");

                    var buttonPanelRect = rect.Find("IngameMenuView/ButtonsPart");
                    Main.Verbose("got button panel");
                    GameObject.Destroy(buttonPanelRect.Find("TBMMultiButton").gameObject);
                    GameObject.Destroy(buttonPanelRect.Find("InventoryButton").gameObject);
                    GameObject.Destroy(buttonPanelRect.Find("Background").gameObject);

                    Main.Verbose("destroyed more old stuff");

                    buttonsContainer = buttonPanelRect.Find("Container").gameObject;
                    var buttonsRect = buttonsContainer.transform as RectTransform;
                    buttonsRect.anchoredPosition = Vector2.zero;
                    buttonsRect.sizeDelta = new Vector2(47.7f * 8, buttonsRect.sizeDelta.y);
                    Main.Verbose("set buttons rect");

                    buttonsContainer.GetComponent<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.LowerLeft;

                    var prefab = buttonsContainer.transform.GetChild(0).gameObject;
                    prefab.SetActive(false);

                    int toRemove = buttonsContainer.transform.childCount;

                    //Loop from 1 and destroy child[1] since we want to keep child[0] as our prefab, which is super hacky but.
                    for (int i = 1; i < toRemove; i++) {
                        GameObject.DestroyImmediate(buttonsContainer.transform.GetChild(1).gameObject);
                    }

                    void AddButton(string text, string tooltip, ButtonSprites sprites, Action act) {
                        var applyBuffsButton = GameObject.Instantiate(prefab, buttonsContainer.transform);
                        applyBuffsButton.SetActive(true);
                        OwlcatButton button = applyBuffsButton.GetComponentInChildren<OwlcatButton>();
                        button.m_CommonLayer[0].SpriteState = new SpriteState {
                            pressedSprite = sprites.down,
                            highlightedSprite = sprites.hover,
                        };
                        button.OnLeftClick.AddListener(() => {
                            act();
                        });
                        button.SetTooltip(new TooltipTemplateSimple(text, tooltip), new TooltipConfig {
                            InfoCallPCMethod = InfoCallPCMethod.None
                        });

                        Buttons.Add(button);

                        applyBuffsButton.GetComponentInChildren<Image>().sprite = sprites.normal;

                    }


                    AddButton("group.normal.tooltip.header".i8(), "group.normal.tooltip.desc".i8(), applyBuffsSprites, () => GlobalBubbleBuffer.Execute(BuffGroup.Long));
                    AddButton("group.important.tooltip.header".i8(), "group.important.tooltip.desc".i8(), applyBuffsImportantSprites, () => GlobalBubbleBuffer.Execute(BuffGroup.Important));
                    AddButton("group.short.tooltip.header".i8(), "group.short.tooltip.desc".i8(), applyBuffsShortSprites, () => GlobalBubbleBuffer.Execute(BuffGroup.Short));
                    if (DungeonController.IsDungeonCampaign) {
                        DungeonShowMap showMap = new();
                        AddButton("showmap.tooltip.header".i8(), "showmap.tooltip.desc".i8(), showMapSprites, () => showMap.RunAction());
                    }

                    Main.Verbose("remove old bubble?");
#if debug
                RemoveOldController<SyncBubbleHud>(hudLayout.ChildObject("IngameMenuView"));
#endif
                    if (hudLayout.ChildObject("IngameMenuView").GetComponent<SyncBubbleHud>() == null) {
                        hudLayout.ChildObject("IngameMenuView").AddComponent<SyncBubbleHud>();
                        Main.Verbose("installed hud sync");
                    }
                }
                else {
                    // We're either on console, or a game update broke something
                    // Proceed with minimal setup so we can still buff from the Unity Mod Manager settings menu
                    if (SpellbookController == null) {
                        Main.Verbose("Creating new controller");
                        SpellbookController = new BubbleBuffSpellbookController();
                        SpellbookController.CreateBuffstate();
                    }
                    if (BubbleBuffGlobalController.Instance == null) {
                        BubbleBuffGlobalController.Init();
                    }
                }

                Main.Verbose("Finished early ui setup");
            } catch (Exception ex) {
                Main.Error(ex, "installing");
            }
        }

#if DEBUG
        private static void RemoveOldController<T>(GameObject on) {
            List<Component> toDelete = new();

            foreach (var component in on.GetComponents<Component>()) {
                Main.Verbose($"checking: {component.name}", "remove-old");
                if (component.GetType().FullName == typeof(T).FullName && component.GetType() != typeof(T)) {
                    var method = component.GetType().GetMethod("Destroy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    method.Invoke(component, new object[] { });
                    toDelete.Add(component);
                }
                Main.Verbose($"checked: {component.name}", "remove-old");
            }

            int count = toDelete.Count;
            for (int i = 0; i < count; i++) {
                GameObject.Destroy(toDelete[0]);
            }

        }

        private static void RemoveOldController(GameObject spellScreen) {
            RemoveOldController<BubbleBuffSpellbookController>(spellScreen);
            RemoveOldController<BubbleBuffGlobalController>(spellScreen.transform.root.gameObject);
        }
#endif

        internal void SetButtonState(bool v) {
            buttonsContainer?.SetActive(v);
        }

        public static List<UnitEntityData> Group => Game.Instance.SelectionCharacter.ActualGroup;

        public static GlobalBubbleBuffer Instance;
        private static ServiceWindowWatcher UiEventSubscriber;
        private static SpellbookWatcher SpellMemorizeHandler;
        private static HideBubbleButtonsWatcher ButtonHiderHandler;

        public static void Install() {

            Instance = new();
            UiEventSubscriber = new();
            SpellMemorizeHandler = new();
            ButtonHiderHandler = new();
            EventBus.Subscribe(Instance);
            EventBus.Subscribe(UiEventSubscriber);
            EventBus.Subscribe(SpellMemorizeHandler);
            EventBus.Subscribe(ButtonHiderHandler);

        }

        public static void Execute(BuffGroup group) {
            Instance.SpellbookController.Execute(group);
        }


        public static void Uninstall() {
            EventBus.Unsubscribe(Instance);
            EventBus.Unsubscribe(UiEventSubscriber);
            EventBus.Unsubscribe(SpellMemorizeHandler);
            EventBus.Unsubscribe(ButtonHiderHandler);
        }
    }

}
