using Game.Util;
using Godot;
using GodotUtilities;

namespace Game.Effect
{
    public class BuildingDestruction : Node2D
    {
        [Node("Control/Sprite")]
        private Sprite sprite;
        [Node]
        private RandomAudioPlayer randomAudioPlayer;
        [Node]
        private RandomAudioPlayer initialAudioPlayer;

        public Texture Texture { set => sprite.Texture = value; }

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            randomAudioPlayer.PlayTimes(3);
            initialAudioPlayer.Play();
        }
    }
}
