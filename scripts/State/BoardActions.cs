using System.Collections.Generic;
using Game.Data;
using Game.GameObject;
using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public static class BoardActions
    {
        public class BuildingSelected : BaseAction
        {
            public SelectedBuildingInfo SelectedBuildingInfo;
        }

        public class BuildingDeselected : BaseAction { }

        public class SetBaseResourceCount : BaseAction
        {
            public int Count;
        }

        public class ResourcesHarvested : BaseAction
        {
            public List<Vector2> Tiles = new();
        }

        public class ResourcesSpent : BaseAction
        {
            public int Count;
        }

        public class TileClicked : BaseAction
        {
            public Vector2 Tile;
        }

        public class TileHovered : BaseAction
        {
            public Vector2 Tile;
        }

        public class SetPlacementValid : BaseAction
        {
            public bool Valid;
        }

        public class BarracksPlaced : BaseAction
        {
            public Barracks Barracks;
        }

        public class BarracksRemoved : BaseAction
        {
            public Vector2 TilePosition;
        }
    }
}
