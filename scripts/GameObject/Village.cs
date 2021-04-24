using System.Collections.Generic;
using Game.State;
using Godot;
using GodotUtilities;
using Game.Level;

namespace Game.GameObject
{
    public class Village : Building
    {
        protected override void Placed()
        {
            base.Placed();
            var resourceTiles = GetNearbyResourceTiles();
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesHarvested { Tiles = resourceTiles });
        }

        private List<Vector2> GetNearbyResourceTiles()
        {
            var result = new List<Vector2>();
            var tileMap = this.GetAncestor<BaseLevel>().TileMap;

            if (tileMap == null) return result;

            for (int x = (int)TilePosition.x - Radius; x <= TilePosition.x + Radius; x++)
            {
                for (int y = (int)TilePosition.y - Radius; y <= TilePosition.y + Radius; y++)
                {
                    var vec = new Vector2(x, y);
                    if (tileMap.GetCell(x, y) == 1)
                    {
                        result.Add(vec);
                    }
                }
            }

            return result;
        }
    }
}
