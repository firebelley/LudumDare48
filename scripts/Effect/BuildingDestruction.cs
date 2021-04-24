using Godot;
using GodotUtilities;

namespace Game.Effect
{
    public class BuildingDestruction : Node2D
    {
        [Node("Control/Sprite")]
        private Sprite sprite;

        public Texture Texture { set => sprite.Texture = value; }

        public override void _EnterTree()
        {
            this.WireNodes();
        }
    }
}
