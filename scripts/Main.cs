using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class Main : Node
    {
        [Node("MarginContainer/VBoxContainer/PlayButton")]
        private Button playButton;

        public override void _EnterTree()
        {
            this.WireNodes();
            VisualServer.SetDefaultClearColor(new Color(21f / 255f, 21f / 255f, 21f / 255f));
        }

        public override void _Ready()
        {
            playButton.Connect("pressed", this, nameof(OnPlayPressed));
        }

        private void OnPlayPressed()
        {
            GameState.MetaStore.DispatchAction(new MetaActions.LevelChanged { ToLevelIndex = 0 });
        }
    }
}
