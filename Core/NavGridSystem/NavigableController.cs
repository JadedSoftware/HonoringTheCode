using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Core;
using Core.Camera;
using Core.GameManagement;
using Core.GameManagement.Interfaces;
using Core.Helpers;
using Core.UI;
using Core.Unit.Specials;
using Drawing;
using TGS;
using UnityEngine;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

/// <summary>
/// Handles navigation effects like cell highlights
/// Keeps track of current highlighted cell
/// </summary>
public class NavigableController : MonoSingleton<NavigableController>, IEvents
{
    public Color selectedCellColor;
    public Color cellHighlightColor;
    public Color cellOccupiedColor;
    public Color deselectCellColor = Color.clear;
    private readonly List<int> blinkedNavs = new();

    private Dictionary<int, TerrainGridSystem> currentHighlightedGrids;
    private List<int> currentNavPath = new();
    private bool isFading;
    private bool isOverDamagable;

    [HideInInspector]public readonly List<int> possibleNavPoints = new();
    private HIGHLIGHT_EFFECT previousHighLightEffect;
    public INavigable selectedNavigable;
    public INavigable highlightedNavigable { get; private set; }
    private GridController gridController => GridController.instance;
    private UnitCommonController unitControl => UnitCommonController.instance;
    private GameManagementController GameManagementController => GameManagementController.instance;
    private CameraController camControl => CameraController.instance;
    private NavPathJobs navPathJobs => NavPathJobs.instance;

    public GameObject grenadeHoverEffect;

    private void OnEnable()
    {
        RegisterEvents();
        currentHighlightedGrids = new Dictionary<int, TerrainGridSystem>();
    }

    private void OnDisable()
    {
        UnRegisterEvents();
    }

    public void RegisterEvents()
    {
        EventSenderController.mouseEnterSelectable += MouseEnterSelectable;
        EventSenderController.mouseExitSelectable += MouseExitSelectable;
        EventSenderController.mouseOverNav += MouseOverNavigable;
        EventSenderController.mouseExitNav += MouseExitNavigable;
        EventSenderController.navigableHighlighted += SetHighlightNavigable;
        EventSenderController.unitSelected += OnUnitSelected;
        EventSenderController.unitDeselected += OnUnitDeselected;
        EventSenderController.unitMovementInitiated += UnitMovementInitiated;
        EventSenderController.unitMovementComplete += UnitMovementComplete;
        EventSenderController.damagableUnitEnter += DamageableEnter;
        EventSenderController.damagableUnitExit += DamageableExit;
        EventSenderController.onOverlayChanged += OnOverlayChanged;
        foreach (var gridSystem in TerrainGridSystem.grids)
        {
            gridSystem.cellHighlightColor = cellHighlightColor;
            gridSystem.OnCellEnter += OnCellEnter;
            gridSystem.OnCellHighlight += OnCellHighlight;
            gridSystem.OnCellExit += OnCellExit;
        }
    }
    
    public void UnRegisterEvents()
    {
        EventSenderController.mouseEnterSelectable -= MouseEnterSelectable;
        EventSenderController.mouseExitSelectable -= MouseExitSelectable;
        EventSenderController.mouseOverNav -= MouseOverNavigable;
        EventSenderController.mouseExitNav -= MouseExitNavigable;
        EventSenderController.navigableHighlighted -= SetHighlightNavigable;
        EventSenderController.unitSelected -= OnUnitSelected;
        EventSenderController.unitDeselected -= OnUnitDeselected;
        EventSenderController.unitMovementInitiated -= UnitMovementInitiated;
        EventSenderController.unitMovementComplete -= UnitMovementComplete;
        EventSenderController.damagableUnitEnter -= DamageableEnter;
        EventSenderController.damagableUnitExit -= DamageableExit;
        EventSenderController.onOverlayChanged -= OnOverlayChanged;
        foreach (var gridSystem in TerrainGridSystem.grids)
        {
            gridSystem.OnCellEnter -= OnCellEnter;
            gridSystem.OnCellHighlight -= OnCellHighlight;
            gridSystem.OnCellExit -= OnCellExit;
        }
    }

    public bool IsPossibleNav(INavigable moveNavPoint)
    {
        return possibleNavPoints.Contains(moveNavPoint.GetNavIndex());
    }

