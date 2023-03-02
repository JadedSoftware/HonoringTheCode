
/// <summary>
/// instance of a object that represents either the high point of a terrain, or the ledge of a cliff
/// </summary>
public class NavLedge : NavPoint
{
    public LedgeType ledgeType;
}

public enum LedgeType
{
    TerrainLedge,
    ObstacleLedge
}