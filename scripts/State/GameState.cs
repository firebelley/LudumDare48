using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public class GameState : StateManager
    {
        public Store<PlayerState> PlayerStore { get; private set; } = new(new PlayerReducer());
    }
}
