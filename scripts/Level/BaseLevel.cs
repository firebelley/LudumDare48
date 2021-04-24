using System.Linq;
using Game.GameObject;
using Game.State;
using Game.Util;
using Godot;
using GodotUtilities;

namespace Game.Level
{
    public class BaseLevel : Node
    {
        private const string INPUT_CLICK = "click";

        [Node("Entities/TileMap")]
        public TileMap TileMap { get; private set; }
        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        private Node2D entities;

        [Export]
        private int startingResources = 5;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.BoardStore.Reset();
            GameState.CreateEffect<BoardActions.TileClicked>(this, nameof(TileClickedEffect));
            GameState.CreateEffect<BoardActions.TowerPlaced>(this, nameof(TowerPlacedEffect));
            GameState.BoardStore.DispatchAction(new BoardActions.SetBaseResourceCount { Count = startingResources });
        }

        public override void _Ready()
        {
            var ui = resourcePreloader.InstanceSceneOrNull<LevelUI>();
            AddChild(ui);
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsActionPressed(INPUT_CLICK))
            {
                GetTree().SetInputAsHandled();
                GameState.BoardStore.DispatchAction(new BoardActions.TileClicked { Tile = GetHoveredTile() });
            }
        }

        public override void _Process(float delta)
        {
            SetPlacementValidity();
        }

        public bool CanDeleteBuilding(Building building)
        {
            return building switch
            {
                Tower => !TowerHasBuildingsInRadius(building as Tower),
                Barracks => !BarracksIsProtecting(building as Barracks),
                _ => true,
            };
        }

        public bool ShouldGoblinCampBeDisabled(GoblinCamp goblinCamp, Barracks excludeBarracks = null)
        {
            var barracks = entities.GetNodesOfType<Barracks>().Where(x => x != excludeBarracks);
            return barracks.Any(x => GridUtils.IsPointWithinRadius(x.TilePosition, goblinCamp.TilePos, x.Radius));
        }

        private bool TowerHasBuildingsInRadius(Tower tower)
        {
            var otherTowersInRadius = entities.GetNodesOfType<Tower>().Where(x => x != tower && GridUtils.IsPointWithinRadius(tower.TilePosition, x.TilePosition, tower.Radius));
            var otherTowersConnected = otherTowersInRadius.All(x => otherTowersInRadius.Any(y => x != y && GridUtils.IsPointWithinRadius(x.TilePosition, y.TilePosition, x.Radius)));
            if (otherTowersInRadius.Count() > 1 && !otherTowersConnected) return true;

            var buildingsInRadius = entities.GetNodesOfType<Building>()
                .Where(x => !(x is Tower) && GridUtils.IsPointWithinRadius(tower.TilePosition, x.TilePosition, tower.Radius));
            if (!buildingsInRadius.Any()) return false;

            var buildingsAreCoveredByOtherTower = buildingsInRadius.All(x => otherTowersInRadius.Any(y => GridUtils.IsPointWithinRadius(y.TilePosition, x.TilePosition, y.Radius)));
            return !buildingsAreCoveredByOtherTower;
        }

        private bool BarracksIsProtecting(Barracks barracks)
        {
            var buildings = entities.GetNodesOfType<Building>().Where(x => x is not Barracks);
            var otherBarracks = entities.GetNodesOfType<Barracks>().Where(x => x != barracks);
            var disabledGoblinCamps = entities.GetNodesOfType<GoblinCamp>().Where(x => x.Disabled);

            if (!disabledGoblinCamps.Any(x => buildings.Any(y => GridUtils.IsPointWithinRadius(x.TilePos, y.TilePosition, GoblinCamp.RADIUS))))
            {
                return false;
            }

            var goblinCampsInRadius = entities.GetNodesOfType<GoblinCamp>().Where(x => GridUtils.IsPointWithinRadius(x.TilePos, barracks.TilePosition, barracks.Radius));
            return goblinCampsInRadius.Any() && !goblinCampsInRadius.All(camp => otherBarracks.Any(other => GridUtils.IsPointWithinRadius(other.TilePosition, camp.TilePos, other.Radius)));
        }

        private Vector2 GetHoveredTile()
        {
            return TileMap.WorldToMap(TileMap.GetGlobalMousePosition());
        }

        private void SetPlacementValidity()
        {
            var selectedBuilding = GameState.BoardStore.State.SelectedBuildingInfo;
            if (selectedBuilding == null) return;

            var hoveredTile = GameState.BoardStore.State.HoveredTile;
            var valid = true;

            if (TileMap.GetCellv(hoveredTile) != 0)
            {
                valid = false;
            }

            if (selectedBuilding.Cost > GameState.BoardStore.State.ResourcesAvailable)
            {
                valid = false;
            }

            var towers = entities.GetNodesOfType<Tower>();
            var hasProximityToTower = towers.Any(x => GridUtils.IsPointWithinRadius(x.TilePosition, hoveredTile, x.Radius));

            if (!hasProximityToTower)
            {
                valid = false;
            }

            var isWithinGoblinCamp = entities.GetNodesOfType<GoblinCamp>().Any(x =>
                !x.Disabled && GridUtils.IsPointWithinRadius(x.TilePos, hoveredTile, GoblinCamp.RADIUS)
            );
            if (isWithinGoblinCamp && selectedBuilding.Type != typeof(Barracks))
            {
                valid = false;
            }

            var isOccupied = entities.GetNodesOfType<Building>().Any(x => x.TilePosition == hoveredTile);
            if (isOccupied)
            {
                valid = false;
            }

            GameState.BoardStore.DispatchAction(new BoardActions.SetPlacementValid { Valid = valid });
        }

        private void HandleTileClick(Vector2 tile)
        {
            if (TileMap.GetCellv(tile) == -1) return;
            if (GameState.BoardStore.State.SelectedBuildingInfo == null) return;
            if (!GameState.BoardStore.State.TilePlacementValid) return;

            var building = GD.Load<PackedScene>(GameState.BoardStore.State.SelectedBuildingInfo.ScenePath).InstanceOrNull<Building>();
            building.GlobalPosition = tile * TileMap.CellSize;
            entities.AddChild(building);
            GameState.BoardStore.DispatchAction(new BoardActions.BuildingDeselected());
        }

        private void TileClickedEffect(BoardActions.TileClicked tileClicked)
        {
            HandleTileClick(tileClicked.Tile);
        }

        private void TowerPlacedEffect(BoardActions.TowerPlaced towerPlaced)
        {
            var goal = entities.GetFirstNodeOfType<Goal>();
            if (goal != null && GridUtils.IsPointWithinRadius(towerPlaced.Tower.TilePosition, goal.TilePosition, towerPlaced.Tower.Radius))
            {
                GD.Print("complete");
            }
        }
    }
}
