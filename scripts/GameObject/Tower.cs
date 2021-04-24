using Game.State;

namespace Game.GameObject
{
    public class Tower : Building
    {
        protected override void Placed()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.TowerPlaced { Tower = this });
        }
    }
}
