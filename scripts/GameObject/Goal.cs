using Game.Level;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Goal : Node2D
    {
        public Vector2 TilePosition { get; private set; }

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            TilePosition = this.GetAncestor<BaseLevel>().TileMap.WorldToMap(GlobalPosition);
        }
    }
}
