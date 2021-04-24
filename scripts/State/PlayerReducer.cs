using GodotUtilities.StateManagement;

namespace Game.State
{
    public class PlayerReducer : Reducer<PlayerState>
    {
        public override void Reduce(PlayerState state, BaseAction action)
        {
            switch (action)
            {
                case PlayerActions.PositionUpdated positionUpdated:
                    state.Position = positionUpdated.Position;
                    break;
            }
        }
    }
}
