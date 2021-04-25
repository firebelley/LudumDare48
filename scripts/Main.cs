using Game.Level;
using Game.State;
using Game.UI;
using Godot;
using GodotUtilities;

namespace Game
{
    public class Main : Node
    {
        [Node("CanvasLayer/MarginContainer/PanelContainer/MarginContainer/VBoxContainer/PlayButton")]
        private Button playButton;
        [Node("CanvasLayer/MarginContainer/PanelContainer/MarginContainer/VBoxContainer/OptionsButton")]
        private Button optionsButton;
        [Node("CanvasLayer/MarginContainer/PanelContainer/MarginContainer/VBoxContainer/QuitButton")]
        private Button quitButton;
        [Node]
        private BaseLevel baseLevel;

        public override void _EnterTree()
        {
            this.WireNodes();
            VisualServer.SetDefaultClearColor(new Color(21f / 255f, 21f / 255f, 21f / 255f));
        }

        public override void _Ready()
        {
            playButton.Connect("pressed", this, nameof(OnPlayPressed));
            quitButton.Connect("pressed", this, nameof(OnQuitPressed));
            optionsButton.Connect("pressed", this, nameof(OnOptionsPressed));
            baseLevel.GetFirstNodeOfType<LevelUI>()?.QueueFree();
        }

        private void OnPlayPressed()
        {
            GameState.MetaStore.DispatchAction(new MetaActions.LevelChanged { ToLevelIndex = 0 });
        }

        private void OnQuitPressed()
        {
            GetTree().Quit();
        }

        private void OnOptionsPressed()
        {
            var menu = GD.Load<PackedScene>("res://scenes/UI/OptionsMenu.tscn").Instance<OptionsMenu>();
            AddChild(menu);
        }
    }
}
