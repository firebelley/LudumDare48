using System.Collections.Generic;
using Game.Data;
using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public class BoardState : IState
    {
        public int ResourcesAvailable => BaseResourceCount + ConsumedResourceTiles.Keys.Count - ResourcesSpent;
        public Dictionary<Vector2, int> ConsumedResourceTiles = new();
        public int ResourcesSpent;
        public int BaseResourceCount;
        public SelectedBuildingInfo SelectedBuildingInfo;
        public Vector2 HoveredTile;
        public bool TilePlacementValid;
        public bool CanPlaceInGoblinCamp;
    }
}
