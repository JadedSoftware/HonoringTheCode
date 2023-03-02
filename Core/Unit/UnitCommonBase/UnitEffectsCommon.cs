using System;
using Core.GameManagement;
using HighlightPlus;
using UnityEngine;
using UnityEngine.EventSystems;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

public abstract partial class UnitCommon : IPointerEnterHandler, IPointerExitHandler, ISelectable
{
    [Header("Effects")] [SerializeField] private HighlightProfile highLightProfile;
    [Header("Attackable Settings")] public Color attackableColor;
    public Color mouseOverColor;
    public Color selectedColor;
    protected bool hasMouseOver;
    private HighlightEffect highlightEffect;
    
    private bool isSelected;

    public ISelectable GetSelectable()
    {
        return this;
    }

    public void RegisterSelectable(ISelectable selectable)
    {
        throw new NotImplementedException();
    }

    public void UnRegisterSelectable(ISelectable selectable)
    {
        throw new NotImplementedException();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        switch (CameraController.instance.cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                PointerEnterTopdown();
                break;
            case CameraViewStates.Attack:
                PointerEnterAttackview();
                break;
            case CameraViewStates.EnemyTurn:
                break;
            case CameraViewStates.AttackOrbit:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        switch (CameraController.instance.cameraStateMachine.CurrentKey)
        {
            case CameraViewStates.Topdown:
                PointerExitTopdown();
                break;
            case CameraViewStates.Attack:
                PointerExitAttackview();
                break;
            case CameraViewStates.EnemyTurn:
                break;
            case CameraViewStates.AttackOrbit:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public INavigable SelectableCurrentNavigable()
    {
        return currentNavPoint;
    }

    public virtual void OnUnitSelected(ISelectable unit)
    {
        if ((UnitCommon) unit != this) return;
        isSelected = true;
        SetHighlight(true, selectedColor);
    }

    public virtual void OnUnitDeselect()
    {
        isSelected = false;
        if (hasMouseOver)
            SetHighlight(true, mouseOverColor);
        else
            SetHighlight(false, Color.clear);
    }

    public SelectableTypes GetSelectableType()
    {
        return selectableType;
    }

    private void PointerEnterTopdown()
    {
        switch (TurnManagementController.instance.currentTurn)
        {
            case SelectableTypes.Player:
                switch (selectableType)
                {
                    case SelectableTypes.Player:
                        if (!isSelected)
                            SetHighlight(true, mouseOverColor);
                        hasMouseOver = true;
                        EventSenderController.MouseEnterSelectable(this);
                        break;

                    case SelectableTypes.AI:
                        if (gameManagementContoller.GetCurrentSelectable() != null)
                        {
                            SetHighlight(true, attackableColor);
                            hasMouseOver = true;
                            EventSenderController.MouseEnterSelectable(this);
                            EventSenderController.DamagableUnitEnter(this);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case SelectableTypes.AI:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PointerExitTopdown()
    {
        switch (TurnManagementController.instance.currentTurn)
        {
            case SelectableTypes.Player:
                switch (selectableType)
                {
                    case SelectableTypes.Player:
                        if (!isSelected)
                            SetHighlight(false, Color.clear);
                        hasMouseOver = false;
                        EventSenderController.MouseExitSelectable(this);
                        break;
                    case SelectableTypes.AI:
                        if (gameManagementContoller.GetCurrentSelectable() != null)
                        {
                            SetHighlight(false, attackableColor);
                            hasMouseOver = false;
                            EventSenderController.MouseExitSelectable(this);
                            EventSenderController.DamagableUnitExit(this);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case SelectableTypes.AI:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PointerEnterAttackview()
    {
        if (!ReferenceEquals(UnitCommonController.instance.pointerOverDamagable, damageable))
            EventSenderController.DamagableUnitEnter(this);
    }

    private void PointerExitAttackview()
    {
        EventSenderController.DamagableUnitExit(this);
    }

    private void SetHighlightEffect()
    {
        highlightEffect = gameObject.AddComponent<HighlightEffect>();
        highlightEffect.ProfileLoad(highLightProfile);
    }

    protected void SetHighlight(bool enable, Color highlightColor)
    {
        highlightEffect.highlighted = enable;
        highlightEffect.outlineColor = highlightColor;
        highlightEffect.innerGlowColor = highlightColor;
        highlightEffect.seeThroughBorderColor = highlightColor;
        highlightEffect.SetGlowColor(highlightColor);
        highlightEffect.UpdateMaterialProperties();
    }

    protected void RefreshMovableNavs()
    {
        navController.FindMovableNavs(this);
    }

    private void EnterAttackView()
    {
        if (isSelected) SetHighlight(false, Color.clear);
    }

    private void EnterTopdownView()
    {
        if (isSelected) SetHighlight(true, selectedColor);
    }

    private void OnUnitTargeted(UnitCommon unitTarget)
    {
        if (unitTarget == this) SetHighlight(true, attackableColor);
    }

    private void ExitAttackView()
    {
        if (isSelected) SetHighlight(true, selectedColor);
    }
}