using System.Collections.Generic;
using Core.GameManagement.Interfaces;
using TGS;
using UnityEngine;

namespace Core.GameManagement.EventSenders
{
    public static partial class EventSenderController
    {
        //------------ Selection ---------------//
        public delegate void OnMouseEnter(ISelectable selectable);

        public static event OnMouseEnter mouseEnterSelectable;

        public delegate void OnMouseExit(ISelectable selectable);

        public static event OnMouseExit mouseExitSelectable;

        public delegate void OnCellEnter(TerrainGridSystem gridSystem, int cellIndex);

        public static event OnCellEnter mouseEnterCell;

        public delegate void OnCellExit(TerrainGridSystem gridSystem, int cellIndex);

        public static event OnCellExit mouseExitCell;

        public delegate void OnHighlightCell(TerrainGridSystem gridSystem, INavigable cellIndex);

        public static event OnHighlightCell highlightCell;

        public delegate void OnUnitSelect(ISelectable unit);

        public static event OnUnitSelect unitSelected;

        public delegate void OnDeselect();

        public static event OnDeselect unitDeselected;

        public delegate void UnitHightlightDeactiveated(UnitCommon unit);

        public static event UnitHightlightDeactiveated unitHighlightDeactivated;

        public delegate void OnNavigableHighlighted(INavigable navigable);

        public static event OnNavigableHighlighted navigableHighlighted;
        
        public delegate void OnDamagableEnter(UnitCommon unit);

        public static event OnDamagableEnter damagableUnitEnter;

        public delegate void OnDamagableExit(UnitCommon unit);

        public static event OnDamagableExit damagableUnitExit;

        public delegate void MouseOverNav(INavigable navigable);

        public static event MouseOverNav mouseOverNav;

        public delegate void MouseExitNav(INavigable navigable);

        public static event MouseExitNav mouseExitNav;

        public delegate void RemoveNavigableHighlighted(INavigable navigable);

        public static event RemoveNavigableHighlighted removeNaviHighlighted;
        
        public delegate void OnHookshotsAvailable(List<IHookshotable> iHookshotables, Vector3 startPos);

        public static event OnHookshotsAvailable hookshotsAvailable;      
        
        public delegate void onHookshotsActive(IHookshotable hookshotable);

        public static event onHookshotsActive hookshotActive;        
        
        public delegate void onHookshotsDeactive();

        public static event onHookshotsDeactive hookshotDeactive;
        
        //-------------- Static Methods ------------//    
        public static void MouseEnterSelectable(ISelectable selectable)
        {
            ScheduleEvent(() => mouseEnterSelectable?.Invoke(selectable));
        }

        public static void MouseExitSelectable(ISelectable selectable)
        {
            ScheduleEvent(() => mouseExitSelectable?.Invoke(selectable));
        }

        public static void OnEnterCell(TerrainGridSystem sender, int cellindex)
        {
            ScheduleEvent(() => mouseEnterCell?.Invoke(sender, cellindex));
        }

        public static void OnExitCell(TerrainGridSystem sender, int cellindex)
        {
            ScheduleEvent(() => mouseExitCell?.Invoke(sender, cellindex));
        }

        public static void OnCellHighlight(TerrainGridSystem sender, INavigable cell)
        {
            ScheduleEvent(() => highlightCell?.Invoke(sender, cell));
        }

        public static void UnitSelected(ISelectable unit)
        {
            ScheduleEvent(() => unitSelected?.Invoke(unit));
        }

        public static void UnitDeselected()
        {
            ScheduleEvent(() => unitDeselected?.Invoke());
        }

        public static void DeactivateUnitHighlight(UnitCommon unit)
        {
            ScheduleEvent(() => unitHighlightDeactivated?.Invoke(unit));
        }

        public static void NavigableHighlighted(INavigable navigable)
        {
            ScheduleEvent(() => navigableHighlighted?.Invoke(navigable));
        }
        
        public static void DamagableUnitEnter(UnitCommon unit)
        {
            ScheduleEvent(() => damagableUnitEnter?.Invoke(unit));
        }

        public static void DamagableUnitExit(UnitCommon unit)
        {
            ScheduleEvent(() => damagableUnitExit?.Invoke(unit));
        }

        public static void MouseOverNavigable(INavigable navigable)
        {
            ScheduleEvent(() => mouseOverNav?.Invoke(navigable));
        }

        public static void MouseExitNavigable(INavigable lastHit)
        {
            ScheduleEvent(() => mouseExitNav?.Invoke(lastHit));
        }

        public static void RemoveNaviHighlighted(INavigable navigable)
        {
            ScheduleEvent(() => removeNaviHighlighted?.Invoke(navigable));
        }

        public static void HookshotTargetsAvailable(List<IHookshotable> availableHookshots, Vector3 startPos)
        {
            ScheduleEvent(() => hookshotsAvailable?.Invoke(availableHookshots, startPos));
        }

        public static void HookshotTargetActive(IHookshotable hookShotable)
        {
            ScheduleEvent(() => hookshotActive?.Invoke(hookShotable));
        }        
        
        public static void HookshotTargetDeactive()
        {
            ScheduleEvent(() => hookshotDeactive?.Invoke());
        }
    }
}