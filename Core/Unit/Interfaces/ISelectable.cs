using UnityEngine;

/// <summary>
/// When an object is selectable
/// </summary>
public interface ISelectable
{
    ISelectable GetSelectable();

    void RegisterSelectable(ISelectable selectable);
    void UnRegisterSelectable(ISelectable selectable);

    Vector3 GetPosition();
    INavigable SelectableCurrentNavigable();
    void OnUnitSelected(ISelectable unit);
    void OnUnitDeselect();

    SelectableTypes GetSelectableType();
    UnitCommon GetUnit();
    IWarrior GetWarrior();
    IMovable GetMovable();
    Transform AttackCamTransform();
}