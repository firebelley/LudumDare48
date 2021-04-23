using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public static class PlayerActions
    {
        public class PositionUpdated : BaseAction
        {
            public Vector2 Position;
        }
    }
}
