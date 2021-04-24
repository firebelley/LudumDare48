using System.Collections.Generic;
using Game.State;
using Godot;
using GodotUtilities;
using Game.Level;
using Game.Util;

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

        protected override void Destroyed()
        {
            base.Destroyed();
            var resourceTiles = GetNearbyResourceTiles();
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesUnharvested { Tiles = resourceTiles });
        }

        private List<Vector2> GetNearbyResourceTiles()
        {
            var result = new List<Vector2>();
            var tileMap = this.GetAncestor<BaseLevel>().TileMap;

            if (tileMap == null) return result;

            GridUtils.ForEachTileInRadius(TilePosition, Radius, (vector) =>
            {
                if (tileMap.GetCellv(vector) == 1)
                {
                    result.Add(vector);
                }
            });
            return result;
        }
    }
}
