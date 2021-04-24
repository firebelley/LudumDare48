using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class Building : Node2D
    {
        private Vector2 tilePosition;
        private Vector2 tileSize;

        [Export]
        protected int radius = 1;
        [Export]
        private int resourceCost = 2;

        public override void _EnterTree()
        {
            tileSize = this.GetAncestor<BaseLevel>().TileMap.CellSize;
        }

        public override void _Ready()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesSpent { Count = resourceCost });
        }

        public void SetTilePosition(Vector2 tilePos)
        {
            GlobalPosition = tilePos * tileSize;
            tilePosition = tilePos;
            var resources = GetNearbyResourceCount();
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesGained { Count = resources });
            GD.Print(GameState.BoardStore.State.ResourceCount);
        }

        private int GetNearbyResourceCount()
        {
            var tileMap = this.GetAncestor<BaseLevel>().TileMap;
            var sum = 0;

            if (tileMap == null) return sum;

            for (int x = (int)tilePosition.x - radius; x <= tilePosition.x + radius; x++)
            {
                for (int y = (int)tilePosition.y - radius; y <= tilePosition.y + radius; y++)
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
