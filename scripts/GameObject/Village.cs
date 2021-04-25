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
        [Node]
        private AudioStreamPlayer sawAudioStreamPlayer;

        protected override void Placed()
        {
            base.Placed();
            CallDeferred(nameof(CollectResources));
            // var resourceTiles = GetNearbyResourceTiles();
            // GameState.BoardStore.DispatchAction(new BoardActions.ResourcesHarvested { Tiles = resourceTiles });
        }

        protected override void Destroyed()
        {
            base.Destroyed();
            var resourceTiles = GetNearbyResourceTiles();
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesUnharvested { Tiles = resourceTiles });
        }

        private void CollectResources()
        {
            var resourceTiles = GetNearbyResourceTiles();
            if (PlayerPlaced && resourceTiles.Count > 0)
            {
                sawAudioStreamPlayer.PlayWithPitchRange(.8f, 1.1f);
            }
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesHarvested { Tiles = resourceTiles });
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
