using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class Transition : CanvasLayer
    {
        [Signal]
        public delegate void TransitionHit();

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        private void EmitTransition()
        {
            EmitSignal(nameof(TransitionHit));
        }
    }
}
