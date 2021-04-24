using Game.State;
using GodotUtilities;

namespace Game.GameObject
{
    public class Village : Building
    {
        protected override void Placed()
        {
            base.Placed();
            var resources = GetNearbyResourceCount();
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesGained { Count = resources });
        }

        private int GetNearbyResourceCount()
        {
            var tileMap = this.GetAncestor<BaseLevel>().TileMap;
            var sum = 0;

            if (tileMap == null) return sum;

            for (int x = (int)TilePosition.x - Radius; x <= TilePosition.x + Radius; x++)
            {
                for (int y = (int)TilePosition.y - Radius; y <= TilePosition.y + Radius; y++)
                {
                    if (tileMap.GetCell(x, y) == 1)
                    {
                        sum++;
                    }
                }
            }

            return sum;
        }
    }
}
