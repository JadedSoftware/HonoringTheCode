
/// <summary>
/// When an object can perform an Action
/// </summary>
public interface IActionable
{
    IActionable GetActionable();
    public ActionTurnType actionType { get; set; }
    public int currentActionPoints { get; set; }
    void ConsumeActionPoints(int amount);
    void AddActionPoints(int amount);
    void RefreshActionPoints();
    int GetPointsRequired(ActionTypes actionTypes);
    void PerformAction(ActionTypes type, int cost);
    void RegisterActionable(IActionable actionable);
    void UnRegisterActionable(IActionable actionable);
    void OnBeginTurn(SelectableTypes type);
    void OnEndTurn();
    public SelectableTypes GetUnitType();
}

public enum ActionTypes
{
    Move,
    Attack,
    Reload,
    Guard,
    Special
}

public enum ActionTurnType
{
    Player,
    Enemy,
    Environment
}