    private void MouseEnterSelectable(ISelectable selectable)
    {
        highlightedNavigable = null;
        var gridSystem = TerrainGridSystem.GetGridAt(selectable.GetPosition());
        var cell = gridSystem.CellGetAtPosition(selectable.GetPosition(), true);
        gridSystem.HighlightCellRegion(cell.index, true);
    }

    private void MouseExitSelectable(ISelectable selectable)
    {
        if (highlightedNavigable != null) return;

        if (!Physics.Raycast(camControl.GetMouseRay(), out var hit, 1000, LayerMaskHelper.levelLayerMask)) return;

        if (!hit.collider.CompareTag("Terrain") && !hit.collider.CompareTag("Grid")) return;
        var gridSystem = TerrainGridSystem.GetGridAt(hit.point);
        if (gridSystem == null) return;
        var cell = gridSystem.CellGetAtPosition(hit.point, true);
        if (cell != null)
        {
            gridSystem.HighlightCellRegion(cell.index, false);
        }
        else
        {
            Collider[] overlap;
            overlap = Physics.OverlapSphere(hit.point, 1.25f, LayerMaskHelper.cellLayerMask);
            foreach (var grid in overlap)
            {
                var newCell = gridSystem.CellGetAtPosition(grid.bounds.center, true);
                Debug.Log(newCell);
            }
        }
    }

    private void OnCellEnter(TerrainGridSystem sender, int cellindex)
    {
        if (GameOverlayController.instance.IsPointerOverUI) return;
        if (camControl.cameraStateMachine.CurrentKey != CameraViewStates.Topdown) return;
        if (!currentHighlightedGrids.ContainsKey(cellindex)) currentHighlightedGrids.Add(cellindex, sender);
        EventSenderController.OnEnterCell(sender, cellindex);
        if(GameOverlayController.instance.grenadeHoverEffect != null)
        {
            var nav = GetNavFromCellIndex(cellindex, sender);
            GameOverlayController.instance.grenadeHoverEffect.transform.position = nav.GetPosition();
        }
    }

    private void OnCellExit(TerrainGridSystem sender, int cellindex)
    {
        if (currentHighlightedGrids.ContainsKey(cellindex)) currentHighlightedGrids.Remove(cellindex);
        if (currentHighlightedGrids.Count == 0) highlightedNavigable = null;
        EventSenderController.OnExitCell(sender, cellindex);
    }

    private void OnCellHighlight(TerrainGridSystem sender, int cellindex, ref bool cancelhighlight)
    {
        var cell = sender.cells.FirstOrDefault(a => a.index == cellindex);
        var navCell = cell.navCell;
        var cancel = true;
        if (GameOverlayController.instance.IsPointerOverUI
            || GameManagementController.instance.isGamePaused
            || camControl.cameraStateMachine.CurrentKey == CameraViewStates.Attack
            || camControl.cameraStateMachine.CurrentKey == CameraViewStates.AttackOrbit)
        {
            cancelhighlight = cancel;
            return;
        }

        if (Physics.Raycast(CameraController.instance.GetMouseRay(), 1000, LayerMaskHelper.unitLayerMask) ||
            navCell.IsOccupied())
        {
            cancelhighlight = cancel;
            return;
        }

        if (!Physics.Raycast(CameraController.instance.GetMouseRay(), 1000, LayerMaskHelper.selectableMask))
        {
            cancelhighlight = cancel;
            return;
        }

        if (navCell.canBeHighlighted)
        {
            if (currentHighlightedGrids.Count > 1)
            {
                var gridSystem = currentHighlightedGrids.Where(a => a.Value != sender);
                foreach (var system in gridSystem) system.Value.HideCellHighlightObject();
            }

            cancel = false;
            EventSenderController.NavigableHighlighted(navCell);
            EventSenderController.OnCellHighlight(sender, navCell);
        }

        cancelhighlight = cancel;
    }
    private void SetCellColor(Cell selectedCell, Color color)
    {
        SetCellVisable(selectedCell);
        selectedCell.navCell.gridSystem.CellSetColor(selectedCell, color);
    }

    private void SetCellVisable(Cell cell)
    {
        var gridSystem = cell.navCell.gridSystem;
        gridSystem.showCells = true;
        gridSystem.CellSetBorderVisible(cell.index, true);
        gridSystem.CellSetVisible(cell.index, true);
    }

