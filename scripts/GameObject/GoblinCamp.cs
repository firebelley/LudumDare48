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

        [Export]
        private Texture destroyedTexture;

        [Node]
        private Particles2D particles2D;
        [Node]
        private Sprite sprite;
        [Node]
        private RandomAudioPlayer randomAudioPlayer;

        private Texture originalTexture;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.CreateEffect<BoardActions.BarracksPlaced>(this, nameof(BarracksPlacedEffect));
            GameState.CreateEffect<BoardActions.BarracksRemoved>(this, nameof(BarracksRemovedEffect));
        }

        public override void _Ready()
        {
            originalTexture = sprite.Texture;
            TilePos = this.GetAncestor<BaseLevel>()?.TileMap.WorldToMap(GlobalPosition) ?? Vector2.Zero;
        }

        private void UpdateState(bool playAudio = false)
        {
            sprite.Modulate = Disabled ? new Color(100f / 255f, 100f / 255f, 100f / 255f) : Colors.White;
            particles2D.Emitting = Disabled;
            sprite.Texture = Disabled ? destroyedTexture : originalTexture;
            if (playAudio)
            {
                randomAudioPlayer.PlayTimes(2);
            }
        }

        private void BarracksPlacedEffect(BoardActions.BarracksPlaced barracksPlaced)
        {
            var wasDisabled = Disabled;
            Disabled = this.GetAncestor<BaseLevel>().ShouldGoblinCampBeDisabled(this);
            UpdateState(Disabled && wasDisabled != Disabled);
        }

        private void BarracksRemovedEffect(BoardActions.BarracksRemoved barracksRemoved)
        {
            var wasDisabled = Disabled;
            Disabled = this.GetAncestor<BaseLevel>().ShouldGoblinCampBeDisabled(this, barracksRemoved.Barracks);
            UpdateState(Disabled && wasDisabled != Disabled);
        }
    }
}
