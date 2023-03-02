using System;
using System.Collections.Generic;
using Core.GameManagement.Interfaces;
using Core.Unit.Specials;

namespace Core.AI
{
    
    /// <summary>
    /// Data container for executing Ai actions during their turn
    /// </summary>
    public class AiActionPlan
    {
        public AIUnit aiUnit;
        public WorldState  worldState;
        
        public List<AiGoalContainer> goals;
        public AiGoalContainer selectedGoal;
        public bool hasValidGoal;

        public List<WeightedAction> actionsWeighted;
        public List<WeightedAction> selectedActions;
        private bool flagForRecalculation;

        public AiActionPlan(AIUnit aiUnit1, WorldState state)
        {
            hasValidGoal = false;
            aiUnit = aiUnit1;
            worldState = state;
            goals = new();
            actionsWeighted = new();
            selectedActions = new();
        }
    }
    
    [Serializable]
    public struct WeightedAction
    {
        public AiGoalContainer goal;
        public IAction action;
        public float weight;
    }
}