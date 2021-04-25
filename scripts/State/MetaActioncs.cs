using GodotUtilities.StateManagement;

namespace Game.State
{
    public static class MetaActions
    {
        public class SetMaxLevels : BaseAction
        {
            public int MaxLevels;
        }

        public class LevelChanged : BaseAction
        {
            public int ToLevelIndex;
        }
    }
}
