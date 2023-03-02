
using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ai priorities holder
/// </summary>
[Serializable]
public class AiGoalContainer
{
    public AiGoalTypes goalType; 
    public float goalWeight;
    private bool flagForRecalculation;
}

public enum AiGoalTypes
{
    DamagePlayer,
    KillPlayer,
    ExposePlayer,
    PinPlayer,
    FlankPlayer,
    MoveToHighground,
    DefendTeammate,
    DefendSelf,
    HealTeammate,
    HealSelf,
    BuffSelf,
    BuffTeammate,
    AllOutAttack,
    AttackAtDistance,
    AttackFromCover,
}