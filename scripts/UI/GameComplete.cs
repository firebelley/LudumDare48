using Game.Level;
using Godot;
using GodotUtilities;

namespace Game
{
    public class GameComplete : Node
    {
        [Node("CanvasLayer/MarginContainer/PanelContainer/MarginContainer/VBoxContainer/BackButton")]
        private Button backButton;
        [Node]
        private BaseLevel baseLevel;

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            backButton.Connect("pressed", this, nameof(OnBackPressed));
            baseLevel.GetFirstNodeOfType<LevelUI>()?.QueueFree();
        }

        private void OnBackPressed()
        {
            TransitionManager.TransitionTo("res://scenes/Main.tscn");
        }
    }
}