    private void MouseOverNavigable(INavigable navigable)
    {
        if (navigable is NavCell) return;
        EventSenderController.NavigableHighlighted(navigable);
    }

    private void MouseExitNavigable(INavigable navigable)
    {
        //highlightedNavigable = null;
    }

    public INavigable GetNavigable(int pointIndex)
    {
        return gridController.allNavigables.First(a => a.GetNavIndex() == pointIndex);
    }

    public INavigable GetNavFromCellIndex(int cellIndex, TerrainGridSystem gridSystem)
    {
        var cell = gridSystem.cells.Find(a => a.index == cellIndex);
        return cell.navCell;
    }

    public INavigable NavAtPosition(RaycastHit hit)
    {
        var obj = hit.collider.gameObject;
        INavigable navPoint;
        switch ((LayersEnum) obj.layer)
        {
            case LayersEnum.Terrain:
                navPoint = NavByPosition(hit.point);
                if (navPoint != null) return navPoint;
                break;
            case LayersEnum.NavCell:
            case LayersEnum.Obstacle:
                if (highlightedNavigable is NavGrapplePoint) return highlightedNavigable;
                if (highlightedNavigable is NavCell)
                {
                    navPoint = NavByPosition(hit.point);
                    if (navPoint != null) return navPoint;
                }

                break;
            case LayersEnum.Ledge:
            case LayersEnum.GrapplePoint:
                if (obj.GetComponent<INavigable>() is NavGrapplePoint grapplePoint) return grapplePoint;
                break;
            default:
                Debug.Log("Null in Switch: " + obj + "\n"
                          + "Layer : " + (LayersEnum) obj.layer);
                return null;
        }

        Debug.Log("Null out of Switch: " + obj + "\n"
                  + "Layer : " + (LayersEnum) obj.layer);
        return null;
    }

    public INavigable NavByPosition(Vector3 hitPoint)
    {
        var gridAtPos = TerrainGridSystem.GetGridAt(hitPoint);
        var cell = gridAtPos.CellGetAtPosition(hitPoint, true);
        return cell?.navCell;
    }

    private void SetHighlightNavigable(INavigable navigable)
    {
        highlightedNavigable = navigable;
    }

    private void HighlightMovableNavs(List<int> neighborsInRange)
    {
        possibleNavPoints.Clear();
        possibleNavPoints.AddRange(neighborsInRange);
        StartCoroutine(HighlightPossibleNavigables(neighborsInRange));
    }
    private void HighlightGrenadeNavs(List<int> neighborsInRange)
    {
        possibleNavPoints.Clear();
        possibleNavPoints.AddRange(neighborsInRange);
        StartCoroutine(HighlightPossibleGrenadeNavigables(neighborsInRange));
    }

