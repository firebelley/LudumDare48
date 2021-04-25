using Game.State;
using Game.UI;
using Godot;
using GodotUtilities;

namespace Game
{
    public class LevelUI : CanvasLayer
    {
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/ResourcesLabel")]
        private Label resourcesLabel;
        [Node("MarginContainer/ButtonPanel/MarginContainer/VBoxContainer/RestartButton")]
        private Button restartButton;
        [Node("MarginContainer/ButtonPanel/MarginContainer/VBoxContainer/OptionsButton")]
        private Button optionsButton;
        [Node("MarginContainer/ButtonPanel/MarginContainer/VBoxContainer/HelpButton")]
        private Button helpButton;
        [Node]
        private ResourcePreloader resourcePreloader;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.CreateEffect<BoardActions.ResourcesHarvested>(this, nameof(ResourcesHarvestedEffect));
            GameState.CreateEffect<BoardActions.ResourcesUnharvested>(this, nameof(ResourcesUnharvestedEffect));
            GameState.CreateEffect<BoardActions.ResourcesSpent>(this, nameof(ResourcesSpentEffect));
            GameState.CreateEffect<BoardActions.ResourcesRecovered>(this, nameof(ResourcesRecoveredEffect));
        }

        public override void _Ready()
        {
            UpdateResourceCount();
            restartButton.Connect("pressed", this, nameof(OnRestartPressed));
            helpButton.Connect("pressed", this, nameof(OnHelpButtonPressed));
            optionsButton.Connect("pressed", this, nameof(OnOptionsPressed));
        }

        private void UpdateResourceCount()
        {
            resourcesLabel.Text = GameState.BoardStore.State.ResourcesAvailable.ToString();
        }

        private void ResourcesHarvestedEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesUnharvestedEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesSpentEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesRecoveredEffect(object _)
        {
            UpdateResourceCount();
        }

        private void OnRestartPressed()
        {
            GameState.MetaStore.DispatchAction(new MetaActions.LevelChanged { ToLevelIndex = GameState.MetaStore.State.CurrentLevelIndex });
        }

        private void OnHelpButtonPressed()
        {
            var tutorial = resourcePreloader.InstanceSceneOrNull<TutorialSlideshow>();
            AddChild(tutorial);
        }

        private void OnOptionsPressed()
        {
            var menu = GD.Load<PackedScene>("res://scenes/UI/OptionsMenu.tscn").Instance<OptionsMenu>();
            AddChild(menu);
        }
    }
}
