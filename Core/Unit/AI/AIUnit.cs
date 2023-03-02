using System.Collections.Generic;
using Core.Data;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ai implementation of UnitCommon 
/// </summary>
public class AIUnit : UnitCommon
{
    [FormerlySerializedAs("priorities")] [Header("AI Priorities")]
    public List<AiGoalContainer> goals;
    public override void OnEnable()
    {
        actionType = ActionTurnType.Enemy;
        selectableType = SelectableTypes.AI;
        base.OnEnable();
    }
}