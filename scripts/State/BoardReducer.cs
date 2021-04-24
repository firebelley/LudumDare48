using System.Collections.Generic;
using Godot;
using GodotUtilities.StateManagement;

namespace Game.State
{
    public class BoardReducer : Reducer<BoardState>
    {
        public override void Reduce(BoardState state, BaseAction action)
        {
            switch (action)
            {
                case BoardActions.BuildingSelected buildingSelected:
                    state.SelectedBuildingInfo = buildingSelected.SelectedBuildingInfo;
                    break;
                case BoardActions.BuildingDeselected _:
                    state.SelectedBuildingInfo = null;
                    break;
                case BoardActions.SetBaseResourceCount baseResourceCount:
                    state.BaseResourceCount = baseResourceCount.Count;
                    break;
                case BoardActions.ResourcesHarvested resourcesHarvested:
                    ChangeResourceHarvestStatus(state, resourcesHarvested.Tiles, 1);
                    break;
                case BoardActions.ResourcesUnharvested resourcesUnharvested:
                    ChangeResourceHarvestStatus(state, resourcesUnharvested.Tiles, -1);
                    break;
                case BoardActions.ResourcesSpent resourcesSpent:
                    state.ResourcesSpent += resourcesSpent.Count;
                    break;
                case BoardActions.ResourcesRecovered resourceRecovered:
                    state.ResourcesSpent -= resourceRecovered.Count;
                    break;
                case BoardActions.TileHovered tileHovered:
                    state.HoveredTile = tileHovered.Tile;
                    break;
                case BoardActions.TileClicked tileClicked:
                    state.HoveredTile = tileClicked.Tile;
                    break;
                case BoardActions.SetPlacementValid placementValid:
                    state.TilePlacementValid = placementValid.Valid;
                    break;
            }
        }

        private void ChangeResourceHarvestStatus(BoardState state, List<Vector2> resourceTiles, int change)
        {
            foreach (var tile in resourceTiles)
            {
                if (!state.ConsumedResourceTiles.ContainsKey(tile))
                {
                    state.ConsumedResourceTiles[tile] = 0;
                }

                state.ConsumedResourceTiles[tile] += change;
                if (state.ConsumedResourceTiles[tile] <= 0)
                {
                    state.ConsumedResourceTiles.Remove(tile);
                }
            }
        }
    }
}
