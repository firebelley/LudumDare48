using System.Collections.Generic;
using System.Linq;
using Game.Level;
using Game.State;
using Godot;
using GodotUtilities.Util;

namespace Game
{
    public class LevelManager : Node
    {
        private List<string> levelPaths = new();

        public override void _EnterTree()
        {
            GameState.CreateEffect<MetaActions.LevelChanged>(this, nameof(LevelChangedEffect));
        }

        public override void _Ready()
        {
            var levels = FileSystem.InstanceScenesInPath<BaseLevel>("res://scenes/Level/AllLevels/");
            levelPaths.AddRange(levels.Select(x => x.Filename));
            levels.ForEach(x => x.QueueFree());

            GameState.MetaStore.DispatchAction(new MetaActions.SetMaxLevels { MaxLevels = levelPaths.Count });
        }

        private void LevelChangedEffect(object _)
        {
            var levelIdx = GameState.MetaStore.State.CurrentLevelIndex;
            if (levelIdx >= GameState.MetaStore.State.MaxLevels)
            {
                // transition to game complete
            }
            else
            {
                TransitionManager.TransitionTo(levelPaths[levelIdx]);
            }
        }
    }
}
