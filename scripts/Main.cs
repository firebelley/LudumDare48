using Godot;
using GodotUtilities;

namespace Game
{
    public class Main : Node
    {
        public override void _EnterTree()
        {
            this.WireNodes();
            VisualServer.SetDefaultClearColor(new Color(21f / 255f, 21f / 255f, 21f / 255f));
        }
    }
}
