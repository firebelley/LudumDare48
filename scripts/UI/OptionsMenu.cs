using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class OptionsMenu : CanvasLayer
    {
        private const string BUS_SFX = "sfx";
        private const string BUS_MUSIC = "music";

        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Done")]
        private Button doneButton;

        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/ScreenMode/HBoxContainer/WindowButton")]
        private Button windowButton;

        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/Music/HBoxContainer/DecrementButton")]
        private Button musicDecrementButton;
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/Music/HBoxContainer/IncrementButton")]
        private Button musicIncrementButton;
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/Music/HBoxContainer/Label")]
        private Label musicValueLabel;

        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/SFX/HBoxContainer/DecrementButton")]
        private Button sfxDecrementButton;
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/SFX/HBoxContainer/IncrementButton")]
        private Button sfxIncrementButton;
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/SFX/HBoxContainer/Label")]
        private Label sfxValueLabel;

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            doneButton.Connect("pressed", this, nameof(OnDonePressed));
            musicDecrementButton.Connect("pressed", this, nameof(ChangeVolume), new Godot.Collections.Array { BUS_MUSIC, -1 });
            musicIncrementButton.Connect("pressed", this, nameof(ChangeVolume), new Godot.Collections.Array { BUS_MUSIC, 1 });
            sfxDecrementButton.Connect("pressed", this, nameof(ChangeVolume), new Godot.Collections.Array { BUS_SFX, -1 });
            sfxIncrementButton.Connect("pressed", this, nameof(ChangeVolume), new Godot.Collections.Array { BUS_SFX, 1 });
            windowButton.Connect("pressed", this, nameof(OnWindowButtonPressed));
            UpdateDisplay();
        }

        private void OnDonePressed()
        {
            QueueFree();
        }

        private void UpdateDisplay()
        {
            windowButton.Text = OS.WindowFullscreen ? "Fullscreen" : "Windowed";
            sfxValueLabel.Text = $"{GetBusVolume(BUS_SFX)}";
            musicValueLabel.Text = $"{GetBusVolume(BUS_MUSIC)}";
        }

        private void ChangeVolume(string bus, int change)
        {
            var vol = Mathf.Clamp(GetBusVolume(bus) + change, 0, 10);
            var volDb = GD.Linear2Db(vol / 10f);
            var busIdx = AudioServer.GetBusIndex(bus);
            AudioServer.SetBusVolumeDb(busIdx, volDb);
            UpdateDisplay();
        }

        private int GetBusVolume(string bus)
        {
            var busIdx = AudioServer.GetBusIndex(bus);
            return (int)(GD.Db2Linear(AudioServer.GetBusVolumeDb(busIdx)) * 10);
        }

        private void OnWindowButtonPressed()
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
            UpdateDisplay();
        }
    }
}
