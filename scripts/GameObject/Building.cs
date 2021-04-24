using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class Building : Node2D
    {
        public Vector2 TilePosition { get; private set; }

        [Export]
        public int Radius { get; private set; } = 1;
        [Export]
        public int ResourceCost { get; private set; } = 2;
        [Export]
        public Texture GhostTexture { get; private set; }

        private TileMap tileMap;

        public override void _EnterTree()
        {
            tileMap = this.GetAncestor<BaseLevel>().TileMap;
        }

        public override void _Ready()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesSpent { Count = ResourceCost });
            SetTilePositionFromGlobalPosition();
        }

        public void SetTilePositionFromGlobalPosition()
        {
            TilePosition = tileMap.WorldToMap(GlobalPosition);
            var resources = GetNearbyResourceCount();
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesGained { Count = resources });
            GD.Print(GameState.BoardStore.State.ResourceCount);
        }

        public void SetTilePosition(Vector2 tilePos)
        {
            GlobalPosition = tilePos * tileMap.CellSize;
            SetTilePositionFromGlobalPosition();
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
