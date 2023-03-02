using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// When an object can move across the NavGrid
/// </summary>
public interface IMovable
{
    public KinematicCharacterMotor motor { get; set; }
    public void RegisterMovable(IMovable movable, bool isActive);
    public void UnRegisterMovable(IMovable movable, bool isActive);
    public Vector3 GetPosition();
    public int GetMoveDistance();
    public int PredictMoveDistance(int actionPoints);
    public INavigable GetCurrentNavigable();
    public SelectableTypes GetUnitType();
    public ISelectable GetSelectable();
    public void SetMovementVelocity(Vector3 pos);
    public void UpdateNavigable(INavigable navPoint);
    void RotateTowardsCamera();
    UnitCommon GetUnit();
}