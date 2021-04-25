using GodotUtilities.StateManagement;

namespace Game.State
{
    public class GameState : StateManager
    {
        public static Store<BoardState> BoardStore { get; private set; } = new(new BoardReducer());
        public static Store<MetaState> MetaStore { get; private set; } = new(new MetaReducer());
    }
}
