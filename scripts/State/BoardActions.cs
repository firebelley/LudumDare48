using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public static class BoardActions
    {
        public class BuildingSelected : BaseAction
        {

        }

        public class BuildingDeselected : BaseAction
        {

        }

        public class ResourcesGained : BaseAction
        {
            public int Count;
        }

        public class ResourcesSpent : BaseAction
        {
            public int Count;
        }

        public class TileClicked : BaseAction
        {
            public Vector2 Tile;
        }
    }
}
