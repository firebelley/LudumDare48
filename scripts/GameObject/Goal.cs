using Game.Level;
using Game.State;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Goal : Node2D
    {
        public Vector2 TilePosition { get; private set; }

        [Node]
        private Particles2D victoryParticles;
        [Node]
        private Particles2D victoryParticlesExplosive;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.CreateEffect<BoardActions.Complete>(this, nameof(CompleteEffect));
        }

        public override void _Ready()
        {
            TilePosition = this.GetAncestor<BaseLevel>().TileMap.WorldToMap(GlobalPosition);
        }

        private void CompleteEffect(object _)
        {
            victoryParticles.Emitting = true;
            victoryParticlesExplosive.Emitting = true;
        }
    }
}
