using System.Collections.Generic;

namespace Core.Data
{
    /// <summary>
    /// An object for storing data for saving and loading
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public List<UnitCommon> persistedUnits;
        public List<NavigableObject> persistedNavigables;
        public SelectableTypes currentTurn;
        public Dictionary<string, int> unitPosition;
        public List<UnitData> unitDataList;
        public GameData()
        {
            persistedUnits = new ();
            unitDataList = new();
            persistedNavigables = new ();
            unitPosition = new();
        }
    }
}