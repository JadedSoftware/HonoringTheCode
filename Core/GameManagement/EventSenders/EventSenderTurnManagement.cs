using System.Collections.Generic;
using Core.AI;

namespace Core.GameManagement.EventSenders
{
    public static partial class EventSenderController
    {
        //------------ Turn Management ---------------//
        public delegate void GamePaused(bool p0);

        public static event GamePaused onGamePaused;

        public delegate void OnBeginTurn(SelectableTypes endingType);

        public static event OnBeginTurn onBeginTurn;

        public delegate void OnEndTurn();

        public static event OnEndTurn onEndTurn;

        public delegate void OnAiActionPlanReady(List<AiActionPlan> actionPlans);

        public static event OnAiActionPlanReady onAiActionPlanReady;
        
        public delegate void OnUnitActionPointsChange(IActionable unit, int previousActionPoints);

        public static event OnUnitActionPointsChange onUnitActionPointsChange;

        public delegate void OnPlayerOutOActionPoints();

        public static event OnPlayerOutOActionPoints onPlayerOutOActionPoints;
        


        //-------------- Static Methods ------------//
        public static void OnGamePaused(bool p0)
        {
            ScheduleEvent(() => onGamePaused?.Invoke(p0));
        }

        public static void BeginTurn(SelectableTypes endingType)
        {
            ScheduleEvent(() => onBeginTurn?.Invoke(endingType));
        }

        public static void EndTurn()
        {
            ScheduleEvent(() => onEndTurn?.Invoke());
        }

        public static void AiActionPlanReady(List<AiActionPlan> actionPlans)
        {
            ScheduleEvent(() => onAiActionPlanReady?.Invoke(actionPlans));
        }
        
        public static void UnitActionPointsChange(IActionable unit, int previousActionPoints)
        {
            ScheduleEvent(() => onUnitActionPointsChange?.Invoke(unit, previousActionPoints));
        }

        public static void PlayerOutOfActionPoints()
        {
            ScheduleEvent(() => onPlayerOutOActionPoints?.Invoke());
        }
    }
}