using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class GoblinCamp : Node2D
    {
        public const int RADIUS = 2;

        public bool Disabled { get; private set; } = false;
        public Vector2 TilePos { get; private set; }

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            TilePos = this.GetAncestor<BaseLevel>()?.TileMap.WorldToMap(GlobalPosition) ?? Vector2.Zero;
        }
    }
}
