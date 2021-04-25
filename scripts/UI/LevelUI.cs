using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class LevelUI : CanvasLayer
    {
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ResourcesLabel")]
        private Label resourcesLabel;
        [Node("MarginContainer/ButtonPanel/MarginContainer/VBoxContainer/RestartButton")]
        private Button restartButton;

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
    }
}
