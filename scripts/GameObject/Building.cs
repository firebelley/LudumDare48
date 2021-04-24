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
        public int Radius { get; private set; } = 1;
        [Export]
        public int ResourceCost { get; private set; } = 2;
        [Export]
        public Texture GhostTexture { get; private set; }

        public override void _EnterTree()
        {
            tileSize = this.GetAncestor<BaseLevel>().TileMap.CellSize;
        }

        public override void _Ready()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesSpent { Count = ResourceCost });
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

            for (int x = (int)tilePosition.x - Radius; x <= tilePosition.x + Radius; x++)
            {
                for (int y = (int)tilePosition.y - Radius; y <= tilePosition.y + Radius; y++)
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
