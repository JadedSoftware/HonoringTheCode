using System;
using System.Collections.Generic;
using System.Linq;
using Core.Camera;
using Core.Data;
using Core.GameManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.UI
{
    /// <summary>
    /// Logic for in-game overlays, including the pause menu.
    /// </summary>
    public class GameOverlayController : MonoSingleton<GameOverlayController>
    {

        [SerializeField] private VisualTreeAsset listEntryTemplate;
        private InputControls inputControls;
        private InputAction openMenu;
        public bool IsPointerOverUI;
        public VisualElement actionOverlay;
        public List<VisualElement> allOverlays;
        public VisualElement weaponSwapListOverlay;
        public Button weaponSwapButton;
        public Button weaponSwapButtonConfirm;
        public Button attackButton;
        public Button endTurnButton;
        public Button previousUnitButton;
        public Button nextUnitButton;
        public Button guardButton;
        public Button hookShotButton;
        public Button grenadeButton;

        public ListView weaponList;

        public VisualElement turnManagementOverlay;
        private readonly UnitIndexDirection next = UnitIndexDirection.Next;
        public Button NextUnitButton;
        private readonly UnitIndexDirection previous = UnitIndexDirection.Previous;
        public Button PreviousUnitButton;
        public Button specialButton;
        
        public VisualElement menuOverlay;
        public Button saveButton;
        public Button loadButton;
        
        public VisualElement unitHudOverlay;
        private VisualElement root;

        public GameObject grenadeHoverEffect;
        private void Start()
        {
            inputControls = CameraController.instance.inputControls;
            openMenu = inputControls.UI.OpenMenu;
            openMenu.performed += ToggleOpenMenu;
            
            root = GetComponent<UIDocument>().rootVisualElement;
            actionOverlay = root.Q<VisualElement>(UiOverlayHelper.ActionOverlay);
            listEntryTemplate = (VisualTreeAsset)Resources.Load("UIElements/ListEntry");

            GameManagementController.instance.weaponListController.InitializeWeaponList(root, listEntryTemplate);

            weaponSwapListOverlay = root.Q<VisualElement>(UiOverlayHelper.WeaponSwapListOverlay);
            weaponList = root.Q<ListView>(UiOverlayHelper.WeaponList);
            weaponSwapButton = root.Q<Button>(UiOverlayHelper.WeaponSwapButton);
            weaponSwapButton.clicked += WeaponSwapButtonPressed;
            weaponSwapButtonConfirm = root.Q<Button>(UiOverlayHelper.WeaponSwapButtonConfirm);
            weaponSwapButtonConfirm.clicked += WeaponSwapButtonConfirmPressed;

            attackButton = root.Q<Button>(UiOverlayHelper.AttackButton);
            attackButton.clicked += AttackButtonPressed;
            guardButton = root.Q<Button>(UiOverlayHelper.GuardButton);
            guardButton.clicked += GuardButtonPressed;
            specialButton = root.Q<Button>(UiOverlayHelper.SpecialButton);
            specialButton.clicked += SpecialButtonPressed;
            hookShotButton = root.Q<Button>(UiOverlayHelper.HookShotButton);
            hookShotButton.clicked += HookShotPressed;
            grenadeButton = root.Q<Button>(UiOverlayHelper.GrenadeButton);
            grenadeButton.clicked += GrenadePressed;

            turnManagementOverlay = root.Q<VisualElement>(UiOverlayHelper.TurnManagementOverlay);
            endTurnButton = root.Q<Button>(UiOverlayHelper.EndTurnButton);
            endTurnButton.clicked += EndTurnPressed;
            nextUnitButton = root.Q<Button>(UiOverlayHelper.NextUnitButton);
            nextUnitButton.clicked += NextPressed;
            previousUnitButton = root.Q<Button>(UiOverlayHelper.PreviousUnitButton);
            previousUnitButton.clicked += PreviousPressed;

            unitHudOverlay = root.Q<VisualElement>(UiOverlayHelper.UnitHudOverlay);

            menuOverlay = root.Q<VisualElement>(UiOverlayHelper.MenuOverlay);
            saveButton = root.Q<Button>(UiOverlayHelper.SaveButton);
            saveButton.clicked += SaveButtonClicked;
            loadButton = root.Q<Button>(UiOverlayHelper.LoadButton);
            loadButton.clicked += LoadButtonClicked;

            menuOverlay.style.display = DisplayStyle.None;
            ToggleWeaponSwapList(DisplayStyle.None);

           
            RegisterMouseEnterCallback();
            RegisterMouseLeaveCallback();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            EventSenderController.onBeginTurn += BeginTurn;
            EventSenderController.onEndTurn += EndTurn;
            EventSenderController.onNavReady += NavReady;
            EventSenderController.onPlayerOutOActionPoints += PlayerOutOfActionPoints;
        }

        private void UnregisterEvents()
        {
            EventSenderController.onBeginTurn -= BeginTurn;
            EventSenderController.onEndTurn -= EndTurn;
            EventSenderController.onNavReady -= NavReady;
            EventSenderController.onPlayerOutOActionPoints -= PlayerOutOfActionPoints;
        }

        private void NavReady()
        {
            foreach (var playerUnit in UnitCommonController.instance.allPlayerUnits.Cast<PlayerUnit>())
                playerUnit.hudElement = new UnitHudElement(playerUnit, unitHudOverlay);
        }
        
        private void ToggleOpenMenu(InputAction.CallbackContext obj)
        {
            if (menuOverlay.style.display == DisplayStyle.None)
            {
                menuOverlay.style.display = DisplayStyle.Flex;
                EventSenderController.OnGamePaused(true);
            }
            else
            {
                menuOverlay.style.display = DisplayStyle.None;
                EventSenderController.OnGamePaused(false);
            }
        }

        public void ClearPaddingMargin(VisualElement element)
        {
            element.style.paddingBottom = 0;
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;
            element.style.paddingRight = 0;
            element.style.marginBottom = 0;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
            element.style.marginTop = 0;
        }

        private void BeginTurn(SelectableTypes endType)
        {
            switch (endType)
            {
                case SelectableTypes.Player:
                    unitHudOverlay.RemoveFromClassList(UiOverlayHelper.EnemyHudStyle);
                    unitHudOverlay.AddToClassList(UiOverlayHelper.PlayerHudStyle);
                    break;
                case SelectableTypes.AI:
                    unitHudOverlay.RemoveFromClassList(UiOverlayHelper.PlayerHudStyle);
                    unitHudOverlay.AddToClassList(UiOverlayHelper.EnemyHudStyle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(endType), endType, null);
            }
        }

        private void EndTurn() {
            RemoveEndOfTurnButtonHighlight();
        }

        private void MouseEnterCallback(MouseEnterEvent evt)
        {
            if (evt.target == actionOverlay 
                || evt.target == turnManagementOverlay 
                || evt.target == unitHudOverlay 
                || evt.target == weaponSwapListOverlay
                || (evt.target == menuOverlay && menuOverlay.style.display == DisplayStyle.Flex))
                IsPointerOverUI = true;
        }
        
        private void MouseLeaveCallback(MouseLeaveEvent evt)
        {
            if (evt.target == actionOverlay || evt.target == turnManagementOverlay || evt.target == unitHudOverlay || evt.target == menuOverlay
                || evt.target == weaponSwapListOverlay)
                IsPointerOverUI = false;
        }

        private void RegisterMouseEnterCallback()
        {
            actionOverlay.RegisterCallback<MouseEnterEvent>(MouseEnterCallback);
            turnManagementOverlay.RegisterCallback<MouseEnterEvent>(MouseEnterCallback);
            unitHudOverlay.RegisterCallback<MouseEnterEvent>(MouseEnterCallback);
            menuOverlay.RegisterCallback<MouseEnterEvent>(MouseEnterCallback);
            weaponSwapListOverlay.RegisterCallback<MouseEnterEvent>(MouseEnterCallback);
        }

        private void RegisterMouseLeaveCallback()
        {
            actionOverlay.RegisterCallback<MouseLeaveEvent>(MouseLeaveCallback);
            turnManagementOverlay.RegisterCallback<MouseLeaveEvent>(MouseLeaveCallback);
            unitHudOverlay.RegisterCallback<MouseLeaveEvent>(MouseLeaveCallback);
            menuOverlay.RegisterCallback<MouseLeaveEvent>(MouseLeaveCallback);
            weaponSwapListOverlay.RegisterCallback<MouseLeaveEvent>(MouseLeaveCallback);
        }

        private void EndTurnPressed()
        {
            EventSenderController.EndTurn();
        }

        private void PreviousPressed()
        {
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player)
                UnitCommonController.instance.UnitSelectDirection(previous);
        }

        private void NextPressed()
        {
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player)
                UnitCommonController.instance.UnitSelectDirection(next);
        }
        private void WeaponSwapButtonPressed()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player
               && GameManagementController.instance.IsUnitSelected() == false)
                return;
                //todo: refactor this to some kind of inventory
                GameManagementController.instance.weaponListController.ReEnumerateAllWeapons(FindObjectOfType<PlayerUnit>().weaponObjects);
            ToggleWeaponSwapList(DisplayStyle.Flex);
        }
        private void WeaponSwapButtonConfirmPressed()
        {
            WarriorWeaponSO selectedWeapon = (WarriorWeaponSO)weaponList.selectedItem;
            print(selectedWeapon);
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player
                && GameManagementController.instance.IsUnitSelected())
            {
                EventSenderController.WeaponSwap(GameManagementController.instance.GetCurrentUnit(), selectedWeapon);
            }
            ToggleWeaponSwapList(DisplayStyle.None);
        }

        private void ToggleWeaponSwapList(DisplayStyle displayStyle)
        {
            weaponSwapListOverlay.style.display = displayStyle;
        }

        private void AttackButtonPressed()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player
                && GameManagementController.instance.IsUnitSelected())
                EventSenderController.EnterAttackView();
        }

        private void GuardButtonPressed()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player)
                throw new NotImplementedException();
        }

        private void SpecialButtonPressed()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player)
                throw new NotImplementedException();
        }
        
        private void HookShotPressed()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player
                && GameManagementController.instance.IsUnitSelected() && CameraOverlayController.instance.currentOverlayState != CameraOverlayStates.Hookshot)
                EventSenderController.OverlayChanged(CameraOverlayStates.Hookshot);
            else
            {
                EventSenderController.OverlayChanged(CameraOverlayStates.Movement);
            }
        }

        public void SetMeleeAttackOverlayState()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player
                && GameManagementController.instance.IsUnitSelected())
            {
                EventSenderController.OverlayChanged(CameraOverlayStates.MeleeAttack);
            }
        }

        public void SetGrenadeOverlayState()
        {
            if (GameManagementController.instance.isGamePaused) return;
            if (TurnManagementController.instance.currentTurn == SelectableTypes.Player
                && GameManagementController.instance.IsUnitSelected())
            {
                EventSenderController.OverlayChanged(CameraOverlayStates.Grenade);
            }
        }

        private void SaveButtonClicked()
        {
            ToggleOpenMenu(new InputAction.CallbackContext());
            DataPersistenceController.instance.SaveGame();
        }
        
        private void LoadButtonClicked()
        {
            DataPersistenceController.instance.LoadGame();
        }

        private void PlayerOutOfActionPoints() {
            AddEndOfTurnButtonHighlight();
        }

        private void AddEndOfTurnButtonHighlight() {
            turnManagementOverlay.AddToClassList(UiOverlayHelper.TurnManagementOverlayAttentionStyle);
            endTurnButton.AddToClassList(UiOverlayHelper.EndTurnButtonAttentionStyle);
        }
        private void RemoveEndOfTurnButtonHighlight() {
            turnManagementOverlay.RemoveFromClassList(UiOverlayHelper.TurnManagementOverlayAttentionStyle);
            endTurnButton.RemoveFromClassList(UiOverlayHelper.EndTurnButtonAttentionStyle);
        }
        private void GrenadePressed()
        {
            SetGrenadeOverlayState();
            NavigableController.instance.FindGrenadeNavs(GameManagementController.instance.GetCurrentUnit());

           grenadeHoverEffect = Instantiate(EffectsController.instance.grenadeHoverEffect);
        }
    }
}