    private void HighlightSelectedNavigable(int index)
    {
        var nav = GetNavigable(index);
        switch (nav.GetNavType())
        {
            case NavigableTypes.NavCell:
                if (nav is NavCell selectedCell)
                {
                    var selectedCellIndex = selectedCell.GetCellIndex();
                    selectedNavigable = selectedCell;
                    possibleNavPoints.Add(selectedCellIndex);
                    SetCellColor(selectedCell.cell, selectedCellColor);
                    selectedCell.gridSystem.showCells = true;
                }
                break;
            case NavigableTypes.Ledge:
                break;
            case NavigableTypes.GrapplePoint:
                if (nav is NavGrapplePoint grapplePoint) grapplePoint.ActivateMovableEffect();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public List<INavigable> meleeMoveCells = new List<INavigable>();
    public List<int> grenadeThrowingCells = new List<int>();
    public void HighlightMeleeNavigables(int index)
    {
        var nav = GetNavigable(index);
        switch (nav.GetNavType())
        {
            case NavigableTypes.NavCell:
                if (nav is NavCell selectedCell)
                {
                    var selectedCellIndex = selectedCell.GetCellIndex();
                    SetCellColor(selectedCell.cell, ColorHelper.GetColor(ColorPallete.Red, ColorShade.Dark));
                    selectedCell.gridSystem.showCells = true;
                }

                break;
            case NavigableTypes.Ledge:
                break;
            case NavigableTypes.GrapplePoint:
                if (nav is NavGrapplePoint grapplePoint) grapplePoint.ActivateMovableEffect();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    

    private void HighlightNavigables(List<int> navigablesIndex)
    {
        foreach (var navigable in navigablesIndex.Select(GetNavigable))
            switch (navigable.GetNavType())
            {
                case NavigableTypes.NavCell:
                    if (navigable is NavCell navCell) SetCellVisable(navCell.cell);
                    break;
                case NavigableTypes.Ledge:
                    break;
                case NavigableTypes.GrapplePoint:
                    if (navigable is NavGrapplePoint grapplePoint) grapplePoint.ActivateMovableEffect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }

    private IEnumerator HighlightPossibleNavigables(List<int> neighborsByDist)
    {
        var removeOccpiedCells = new List<int>();
        yield return new WaitForEndOfFrame();
        foreach (var nav in neighborsByDist.Select(navIndex => GetNavigable(navIndex)))
        {
            if (!nav.IsTraversable() || nav.IsOccupied()) continue;
            switch (nav.GetNavType())
            {
                case NavigableTypes.NavCell:
                    if (nav is NavCell navCell)
                    {
                        if (navCell.IsOccupied() || navCell.CompareTag(UserTags.offGridTag))
                        {
                            removeOccpiedCells.Add(nav.GetNavIndex());
                            continue;
                        }

                        SetCellVisable(navCell.cell);
                    }

                    break;
                case NavigableTypes.Ledge:
                    nav.ActivateMovableEffect();
                    break;
                case NavigableTypes.GrapplePoint:
                    nav.ActivateMovableEffect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var grid in TerrainGridSystem.grids) grid.highlightEffect = HIGHLIGHT_EFFECT.Default;
        RemoveNavHighlights(removeOccpiedCells);
    }

    private IEnumerator HighlightPossibleGrenadeNavigables(List<int> neighborsByDist)
    {
        var removeOccpiedCells = new List<int>();
        yield return new WaitForEndOfFrame();
        foreach (var nav in neighborsByDist.Select(navIndex => GetNavigable(navIndex)))
        {
            if (!nav.IsTraversable() ) continue;
            switch (nav.GetNavType())
            {
                case NavigableTypes.NavCell:
                    if (nav is NavCell navCell)
                    {
                        if (navCell.CompareTag(UserTags.offGridTag))
                        {
                            removeOccpiedCells.Add(nav.GetNavIndex());
                            continue;
                        }

                        SetCellColor(navCell.cell, ColorHelper.GetColor(ColorPallete.Red, ColorShade.Dark));
                        navCell.gridSystem.showCells = true;
                    }

                    break;
                case NavigableTypes.Ledge:
                    nav.ActivateMovableEffect();
                    break;
                case NavigableTypes.GrapplePoint:
                    nav.ActivateMovableEffect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var grid in TerrainGridSystem.grids) grid.highlightEffect = HIGHLIGHT_EFFECT.Default;
        RemoveNavHighlights(removeOccpiedCells);
    }

    private void RemoveNavHighlights(List<int> navs)
    {
        var removeCellList = new List<int>();
        foreach (var nav in navs)
        {
            var navigable = GetNavigable(nav);
            switch (navigable.GetNavType())
            {
                case NavigableTypes.NavCell:
                    if (navigable is NavCell navCell)
                    {
                        navCell.gridSystem.CellClear(navCell.GetCellIndex());
                        navCell.gridSystem.CellSetVisible(navCell.GetCellIndex(), false);
                    }

                    break;
                case NavigableTypes.Ledge:
                    break;
                case NavigableTypes.GrapplePoint:
                    if (navigable is NavGrapplePoint grapplePoint) grapplePoint.grappleMarkerEffect.StopAllEffect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            removeCellList.Add(nav);
        }

        foreach (var cell in removeCellList) possibleNavPoints.Remove(cell);
    }

    private void ClearCachedNavigables()
    {
        StopAllCoroutines();
        isFading = false;
        foreach (var nav in possibleNavPoints.Select(navPoint => GetNavigable(navPoint))) nav.SetCanvasActive(false);
        foreach (var nav in currentNavPath.Select(navPoint => GetNavigable(navPoint))) nav.SetCanvasActive(false);
        RemoveNavHighlights(currentNavPath);
        RemoveNavHighlights(possibleNavPoints);
        foreach (var grid in TerrainGridSystem.grids) grid.showCells = false;
        blinkedNavs.Clear();
        currentNavPath.Clear();
        possibleNavPoints.Clear();
        meleeMoveCells.Clear();
        selectedNavigable = null;
    }

    private void ClearSelectedGrid()
    {
        foreach (var grid in TerrainGridSystem.grids)
        {
            grid.highlightEffect = HIGHLIGHT_EFFECT.None;
            grid.showCells = false;
        }

        if (selectedNavigable is NavCell selectedNavCell)
        {
            var selectedCellIndex = selectedNavCell.GetCellIndex();
            var selectedGridSystem = selectedNavCell.gridSystem;
            selectedGridSystem.showCells = false;
            selectedGridSystem.CellSetBorderVisible(selectedCellIndex, false);
            selectedGridSystem.CellSetVisible(selectedCellIndex, false);
            selectedGridSystem.CellSetColor(selectedCellIndex, deselectCellColor);
        }
    }

    private void OnUnitSelected(ISelectable unit)
    {
        ClearCachedNavigables();
        FindMovableNavs(unit);
    }

    public void OnUnitDeselected()
    {
        ClearSelectedGrid();
        ClearCachedNavigables();
    }

    private void UnitMovementInitiated(IMovable unit, Stack<int> navPath, INavigable endNavPoint)
    {
        foreach (var navPoint in possibleNavPoints) GetNavigable(navPoint).SetCanvasActive(false);
        RemoveNavHighlights(possibleNavPoints);
        ClearCachedNavigables();
        currentNavPath = new List<int>();
        var moveNavigable = unit.GetCurrentNavigable();
        currentNavPath.Add(moveNavigable.GetNavIndex());
        foreach (var navigable in navPath.Select(GetNavigable))
        {
            navigable.SetCanvasActive(true);
            if (!currentNavPath.Contains(navigable.GetNavIndex()))
                currentNavPath.Add(navigable.GetNavIndex());
        }

        HighlightNavigables(currentNavPath);
        HighlightSelectedNavigable(endNavPoint.GetNavIndex());
    }

    private void UnitMovementComplete(IMovable unit)
    {
        if (unit == GameManagementController.instance.GetCurrentSelectable())
            FindMovableNavs(unit.GetSelectable());
    }

    public void FindMovableNavs(ISelectable unit)
    {
        ClearCachedNavigables();
        selectedNavigable = unit.SelectableCurrentNavigable();
        if (unit is IMovable movable)
        {
            var nativeNavList = navPathJobs.NavsInRange(unit.SelectableCurrentNavigable().GetNavIndex(),
                movable.GetMoveDistance());
            List<int> neighborsInRange = new();
            foreach (var nav in nativeNavList)
            {
                if (neighborsInRange.Contains(nav))
                    continue;
                neighborsInRange.Add(nav);
            }

            HighlightMovableNavs(neighborsInRange);
            nativeNavList.Dispose();
        }
    }

    public void FindGrenadeNavs(ISelectable unit)
    {
        //ClearCachedNavigables();
        selectedNavigable = unit.SelectableCurrentNavigable();
        if (unit is IMovable movable)
        {
            var nativeNavList = navPathJobs.NavsInRange(unit.SelectableCurrentNavigable().GetNavIndex(),
                5);
            foreach (var nav in nativeNavList)
            {
                if (grenadeThrowingCells.Contains(nav))
                    continue;
                grenadeThrowingCells.Add(nav);
            }

            HighlightGrenadeNavs(grenadeThrowingCells);
            nativeNavList.Dispose();
        }
    }

    private void DamageableEnter(UnitCommon unit)
    {
        isOverDamagable = true;
        var selectedUnit = GameManagementController.instance.GetCurrentSelectable();
        var selectedWarrior = selectedUnit.GetWarrior();
        var selectedMovable = selectedUnit.GetMovable();
        if (selectedWarrior != null && possibleNavPoints.Count > 0)
        {
            var testDepth = selectedMovable.GetMoveDistance();
            var attackCost = selectedWarrior.AttackActionCost();
            var depth = selectedMovable.PredictMoveDistance(attackCost);
            var nativeNavList = navPathJobs.NavsInRange(selectedUnit.SelectableCurrentNavigable().GetNavIndex(),
                depth);

            List<int> neighborsInRange = new();
            foreach (var nav in nativeNavList)
            {
                if (neighborsInRange.Contains(nav))
                    continue;
                neighborsInRange.Add(nav);
            }

            nativeNavList.Dispose();
            var blinkNavs = new List<int>(possibleNavPoints);
            foreach (var nav in neighborsInRange.Where(nav => blinkNavs.Contains(nav))) blinkNavs.Remove(nav);

            if (!isFading) StartCoroutine(BlinkCells(blinkNavs));
        }
    }

    private void DamageableExit(UnitCommon unit)
    {
        isOverDamagable = false;
    }

    private IEnumerator BlinkCells(List<int> navCellIndexs)
    {
        isFading = true;
        var yellow = ColorHelper.GetColor(ColorPallete.Yellow, ColorShade.Darkest);
        yellow.a = .75f;
        foreach (var nav in navCellIndexs.Select(index => GetNavigable(index)))
            if (nav is NavCell navCell)
            {
                blinkedNavs.Add(nav.GetNavIndex());
                var gridSystem = navCell.gridSystem;
                gridSystem.CellFadeOut(navCell.GetCellIndex(), yellow, 3f);
            }

        yield return new WaitForSeconds(3f);
        isFading = false;
        if (isOverDamagable) StartCoroutine(BlinkCells(navCellIndexs));
    }

    public List<IHookshotable> AvailableHookshots(Vector3 startPos, SpecialHookshot specialHookshot)
    {
        List<NavPoint> pointsInRange = new();
        List<IHookshotable> availablePoints = new();
        pointsInRange.AddRange(gridController.allNavLedges);
        pointsInRange.AddRange(gridController.allGrapplePoints);
        var navPoints = pointsInRange.Where(point => Vector3.Distance(startPos, point.GetPosition()) < specialHookshot.range).ToList();
        foreach (var navPoint in navPoints)
        {
            var endPos = navPoint.GetPosition();
            var ray = new Ray(startPos, endPos - startPos);
            if(Physics.Raycast(ray, out RaycastHit hitInfo, Vector3.Distance(startPos, endPos) * 1.2f, LayerMaskHelper.moveLayerMask))
            {
                if (hitInfo.collider == navPoint.GetCollider())
                {
                    possibleNavPoints.Add(navPoint.GetNavIndex());
                    availablePoints.Add(navPoint);
                }
            }
        }
        return availablePoints;
    }
    private void OnOverlayChanged(CameraOverlayStates overlayState)
    {
        switch (overlayState)
        {
            case CameraOverlayStates.Movement:
                
                ClearSelectedGrid();
                ClearCachedNavigables();
                OnMovementOverlay();
                break;
            case CameraOverlayStates.Tactical:

                ClearSelectedGrid();
                ClearCachedNavigables();
                OnTacticalOverlay();
                break;
            case CameraOverlayStates.Hookshot:

                ClearSelectedGrid();
                ClearCachedNavigables();
                OnHookshotOverlay(GameManagementController.instance.GetCurrentUnit());
                break;
            case CameraOverlayStates.Ai:
                OnAiOverlay();
                break;
            case CameraOverlayStates.MeleeAttack:
                OnMeleeAttackOverlay();
                break;
            case CameraOverlayStates.Grenade:
                OnGrenadeOverlay();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(overlayState), overlayState, null);
        }
    }
    private void OnMovementOverlay()
    {
        if (!GameManagementController.instance.IsUnitSelected())
        {
            UnitCommonController.instance.UnitSelectDirection(UnitIndexDirection.Next);
        }
        FindMovableNavs(GameManagementController.instance.GetCurrentSelectable());
    }

    private void OnTacticalOverlay()
    {
    }
    
    private void OnHookshotOverlay(UnitCommon selectedUnit)
    {
        var hookshotData = selectedUnit.specialMovesList.FirstOrDefault(x => x.specialType == SpecialTypes.Hookshot);
        if (hookshotData is SpecialHookshot specialHookshot)
        {
            var startPos = selectedUnit.motor.transform.position + selectedUnit.motor.Capsule.center;
            EventSenderController.HookshotTargetsAvailable(AvailableHookshots(startPos, specialHookshot), startPos );
        }
    }
    
    private void OnAiOverlay()
    {
    }

    private void OnGrenadeOverlay()
    {

    }
    private void OnMeleeAttackOverlay()
    {
        foreach (var meleeCell in meleeMoveCells)
        {
            HighlightMeleeNavigables(meleeCell.GetNavIndex());
        }
        
    }

}