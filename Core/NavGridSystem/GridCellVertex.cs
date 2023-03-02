using UnityEngine;

public enum VertEdgePosition
{
    Right = 0,
    TopRight = 1,
    TopLeft = 2,
    Left = 3,
    BottomLeft = 4,
    BottomRight = 5
}

public enum VertCoverType
{
    None,
    Half,
    Full
}
/// <summary>
/// Corner of a hex grid
/// </summary>

public class GridCellVertex : MonoBehaviour
{
    public int index;
    public NavCell navCell;
    public Vector3 vertPos;
    public VertEdgePosition vertEdgePos;
    public bool isOffLedge;
    public VertCoverType cover;
}