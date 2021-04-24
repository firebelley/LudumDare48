using Game.State;

namespace Game.GameObject
{
    public class Barracks : Building
    {
        protected override void Placed()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.BarracksPlaced { Barracks = this });
        }

        protected override void Destroyed()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.BarracksRemoved { Barracks = this });
        }
    }
}
