using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class OptionsMenu : CanvasLayer
    {
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Done")]
        private Button doneButton;

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            doneButton.Connect("pressed", this, nameof(OnDonePressed));
        }

        private void OnDonePressed()
        {
            QueueFree();
        }
    }
}
