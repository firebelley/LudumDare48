using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class SoundButton : Button
    {
        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            Connect("pressed", this, nameof(OnSelfPressed));
        }

        private void OnSelfPressed()
        {
            ButtonClicker.Click();
        }
    }
}
