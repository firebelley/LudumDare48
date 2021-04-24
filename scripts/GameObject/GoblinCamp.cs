using Game.Level;
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
            GameState.CreateEffect<BoardActions.BarracksRemoved>(this, nameof(BarracksRemovedEffect));
        }

        public override void _Ready()
        {
            TilePos = this.GetAncestor<BaseLevel>()?.TileMap.WorldToMap(GlobalPosition) ?? Vector2.Zero;
        }

        private void BarracksPlacedEffect(BoardActions.BarracksPlaced barracksPlaced)
        {
            Disabled = this.GetAncestor<BaseLevel>().ShouldGoblinCampBeDisabled(this);
            Modulate = Disabled ? Colors.Transparent : Colors.White;
        }

        private void BarracksRemovedEffect(BoardActions.BarracksRemoved barracksRemoved)
        {
            Disabled = this.GetAncestor<BaseLevel>().ShouldGoblinCampBeDisabled(this, barracksRemoved.Barracks);
            Modulate = Disabled ? Colors.Transparent : Colors.White;
        }
    }
}
