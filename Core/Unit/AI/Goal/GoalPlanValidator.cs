using System;
using System.Collections.Generic;
using System.Linq;
using Core.AI;
using UnityEngine;

namespace Core.Unit.AI.Goal
{
    public static partial class GoalValidator
    {
        public static bool IsGoalPossible(AiGoalContainer goal, AiActionPlan actionPlan)
        {
            var unitActionPoints = actionPlan.aiUnit.currentActionPoints;
            return goal.goalType switch
            {
                AiGoalTypes.DamagePlayer => IsUnitAttackable(unitActionPoints, actionPlan),
                AiGoalTypes.KillPlayer => IsUnitKillable(unitActionPoints, actionPlan),
                AiGoalTypes.ExposePlayer => IsUnitExposable(unitActionPoints, actionPlan),
                AiGoalTypes.PinPlayer => IsUnitPinnable(unitActionPoints, actionPlan),
                AiGoalTypes.FlankPlayer => IsUnitFlankable(unitActionPoints, actionPlan),
                AiGoalTypes.MoveToHighground => IsHighgroundAvailable(unitActionPoints, actionPlan),
                AiGoalTypes.DefendTeammate => IsTeamDefendable(unitActionPoints, actionPlan),
                AiGoalTypes.DefendSelf => IsSelfDefendable(unitActionPoints, actionPlan),
                AiGoalTypes.HealTeammate => IsTeamHealable(unitActionPoints, actionPlan),
                AiGoalTypes.HealSelf => IsSelfHealable(unitActionPoints, actionPlan),
                AiGoalTypes.BuffSelf => IsSelfBuffable(unitActionPoints, actionPlan),
                AiGoalTypes.BuffTeammate => IsTeamBuffable(unitActionPoints, actionPlan),
                AiGoalTypes.AllOutAttack => IsUnitAttackable(unitActionPoints, actionPlan),
                AiGoalTypes.AttackAtDistance => IsUnitAttackable(unitActionPoints, actionPlan),
                AiGoalTypes.AttackFromCover => (IsUnitAttackable(unitActionPoints, actionPlan) &&
                                                IsCoverAvailable(unitActionPoints, actionPlan)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static bool IsUnitMeleeable(int unitActionPoints, AiActionPlan actionPlan)
        {
            var currentUnit = actionPlan.aiUnit;
            var meleeWeapon = currentUnit.weaponObjects.FirstOrDefault(x => x.attackType == AttackType.Melee);
            return meleeWeapon != null 
                   && actionPlan.worldState.playerInfo
                       .Where(unitInfo => unitInfo.unitType == SelectableTypes.Player)
                       .Any(unitInfo => unitInfo.navPath.Count * currentUnit.moveCost + meleeWeapon.actionPointCost <= unitActionPoints);
        }

        private static bool IsUnitAttackable(int unitActionPoints, AiActionPlan actionPlan)
        {
            float longestRange = 0;
            if (actionPlan.aiUnit.weaponObjects.Where(x => x.GetType() == typeof(WarriorRangedWeapon)) 
                is List<WarriorRangedWeapon> rangedWeapons)
            {
                longestRange = rangedWeapons.OrderByDescending(x => x.range).FirstOrDefault()!.range;
            }
            bool isValid = actionPlan.worldState.playerInfo
                               .Any(unitInfo => Vector3.Distance(actionPlan.aiUnit.GetPosition(), unitInfo.unit.GetPosition()) < longestRange) 
                           || IsUnitMeleeable(unitActionPoints, actionPlan);
            if (!isValid)
            {
                //todo if unit can move to a spot where a target is in range and have enough action points left to attack
            }
            return isValid;
        }

        private static bool IsUnitKillable(int unitActionPoints, AiActionPlan actionPlan)
        {
            if (!IsUnitAttackable(unitActionPoints, actionPlan) || !IsUnitMeleeable(unitActionPoints, actionPlan))
            {
                return false;
            }
            float maxDamage = 0;
            var weaponMaxDamage = actionPlan.aiUnit.weaponObjects.OrderByDescending(x=> x.damage).FirstOrDefault();
            if (weaponMaxDamage != null) maxDamage = weaponMaxDamage.damage;
            return actionPlan.worldState.playerInfo.Any(unitInfo => maxDamage >= unitInfo.unit.currentHealth);
        }

        private static bool IsUnitExposable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsUnitPinnable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsUnitFlankable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsHighgroundAvailable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsTeamDefendable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsSelfDefendable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsTeamHealable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsSelfHealable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsSelfBuffable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }

        private static bool IsTeamBuffable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }
        
        private static bool IsCoverAvailable(int unitActionPoints, AiActionPlan actionPlan)
        {
            throw new NotImplementedException();
        }
    }
}