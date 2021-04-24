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
        private const string INPUT_CLICK_ALTERNATE = "click_alternate";
        private const string INPUT_ESCAPE = "escape";

        [Node("Entities/TileMap")]
        public TileMap TileMap { get; private set; }
        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        public Node2D Entities { get; private set; }

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
            else if (evt.IsActionPressed(INPUT_CLICK_ALTERNATE) || evt.IsActionPressed(INPUT_ESCAPE))
            {
                if (GameState.BoardStore.State.SelectedBuildingInfo != null)
                {
                    GameState.BoardStore.DispatchAction(new BoardActions.BuildingDeselected());
                    GetTree().SetInputAsHandled();
                }
            }
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
            var barracks = Entities.GetNodesOfType<Barracks>().Where(x => x != excludeBarracks);
            return barracks.Any(x => GridUtils.IsPointWithinRadius(x.TilePosition, goblinCamp.TilePos, x.Radius));
        }

        private bool TowerHasBuildingsInRadius(Tower tower)
        {
            var otherTowersInRadius = Entities.GetNodesOfType<Tower>().Where(x => x != tower && GridUtils.IsPointWithinRadius(tower.TilePosition, x.TilePosition, tower.Radius));
            var otherTowersConnected = otherTowersInRadius.All(x => otherTowersInRadius.Any(y => x != y && GridUtils.IsPointWithinRadius(x.TilePosition, y.TilePosition, x.Radius)));
            if (otherTowersInRadius.Count() > 1 && !otherTowersConnected) return true;

            var buildingsInRadius = Entities.GetNodesOfType<Building>()
                .Where(x => !(x is Tower) && GridUtils.IsPointWithinRadius(tower.TilePosition, x.TilePosition, tower.Radius));
            if (!buildingsInRadius.Any()) return false;

            var buildingsAreCoveredByOtherTower = buildingsInRadius.All(x => otherTowersInRadius.Any(y => GridUtils.IsPointWithinRadius(y.TilePosition, x.TilePosition, y.Radius)));
            return !buildingsAreCoveredByOtherTower;
        }

        private bool BarracksIsProtecting(Barracks barracks)
        {
            var buildings = Entities.GetNodesOfType<Building>().Where(x => x is not Barracks);
            var otherBarracks = Entities.GetNodesOfType<Barracks>().Where(x => x != barracks);
            var disabledGoblinCamps = Entities.GetNodesOfType<GoblinCamp>().Where(x => x.Disabled);

            if (!disabledGoblinCamps.Any(x => buildings.Any(y => GridUtils.IsPointWithinRadius(x.TilePos, y.TilePosition, GoblinCamp.RADIUS))))
            {
                return false;
            }

            var goblinCampsInRadius = Entities.GetNodesOfType<GoblinCamp>().Where(x => GridUtils.IsPointWithinRadius(x.TilePos, barracks.TilePosition, barracks.Radius));
            return goblinCampsInRadius.Any() && !goblinCampsInRadius.All(camp => otherBarracks.Any(other => GridUtils.IsPointWithinRadius(other.TilePosition, camp.TilePos, other.Radius)));
        }

        private Vector2 GetHoveredTile()
        {
            return TileMap.WorldToMap(TileMap.GetGlobalMousePosition());
        }

        private void HandleTileClick(Vector2 tile)
        {
            if (TileMap.GetCellv(tile) == -1) return;
            if (GameState.BoardStore.State.SelectedBuildingInfo == null) return;
            if (!GameState.BoardStore.State.TilePlacementValid) return;

            var building = GD.Load<PackedScene>(GameState.BoardStore.State.SelectedBuildingInfo.ScenePath).InstanceOrNull<Building>();
            building.GlobalPosition = tile * TileMap.CellSize;
            Entities.AddChild(building);
            GameState.BoardStore.DispatchAction(new BoardActions.BuildingDeselected());
        }

        private void TileClickedEffect(BoardActions.TileClicked tileClicked)
        {
            HandleTileClick(tileClicked.Tile);
        }

        private void TowerPlacedEffect(BoardActions.TowerPlaced towerPlaced)
        {
            var goal = Entities.GetFirstNodeOfType<Goal>();
            if (goal != null && GridUtils.IsPointWithinRadius(towerPlaced.Tower.TilePosition, goal.TilePosition, towerPlaced.Tower.Radius))
            {
                GD.Print("complete");
            }
        }
    }
}
