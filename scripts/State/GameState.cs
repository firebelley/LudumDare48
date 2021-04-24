using GodotUtilities.StateManagement;

namespace Game.State
{
    public class GameState : StateManager
    {
        public static Store<PlayerState> PlayerStore { get; private set; } = new(new PlayerReducer());
        public static Store<BoardState> BoardStore { get; private set; } = new(new BoardReducer());
    }
}
