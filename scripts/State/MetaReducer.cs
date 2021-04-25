using GodotUtilities.StateManagement;

namespace Game.State
{
    public class MetaReducer : Reducer<MetaState>
    {
        public override void Reduce(MetaState state, BaseAction action)
        {
            switch (action)
            {
                case MetaActions.LevelChanged levelChanged:
                    state.CurrentLevelIndex = levelChanged.ToLevelIndex;
                    break;
                case MetaActions.SetMaxLevels maxLevels:
                    state.MaxLevels = maxLevels.MaxLevels;
                    break;
            }
        }
    }
}
