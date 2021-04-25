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

        public void EmitTransition()
        {
            EmitSignal(nameof(TransitionHit));
        }
    }
}
