using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameManagement;
using Core.Unit.Targeting;
using Core.Unit.Warrior;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.Unit
{
    public enum TargetingType
    {
        BiPedal,
        Quadruped
    }

    public enum TargetingObjectType
    {
        None,
        Head,
        Chest,
        RightArm,
        RightHand,
        LeftArm,
        LeftHand,
        RightLeg,
        RightFoot,
        LeftLeg,
        LeftFoot
    }
    /// <summary>
    /// Handles all aspects of targeting different parts of the unit, like head/body/arms/legs.
    /// Provides methods for changing which area is targeted.
    /// </summary>

    public class TargetingController : MonoSingleton<TargetingController>
    {

        public CompassDirections lastDirection = CompassDirections.None;
        public TargetingObjectCrosshair crosshairPrefab;
        public TargetingHealthUI targetHealthPrefab;
        private TargetingObjectCrosshair activeCrosshair;
        private TargetingHealthUI activeTargetingHealth;

        private readonly List<TargetingObjectCrosshair> crosshairs = new();
        private bool isAttackView;
        private readonly List<UnitCommon> lastUnits = new();
        private readonly List<BestTarget> quad1Enemies = new();
        private readonly List<BestTarget> quad2Enemies = new();
        private readonly List<BestTarget> quad3Enemies = new();
        private readonly List<BestTarget> quad4Enemies = new();
        public Dictionary<IDamageable, TargetingHealthUI> targetingHealthUis = new();

        public Dictionary<IDamageable, List<TargetingObject>> targetingObjects = new();

        public List<PlayerUnit> allPlayerUnits => UnitCommonController.instance.allPlayerUnits;
        public List<AIUnit> allAiUnits => UnitCommonController.instance.allAiUnits;
        public List<UnitCommon> allUnits => UnitCommonController.instance.allUnits;
        public TargetingObject currentTargetingObject { get; private set; }
        private CameraController camControl => CameraController.instance;
        
        [Range(0.01f, 10f)] 
        public float overDamageableCount  = .3f;
        [Range(0.01f, 10f)] 
        public float rayWaitTime = .1f;
        public void OnEnable()
        {
            CreateCrosshairs();
            RegisterEvents();
        }

        public void OnDisable()
        {
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            EventSenderController.enterAttackView += EnterAttackView;
            EventSenderController.exitAttackView += ExitAttackView;
            EventSenderController.engageTargetingObject += EngageTargetingObject;
            EventSenderController.changeTargetingObject += ChangeTargetingObject;
            EventSenderController.disengageTargetingObject += DisengageTargetingUi;
            EventSenderController.initiateDamagableDeath += DamageableDeathInitiated;
        }
        
        private void UnRegisterEvents()
        {
            EventSenderController.enterAttackView -= EnterAttackView;
            EventSenderController.exitAttackView -= ExitAttackView;
            EventSenderController.engageTargetingObject -= EngageTargetingObject;
            EventSenderController.changeTargetingObject -= ChangeTargetingObject;
            EventSenderController.disengageTargetingObject -= DisengageTargetingUi;
            EventSenderController.initiateDamagableDeath -= DamageableDeathInitiated;
        }

        private void EnterAttackView()
        {
        }

        private void ExitAttackView()
        {
        }

        private void CreateCrosshairs()
        {
            for (var i = 0; i < 10; i++)
            {
                var newCrosshair = Instantiate(crosshairPrefab, transform, true);
                newCrosshair.DisableCrosshair();
                crosshairs.Add(newCrosshair);
            }
        }

        public TargetingHealthUI CreateHealthUi(IDamageable targetingObjectDamageable)
        {
            var newHealthUi = Instantiate(targetHealthPrefab, transform, true);
            newHealthUi.DisableTargetingUI();
            newHealthUi.Create();
            targetingHealthUis.Add(targetingObjectDamageable, newHealthUi);
            targetingObjectDamageable.ConfigureHealthUI(newHealthUi);
            return newHealthUi;
        }

        public void RegisterTargetObjects(IDamageable damageable, TargetingObject targetingObject)
        {
            if (!targetingObjects.ContainsKey(damageable))
                targetingObjects.Add(damageable, new List<TargetingObject> {targetingObject});
            else
                targetingObjects[damageable].Add(targetingObject);
        }
        
        
        private void DamageableDeathInitiated(AttackAction attackAction)
        {
            if (targetingHealthUis.ContainsKey(attackAction.damageable))
                targetingHealthUis.Remove(attackAction.damageable);
            if (targetingObjects.ContainsKey(attackAction.damageable))
                targetingObjects.Remove(attackAction.damageable);
        }

        public UnitCommon ClosestTarget(UnitCommon selectedUnit)
        {
            var bestPossibleTarget = allAiUnits.FirstOrDefault();
            var unitCenter = selectedUnit.GetPosition() + selectedUnit.motor.Capsule.center;
            var maxDist = selectedUnit.AttackRange();
            var bestDist = maxDist;
            foreach (var enemy in allAiUnits)
            {
                var enemyCenter = enemy.GetPosition() + enemy.motor.Capsule.center;
                var dir = enemyCenter - unitCenter;
                var enemyRay = new Ray(unitCenter, dir);
                RaycastHit hit;
                if (Physics.Raycast(enemyRay, out hit, maxDist, LayerMaskHelper.unitLayerMask))
                {
                    var hitObject = hit.collider.gameObject;
                    if (hitObject.layer != (int) LayersEnum.Unit) continue;

                    var hitUnit = hitObject.GetComponent<UnitCommon>();
                    if (hitUnit == null) continue;

                    if (hitUnit == selectedUnit) continue;
                    var enemyDist = Vector3.Distance(unitCenter,
                        enemy.GetPosition() + enemy.motor.Capsule.center);
                    if (enemyDist < bestDist)
                    {
                        bestPossibleTarget = enemy;
                        bestDist = enemyDist;
                    }
                }
            }

            return bestPossibleTarget;
        }

        public UnitCommon AttackViewTarget(UnitCommon selectedUnit)
        {
            UnitsToQuads(selectedUnit.motor.transform.position);
            List<BestTarget> allPossibleTargets = new();
            allPossibleTargets.AddRange(quad1Enemies);
            allPossibleTargets.AddRange(quad2Enemies);
            allPossibleTargets.AddRange(quad3Enemies);
            allPossibleTargets.AddRange(quad4Enemies);
            var selectedUnitNav = selectedUnit.GetCurrentNavigable().GetNavIndex();
            var bestPossibleList = allPossibleTargets.OrderBy(x => Math.Abs(90 - x.bearing));
            var bestTarget = bestPossibleList.FirstOrDefault();
            foreach (var target in bestPossibleList)
            {
                var potentialPathCount = NavPathJobs.instance.FindPath(selectedUnitNav,
                    target.unit.GetCurrentNavigable().GetNavIndex()).Count;
                if (potentialPathCount <= CalculatePathAddition(0))
                {
                    bestTarget = target;
                    break;
                }
            }
            lastUnits.Add(bestTarget.unit);
            return bestTarget.unit;
        }
        
        public UnitCommon ResetOnDeath(UnitCommon selectedUnit, IDamageable deadDamageable)
        { 
            UnitsToQuads(selectedUnit.motor.transform.position);
            List<BestTarget> allPossibleTargets = new();
            allPossibleTargets.AddRange(quad1Enemies);
            allPossibleTargets.AddRange(quad2Enemies);
            allPossibleTargets.AddRange(quad3Enemies);
            allPossibleTargets.AddRange(quad4Enemies);
            var deadBestTarget = allPossibleTargets.Find(x => x.unit == deadDamageable.GetUnit());
            allPossibleTargets.Remove(deadBestTarget);
            var selectedUnitNav = selectedUnit.GetCurrentNavigable().GetNavIndex();
            var bestPossibleList = allPossibleTargets.OrderBy(x => Math.Abs(90 - x.bearing));
            var bestTarget = bestPossibleList.FirstOrDefault();
            foreach (var target in bestPossibleList)
            {
                var potentialPathCount = NavPathJobs.instance.FindPath(selectedUnitNav,
                    target.unit.GetCurrentNavigable().GetNavIndex()).Count;
                if (potentialPathCount <= CalculatePathAddition(0))
                {
                    bestTarget = target;
                    break;
                }
            }
            lastUnits.Add(bestTarget.unit);
            return bestTarget.unit;
            
        }
        private void UnitsToQuads(Vector3 startPos)
        {
            //Get camera forward  
            var camTransform = CameraController.instance.currentCinemachine.transform;
            //Clear all Quadrants before we add enemy's from current rotation
            quad1Enemies.Clear();
            quad2Enemies.Clear();
            quad3Enemies.Clear();
            quad4Enemies.Clear();
            foreach (var unit in allAiUnits)
            {
                var distance = Vector3.Distance(startPos, unit.motor.transform.position);
                var offset = unit.transform.position - startPos;
                offset = camTransform.transform.InverseTransformDirection(offset);
                var bearing = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;

                //Blue
                if (0 <= bearing && bearing <= 90)
                    quad1Enemies.Add(new BestTarget(unit, bearing, distance));

                //Green
                if (90 < bearing && bearing <= 180)
                    quad2Enemies.Add(new BestTarget(unit, bearing, distance));

                //Red
                if (-90 > bearing && bearing >= -180)
                    quad3Enemies.Add(new BestTarget(unit, bearing, distance));

                //Yellow
                if (-90 <= bearing && bearing < 0)
                    quad4Enemies.Add(new BestTarget(unit, bearing, distance));
            }
        }

        public UnitCommon ChangeAttackViewTarget(UnitCommon selectedUnit, UnitCommon currentTarget,
            CompassDirections direction)
        {
            var bestTarget = currentTarget;
            BestTarget potentialBestTarget;
            var selectUnitNav = selectedUnit.GetCurrentNavigable().GetNavIndex();
            var currentTargetNav = currentTarget.GetCurrentNavigable().GetNavIndex();
            var currentTargetPathCount = NavPathJobs.instance.FindPath(selectUnitNav, currentTargetNav).Count;

            if (IsOppositeDirection(direction))
            {
                var currentIndex = lastUnits.IndexOf(lastUnits.Last());
                if (currentIndex >= 1)
                {
                    var potentialUnit = lastUnits[currentIndex - 1];
                    var potentialPathCount = NavPathJobs.instance.FindPath(selectUnitNav,
                        potentialUnit.GetCurrentNavigable().GetNavIndex()).Count;
                    if (potentialPathCount <= currentTargetPathCount + CalculatePathAddition(currentTargetPathCount))
                        if (currentTarget != potentialUnit)
                        {
                            lastUnits.Add(potentialUnit);
                            if (lastUnits.Count >= 3) lastUnits.RemoveAt(0);
                            lastDirection = direction;
                            Debug.Log("Return last unit");
                            return potentialUnit;
                        }
                }
            }

            UnitsToQuads(selectedUnit.motor.transform.position);
            var newUnitList = new List<BestTarget>();
            var currentBearing = CurrentTargetBearing(currentTarget);
            List<BestTarget> orderedUnitList = new();
            List<BestTarget> unEffectedList = new();

            switch (direction)
            {
                case CompassDirections.None:
                    break;
                case CompassDirections.N:
                    newUnitList.AddRange(quad1Enemies);
                    newUnitList.AddRange(quad2Enemies);
                    potentialBestTarget = BestUnitNorth(newUnitList, currentTarget);
                    bestTarget = potentialBestTarget == null ? currentTarget : potentialBestTarget.unit;
                    break;
                case CompassDirections.S:
                    newUnitList.AddRange(quad1Enemies);
                    newUnitList.AddRange(quad2Enemies);
                    newUnitList.AddRange(quad3Enemies);
                    newUnitList.AddRange(quad4Enemies);
                    potentialBestTarget = BestUnitSouth(selectUnitNav, newUnitList, currentTarget);
                    bestTarget = potentialBestTarget == null ? currentTarget : potentialBestTarget.unit;
                    break;
                case CompassDirections.E:
                    newUnitList.AddRange(quad1Enemies);
                    newUnitList.AddRange(quad4Enemies);
                    unEffectedList.AddRange(quad1Enemies);
                    unEffectedList.AddRange(quad4Enemies);
                    orderedUnitList = quad1Enemies.OrderByDescending(x => x.bearing).ToList();
                    potentialBestTarget = BestUnitEast(selectUnitNav, orderedUnitList, currentTargetPathCount,
                        currentBearing,
                        0,
                        orderedUnitList.Count);
                    bestTarget = potentialBestTarget == null
                        ? unEffectedList.OrderBy(x => x.distance).FirstOrDefault()?.unit
                        : potentialBestTarget.unit;
                    break;
                case CompassDirections.W:
                    var orderedQuad2 = quad2Enemies.OrderBy(x => x.bearing).ToList();
                    var orderedQuad3 = quad3Enemies.OrderBy(x => x.bearing).ToList();
                    newUnitList.AddRange(orderedQuad2);
                    newUnitList.AddRange(orderedQuad3);
                    unEffectedList.AddRange(quad2Enemies);
                    unEffectedList.AddRange(quad3Enemies);
                    unEffectedList = newUnitList;
                    potentialBestTarget = BestUnitWest(selectUnitNav, newUnitList, currentTargetPathCount,
                        currentBearing, 0,
                        newUnitList.Count);
                    bestTarget = potentialBestTarget == null
                        ? unEffectedList.OrderBy(x => x.distance).FirstOrDefault()?.unit
                        : potentialBestTarget.unit;
                    break;
                case CompassDirections.NE:
                    newUnitList.AddRange(quad1Enemies);
                    break;
                case CompassDirections.NW:
                    newUnitList.AddRange(quad2Enemies);
                    break;
                case CompassDirections.SE:
                    newUnitList.AddRange(quad4Enemies);
                    break;
                case CompassDirections.SW:
                    newUnitList.AddRange(quad3Enemies);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var key in newUnitList.Select(target => target.unit))
                DebugController.instance.DrawDebug(DrawDebugTypes.Cylinder, Color.cyan, 5, false,
                    key.motor.transform.position + key.motor.Capsule.center, 1, null);

            lastDirection = direction;
            if (currentTarget != bestTarget)
            {
                lastUnits.Add(bestTarget);
                if (lastUnits.Count >= 3) lastUnits.RemoveAt(0);
            }

            return bestTarget;
        }

        private BestTarget BestUnitNorth(List<BestTarget> newUnitList, UnitCommon currentTarget)
        {
            var currentBestTarget = newUnitList.FirstOrDefault(x => x.unit == currentTarget);
            newUnitList.Remove(currentBestTarget);
            return (
                from target in newUnitList
                    .Where(x => currentBestTarget != null && x.bearing > 45 && x.bearing < 135 &&
                                x.distance > currentBestTarget.distance).OrderBy(x => x.distance)
                select target
            ).FirstOrDefault();
        }

        private BestTarget BestUnitSouth(int selectUnitNav, List<BestTarget> newUnitList, UnitCommon currentTarget)
        {
            var currentBestTarget = newUnitList.FirstOrDefault(x => x.unit == currentTarget);
            newUnitList.Remove(currentBestTarget);
            BestTarget potentialBestTarget = null;
            float bestDistance = 0;
            foreach (var target in newUnitList.Where(x => x.bearing > 45 && x.bearing < 135)
                         .OrderBy(x => Math.Abs(90 - x.bearing)))
                if (Math.Abs(90 - target.bearing) <= 20 && currentBestTarget.distance - target.distance > bestDistance)
                {
                    potentialBestTarget = target;
                    bestDistance = currentBestTarget.distance - target.distance;
                }

            if (potentialBestTarget == null)
                potentialBestTarget = newUnitList.Where(x => x.bearing < 0)
                    .OrderBy(x => Math.Abs(90 + x.bearing)).FirstOrDefault();

            return potentialBestTarget;
        }

        private BestTarget BestUnitWest(int selectedUnitNav, List<BestTarget> unitList, int pathCount, float bearing,
            int loopCount, int totalCount)
        {
            if (loopCount == totalCount || unitList.Count == 0) return null;

            var potentialUnit = unitList.FirstOrDefault(x => x.bearing > bearing)
                                ?? unitList.FirstOrDefault(x => x.bearing >= -180 && x.bearing <= -90);

            if (potentialUnit != null)
            {
                var potentialPathCount = NavPathJobs.instance.FindPath(selectedUnitNav,
                    potentialUnit.unit.GetCurrentNavigable().GetNavIndex()).Count;
                if (potentialPathCount <= pathCount + CalculatePathAddition(pathCount)) return potentialUnit;

                unitList.Remove(potentialUnit);
                loopCount++;
                return BestUnitWest(selectedUnitNav, unitList, pathCount,
                    potentialUnit.bearing, loopCount, totalCount);
            }

            return potentialUnit;
        }

        private BestTarget BestUnitEast(int selectedUnitNav, List<BestTarget> unitList, int pathCount,
            float bearing, int loopCount, int totalCount)
        {
            if (loopCount == totalCount || unitList.Count == 0) return null;

            var potentialUnit = unitList.FirstOrDefault(x => x.bearing < bearing && x.bearing > -90);
            if (potentialUnit != null)
            {
                var potentialPathCount = NavPathJobs.instance.FindPath(selectedUnitNav,
                    potentialUnit.unit.GetCurrentNavigable().GetNavIndex()).Count;
                if (potentialPathCount <= pathCount + CalculatePathAddition(pathCount)) return potentialUnit;

                unitList.Remove(potentialUnit);
                loopCount++;
                return BestUnitEast(selectedUnitNav, unitList, pathCount,
                    potentialUnit.bearing, loopCount, totalCount);
            }

            return potentialUnit;
        }

        private int CalculatePathAddition(int pathCount)
        {
            return pathCount switch
            {
                <= 0 => 8,
                <= 4 => 4,
                <= 6 => 2,
                _ => 1
            };
        }


        public static UnitCommon GetClosestEnemy(Vector3 startPos, List<UnitCommon> units)
        {
            UnitCommon closestUnit = null;
            var minDist = Mathf.Infinity;
            foreach (var t in units)
            {
                var dist = Vector3.Distance(t.transform.position, startPos);
                if (!(dist < minDist)) continue;
                closestUnit = t;
                minDist = dist;
            }

            return closestUnit;
        }

        private float CurrentTargetBearing(UnitCommon currentTarget)
        {
            float currentBearing = 90;
            foreach (var target in quad1Enemies.Where(target => target.unit == currentTarget))
                currentBearing = target.bearing;

            foreach (var target in quad2Enemies.Where(target => target.unit == currentTarget))
                currentBearing = target.bearing;

            foreach (var target in quad3Enemies.Where(target => target.unit == currentTarget))
                currentBearing = target.bearing;

            foreach (var target in quad4Enemies.Where(target => target.unit == currentTarget))
                currentBearing = target.bearing;

            return currentBearing;
        }

        private bool IsOppositeDirection(CompassDirections direction)
        {
            return (lastDirection == CompassDirections.E && direction == CompassDirections.W)
                   || (lastDirection == CompassDirections.W && direction == CompassDirections.E)
                   || (lastDirection == CompassDirections.S && direction == CompassDirections.N)
                   || (lastDirection == CompassDirections.N && direction == CompassDirections.S);
        }

        private void EngageTargetingObject(TargetingObject targetingObject)
        {
            activeTargetingHealth 
                = targetingHealthUis.ContainsKey(targetingObject.damageable) 
                ? targetingHealthUis[targetingObject.damageable] 
                : CreateHealthUi(targetingObject.damageable);

            activeTargetingHealth.EnableTargetingUi();
            activeTargetingHealth.ActivateTargetingHealthbar(targetingObject);
            activeCrosshair = crosshairs.FirstOrDefault(x => x.isVisible == false);
            currentTargetingObject = targetingObject;
            if (activeCrosshair != null) activeCrosshair.EnableCrosshair(targetingObject);
        }

        private void ChangeTargetingObject(TargetingObject targetingObject)
        {
            currentTargetingObject = targetingObject;
            activeTargetingHealth.ChangeTargetingHealthbar(targetingObject);
            activeCrosshair.ChangeTargetingObject(targetingObject);
        }

        private void DisengageTargetingUi()
        {
            activeTargetingHealth.DeactivateHealthBar();
            activeTargetingHealth.DisableTargetingUI();
            activeTargetingHealth = null;
            ClearTargeting();
        }

        public void ClearTargeting()
        {
            activeTargetingHealth = null;
            currentTargetingObject = null;
            activeCrosshair = null;
            foreach (var crosshair in crosshairs) crosshair.DisableCrosshair();
        }

        public List<TargetingObject> UnitTargetingObjects(IDamageable damageable)
        {
            return targetingObjects.ContainsKey(damageable) ? targetingObjects[damageable] : null;
        }

        public TargetingObject NextTargetingObject(TargetingObject targetingObject, CompassDirections direction)
        {
            var nextTargetingObject = targetingObject;
            var targetingObjectList = targetingObjects[targetingObject.damageable];
            nextTargetingObject = targetingObject.targetObjectType switch
            {
                //Left is east, right is west
                TargetingObjectType.Head => direction switch
                {
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftFoot),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    CompassDirections.S => targetingObjectList.Find(
                        x => x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    _ => nextTargetingObject
                },
                TargetingObjectType.Chest => direction switch
                {
                    CompassDirections.N =>
                        targetingObjectList.Find(x => x.targetObjectType == TargetingObjectType.Head),
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightLeg),
                    _ => nextTargetingObject
                },
                TargetingObjectType.RightArm => direction switch
                {
                    CompassDirections.N =>
                        targetingObjectList.Find(x => x.targetObjectType == TargetingObjectType.Head),
                    CompassDirections.S => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    CompassDirections.E => targetingObjectList.Find(
                        x => x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightLeg),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    _ => nextTargetingObject
                },
                TargetingObjectType.RightHand => direction switch
                {
                    CompassDirections.N => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightLeg),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    _ => nextTargetingObject
                },
                TargetingObjectType.LeftArm => direction switch
                {
                    CompassDirections.N =>
                        targetingObjectList.Find(x => x.targetObjectType == TargetingObjectType.Head),
                    CompassDirections.S => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    CompassDirections.W => targetingObjectList.Find(
                        x => x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    _ => nextTargetingObject
                },
                TargetingObjectType.LeftHand => direction switch
                {
                    CompassDirections.N => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftFoot),
                    _ => nextTargetingObject
                },
                TargetingObjectType.RightLeg => direction switch
                {
                    CompassDirections.N => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftFoot),
                    CompassDirections.S => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    _ => nextTargetingObject
                },
                TargetingObjectType.RightFoot => direction switch
                {
                    CompassDirections.N => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightLeg),
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightHand),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftFoot),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftFoot),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.S =>
                        targetingObjectList.Find(x => x.targetObjectType == TargetingObjectType.Head),
                    _ => nextTargetingObject
                },
                TargetingObjectType.LeftLeg => direction switch
                {
                    CompassDirections.N => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.Chest),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightLeg),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    CompassDirections.S => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftFoot),
                    _ => nextTargetingObject
                },
                TargetingObjectType.LeftFoot => direction switch
                {
                    CompassDirections.N => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftLeg),
                    CompassDirections.NE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftHand),
                    CompassDirections.NW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightLeg),
                    CompassDirections.E => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    CompassDirections.W => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightFoot),
                    CompassDirections.SE => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.RightArm),
                    CompassDirections.SW => targetingObjectList.Find(x =>
                        x.targetObjectType == TargetingObjectType.LeftArm),
                    CompassDirections.S =>
                        targetingObjectList.Find(x => x.targetObjectType == TargetingObjectType.Head),
                    _ => nextTargetingObject
                },
                _ => throw new ArgumentOutOfRangeException()
            };
            Debug.Log("From: " + currentTargetingObject.targetObjectType + "\n Dir: " + direction + "\n To: " +
                      nextTargetingObject.targetObjectType);
            return nextTargetingObject;
        }

     
    }
}