using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameManagement.EventSenders;
using Core.GameManagement.Interfaces;
using Core.Unit.AI.Goal;
using Core.Unit.AI.Goal.GoalActions;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Core.AI
{
    /// <summary>
    /// Handles assigning Ai actions during their turn
    /// </summary>
    public class AiController : MonoSingleton<AiController>
    {
        public List<AiActionPlan> allActionPlans;
        public List<AiActionPlan> cachedActions;

        public void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            EventSenderController.onBeginTurn += OnBeginTurn;
        }

        private void UnRegisterEvents()
        {
            EventSenderController.onBeginTurn -= OnBeginTurn;
        }

        private void OnBeginTurn(SelectableTypes type)
        {
            allActionPlans = new();
            switch (type)
            {
                case SelectableTypes.AI:
                {
                    foreach (var unit in UnitCommonController.instance.allUnits)
                    {
                        // Create estimation objects,
                        // these allow us to determine what the ActionPlan is going to accomplish 
                        // before executing.
                        ActionEstimation.CreateEstimate(unit);
                    }
                    foreach (var aiUnit in UnitCommonController.instance.allAiUnits.Where(x => x.isAlive))
                    {
                        // get the world state and create an action plan object
                        // goals are first validated that they are possible this turn
                        BuildActionPlan(aiUnit);
                    }

                    foreach (var actionPlan in allActionPlans)
                    {
                        // from the valid goals, select a random one in a weighted function
                        // then build all possible actions to accomplish that goal
                        // weight the actions based on how well they accomplish the goal
                        actionPlan.selectedGoal = GoalPlanner.EstablishGoal(actionPlan.goals);
                        GoalPlanner.BuildPlan(actionPlan,  actionPlan.selectedGoal);
                        GoalPlanner.EstablishAction(actionPlan);
                    }
                    GoalValidator.ValidateActions(allActionPlans);
                    
                    ExecuteActionPlan(allActionPlans);
                    break;
                }
                case SelectableTypes.Player:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void BuildActionPlan(AIUnit aiUnit)
        {
            WorldState worldState = new(GetPlayerInfo(aiUnit), GetMovableNavs(aiUnit));
            AiActionPlan actionPlan = new(aiUnit, worldState);
            var validGoals = new List<AiGoalContainer>();
            foreach (var goal in aiUnit.goals.Where(goal => GoalValidator.IsGoalPossible(goal, actionPlan)))
            {
                actionPlan.hasValidGoal = true;
                validGoals.Add(goal);
            }

            if (!actionPlan.hasValidGoal)
            {
                var newGoal = GoalPlanner.CalculateGoal(actionPlan);
                validGoals.Add(newGoal);
                actionPlan.hasValidGoal = true;
            }
            actionPlan.goals.AddRange(validGoals);
            allActionPlans.Add(actionPlan);
        }

        private void ExecuteActionPlan(List<AiActionPlan> aiActionPlans)
        {
            DebugController.instance.Log("Execute ActionPlan: ");
            foreach (var actionPlan in aiActionPlans)
            {
                var actions = actionPlan.selectedActions;
                 
            }
        }

        private List<INavigable> GetMovableNavs(AIUnit aiUnit)
        {
            var navs = new List<INavigable>();
            if (aiUnit is IMovable moveable)
            {
                var navPath = NavPathJobs.instance.NavsInRange(aiUnit.GetCurrentNavigable().GetNavIndex(),
                    moveable.GetMoveDistance());
                foreach (var i in navPath)
                {
                    var nav = NavigableController.instance.GetNavigable(i);
                    navs.Add(nav);
                }

                navPath.Dispose();
            }

            return navs;
        }

        private List<UnitInfo> GetPlayerInfo(UnitCommon startingUnit)
        {
            return UnitCommonController.instance.allUnits
                .Where(x => x.isAlive)
                .Select(unit => new UnitInfo(unit, unit.GetSelectableType(),
                    NavPathJobs.instance.FindPath(startingUnit.GetCurrentNavigable().GetNavIndex(),
                        unit.GetCurrentNavigable().GetNavIndex()))).ToList();
        }

        private Dictionary<INavigable, int> FindClosestHighground(List<INavigable> moveableNavs)
        {
            throw new NotImplementedException();
        }
    }
}