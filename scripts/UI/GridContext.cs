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
        private const int INVALID_INDEX = 1;
        private const int VALID_INDEX = 0;

        private BaseLevel baseLevel;

        public override void _EnterTree()
        {
            this.WireNodes();
            baseLevel = this.GetAncestor<BaseLevel>();
            GameState.CreateEffect<BoardActions.BuildingSelected>(this, nameof(BuildingSelectedEffect));
            GameState.CreateEffect<BoardActions.BuildingDeselected>(this, nameof(BuildingDeselectedEffect));
        }

        public override void _Process(float delta)
        {
            SetHoverPlacementValid();
        }

        private void UpdateTileMap()
        {
            foreach (var tower in baseLevel.Entities.GetNodesOfType<Tower>())
            {
                GridUtils.ForEachTileInRadius(tower.TilePosition, tower.Radius, (tile) =>
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

            if (!hasProximityToTower)
            {
                return false;
            }

            var isWithinGoblinCamp = baseLevel.Entities.GetNodesOfType<GoblinCamp>().Any(x =>
                !x.Disabled && GridUtils.IsPointWithinRadius(x.TilePos, tile, GoblinCamp.RADIUS)
            );
            var selectedBuilding = GameState.BoardStore.State.SelectedBuildingInfo;
            if (isWithinGoblinCamp && selectedBuilding.Type != typeof(Barracks))
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
