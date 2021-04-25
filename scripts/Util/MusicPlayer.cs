using Godot;
using GodotUtilities;

namespace Game
{
    public class MusicPlayer : Node
    {
        [Node]
        private AudioStreamPlayer audioStreamPlayer;
        [Node]
        private Timer timer;

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            timer.Connect("timeout", this, nameof(OnTimeout));
            audioStreamPlayer.Connect("finished", this, nameof(OnFinished));
        }

        private void OnFinished()
        {
            timer.Start();
        }

        private void OnTimeout()
        {
            audioStreamPlayer.Play();
        }
    }
}
