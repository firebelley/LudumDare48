using System.Linq;
using Game.GameObject;
using Game.Level;
using Game.State;
using Game.Util;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class GridContext : TileMap
    {
        private const int REALTIME_INDEX = 2;
        private const int INVALID_INDEX = 1;
        private const int VALID_INDEX = 0;

        [Node]
        private TileMap realtimeMap;

        private BaseLevel baseLevel;

        public override void _EnterTree()
        {
            this.WireNodes();
            baseLevel = this.GetAncestor<BaseLevel>();
            GameState.CreateEffect<BoardActions.BuildingSelected>(this, nameof(BuildingSelectedEffect));
            GameState.CreateEffect<BoardActions.BuildingDeselected>(this, nameof(BuildingDeselectedEffect));
            GameState.CreateDeferredEffect<BoardActions.TileHovered>(this, nameof(TileHoveredEffect));
        }

        public override void _Process(float delta)
        {
            SetHoverPlacementValid();
            if (GameState.BoardStore.State.SelectedBuildingInfo == null)
            {
                realtimeMap.Clear();
            }
        }

        private void UpdateTileMap()
        {
            var boundaryEntites = baseLevel.Entities.GetNodesOfType<Building>().Where(x => x is Tower || x is MainBuilding);
            foreach (var building in boundaryEntites)
            {
                GridUtils.ForEachTileInRadius(building.TilePosition, building.Radius, (tile) =>
                {
                    if (baseLevel.TileMap.GetCellv(tile) == 0)
                    {
                        SetCellv(tile, IsTilePlacementValid(tile) ? VALID_INDEX : INVALID_INDEX);
                    }
                });
            }
        }

        private void SetHoverPlacementValid()
        {
            var selectedBuilding = GameState.BoardStore.State.SelectedBuildingInfo;
            if (selectedBuilding == null) return;

            var valid = true;
            if (selectedBuilding.Cost > GameState.BoardStore.State.ResourcesAvailable)
            {
                valid = false;
            }

            GameState.BoardStore.DispatchAction(new BoardActions.SetPlacementValid { Valid = valid && IsTilePlacementValid(GameState.BoardStore.State.HoveredTile) });
        }

        private bool IsTilePlacementValid(Vector2 tile)
        {
            var valid = true;
            if (baseLevel.TileMap.GetCellv(tile) != 0)
            {
                return false;
            }

            var towers = baseLevel.Entities.GetNodesOfType<Tower>();
            var hasProximityToTower = towers.Any(x => GridUtils.IsPointWithinRadius(x.TilePosition, tile, x.Radius));
            var main = baseLevel.Entities.GetNodesOfType<MainBuilding>();
            var hasProximityToMain = main.Any(x => GridUtils.IsPointWithinRadius(x.TilePosition, tile, x.Radius));
            if (!hasProximityToMain && !hasProximityToTower)
            {
                return false;
            }

            var isWithinGoblinCamp = baseLevel.Entities.GetNodesOfType<GoblinCamp>().Any(x =>
                !x.Disabled && GridUtils.IsPointWithinRadius(x.TilePos, tile, GoblinCamp.RADIUS)
            );
            var isOnGoblinCamp = baseLevel.Entities.GetNodesOfType<GoblinCamp>().Any(x => x.TilePos == tile);
            var selectedBuilding = GameState.BoardStore.State.SelectedBuildingInfo;
            if (isOnGoblinCamp || (isWithinGoblinCamp && selectedBuilding.Type != typeof(Barracks)))
            {
                valid = false;
            }

            var isOccupied = baseLevel.Entities.GetNodesOfType<Building>().Any(x => x.TilePosition == tile);
            if (isOccupied)
            {
                valid = false;
            }
            return valid;
        }

        private void TileHoveredEffect(object _)
        {
            realtimeMap.Clear();
            var selectedBuilding = GameState.BoardStore.State.SelectedBuildingInfo;
            if (selectedBuilding != null && GameState.BoardStore.State.TilePlacementValid)
            {
                GridUtils.ForEachTileInRadius(GameState.BoardStore.State.HoveredTile, selectedBuilding.Radius, (vector) =>
                {
                    var baseTile = baseLevel.TileMap.GetCellv(vector);
                    var highlightCell = GetCellv(vector) == -1 && selectedBuilding.Type == typeof(Village) ? baseTile == 1 : baseTile == 0;
                    if (GetCellv(vector) == -1 && highlightCell)
                    {
                        realtimeMap.SetCellv(vector, REALTIME_INDEX);
                    }
                });
            }
        }

        private void BuildingSelectedEffect(object _)
        {
            UpdateTileMap();
        }

        private void BuildingDeselectedEffect(object _)
        {
            Clear();
        }
    }
}
