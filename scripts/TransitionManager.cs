using Game.UI;
using Godot;
using GodotUtilities;

namespace Game
{
    public class TransitionManager : Node
    {
        [Node]
        private ResourcePreloader resourcePreloader;

        private static TransitionManager instance;

        public override void _EnterTree()
        {
            this.WireNodes();
            instance = this;
        }

        public static async void TransitionTo(string scenePath)
        {
            var transition = instance.resourcePreloader.InstanceSceneOrNull<Transition>();
            instance.AddChild(transition);
            await instance.ToSignal(transition, nameof(Transition.TransitionHit));
            instance.GetTree().ChangeScene(scenePath);
        }
    }
}
