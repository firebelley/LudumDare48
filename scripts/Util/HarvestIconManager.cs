using System.Linq;
using Game.Level;
using Game.State;
using Game.UI;
using Godot;
using Godot.Collections;
using GodotUtilities;

namespace Game.Util
{
    public class HarvestIconManager : Node
    {
        private Dictionary<Vector2, HarvestIcon> icons = new();

        [Node]
        private ResourcePreloader resourcePreloader;

        private BaseLevel baseLevel;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.CreateEffect<BoardActions.ResourcesHarvested>(this, nameof(HarvestEffect));
            GameState.CreateEffect<BoardActions.ResourcesUnharvested>(this, nameof(HarvestEffect));
        }

        public override void _Ready()
        {
            baseLevel = Owner as BaseLevel;
        }

        private void HarvestEffect(object _)
        {
            var resourceState = GameState.BoardStore.State.ConsumedResourceTiles;
            var oldKeys = icons.Keys.Where(x => !resourceState.ContainsKey(x));
            var newKeys = resourceState.Keys.Where(x => !icons.ContainsKey(x));

            foreach (var key in oldKeys)
            {
                icons[key].QueueFree();
                icons.Remove(key);
            }

            foreach (var key in newKeys)
            {
                var icon = resourcePreloader.InstanceSceneOrNull<HarvestIcon>();
                baseLevel.Entities.AddChild(icon);
                icon.GlobalPosition = key * baseLevel.TileMap.CellSize;
                icons[key] = icon;
            }
        }
    }
}
