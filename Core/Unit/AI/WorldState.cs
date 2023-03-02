using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core.AI
{
    public class WorldState
    {
        public readonly List<UnitInfo> playerInfo;
        public List<INavigable> movableNavs;

        public WorldState(List<UnitInfo> playerInfo, List<INavigable> movableNavs)
        {
            this.playerInfo = playerInfo;
            this.movableNavs = movableNavs;
        }
    }

    public struct UnitInfo
    {
        public UnitCommon unit;
        public SelectableTypes unitType;
        public Stack<int> navPath;

        public UnitInfo(UnitCommon unit, SelectableTypes unitType, Stack<int> navPath)
        {
            this.unit = unit;
            this.unitType = unitType;
            this.navPath = navPath;
        }
    }
}