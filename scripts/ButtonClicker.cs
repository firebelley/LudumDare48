using Game.Util;
using Godot;
using GodotUtilities;

namespace Game
{
    public class ButtonClicker : Node
    {
        [Node]
        private RandomAudioPlayer randomAudioPlayer;

        private static ButtonClicker instance;

        public override void _EnterTree()
        {
            this.WireNodes();
            instance = this;
        }

        public static void Click()
        {
            instance.randomAudioPlayer.Play();
        }
    }
}
