using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class LevelComplete : CanvasLayer
    {
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Button")]
        private Button nextLevelButton;
        [Node("MarginContainer/PanelContainer")]
        private PanelContainer panelContainer;

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            nextLevelButton.Connect("pressed", this, nameof(OnNextLevelPressed));
        }

        public override void _Process(float delta)
        {
            panelContainer.RectPivotOffset = panelContainer.RectSize / 2f;
        }

        private void OnNextLevelPressed()
        {
            GameState.MetaStore.DispatchAction(new MetaActions.LevelChanged { ToLevelIndex = GameState.MetaStore.State.CurrentLevelIndex + 1 });
        }
    }
}
