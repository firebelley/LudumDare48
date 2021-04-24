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
                    foreach (var tile in resourcesHarvested.Tiles)
                    {
                        if (!state.ConsumedResourceTiles.ContainsKey(tile))
                        {
                            state.ConsumedResourceTiles[tile] = 0;
                        }
                        state.ConsumedResourceTiles[tile]++;
                    }
                    break;
                case BoardActions.ResourcesSpent resourcesSpent:
                    state.ResourcesSpent += resourcesSpent.Count;
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
    }
}
