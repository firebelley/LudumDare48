using Game.State;
using Game.Util;
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
            GameState.CreateEffect<BoardActions.BarracksPlaced>(this, nameof(BarracksPlacedEffect));
        }

        public override void _Ready()
        {
            TilePos = this.GetAncestor<BaseLevel>()?.TileMap.WorldToMap(GlobalPosition) ?? Vector2.Zero;
        }

        private void BarracksPlacedEffect(BoardActions.BarracksPlaced barracksPlaced)
        {
            if (GridUtils.IsPointWithinRadius(TilePos, barracksPlaced.Barracks.TilePosition, barracksPlaced.Barracks.Radius))
            {
                Disabled = true;
                Modulate = Colors.Transparent;
            }
        }
    }
}
