using Game.Data;
using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public class BoardState : IState
    {
        public SelectedBuildingInfo SelectedBuildingInfo;
        public int ResourceCount;
        public Vector2 HoveredTile;
        public bool TilePlacementValid;
    }
}
