using Game.Level;
using Game.State;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class GoblinCamp : Node2D
    {
        public const int RADIUS = 2;

        public bool Disabled { get; private set; } = false;
        public Vector2 TilePos { get; private set; }

        [Node]
        private Particles2D particles2D;
        [Node]
        private Sprite sprite;

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

        private void UpdateState()
        {
            sprite.Modulate = Disabled ? new Color(100f / 255f, 100f / 255f, 100f / 255f) : Colors.White;
            particles2D.Emitting = Disabled;
        }

        private void BarracksPlacedEffect(BoardActions.BarracksPlaced barracksPlaced)
        {
            Disabled = this.GetAncestor<BaseLevel>().ShouldGoblinCampBeDisabled(this);
            UpdateState();
        }

        private void BarracksRemovedEffect(BoardActions.BarracksRemoved barracksRemoved)
        {
            Disabled = this.GetAncestor<BaseLevel>().ShouldGoblinCampBeDisabled(this, barracksRemoved.Barracks);
            UpdateState();
        }
    }
}
