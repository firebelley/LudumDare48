using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class LevelUI : CanvasLayer
    {
        [Node("MarginContainer/ResourcesLabel")]
        private Label resourcesLabel;

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
        }

        private void UpdateResourceCount()
        {
            resourcesLabel.Text = $"Resources: {GameState.BoardStore.State.ResourcesAvailable}";
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
    }
}
