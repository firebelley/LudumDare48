using System.Linq;
using Game.Data;
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

        [Node]
        public TileMap TileMap { get; private set; }
        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        private Node2D entities;
        [Node("CanvasLayer/Control/VBoxContainer/SelectVillageButton")]
        private Button selectVillageButton;
        [Node("CanvasLayer/Control/VBoxContainer/SelectTowerButton")]
        private Button selectTowerButton;
        [Node("CanvasLayer/Control/VBoxContainer/SelectBarracksButton")]
        private Button selectBarracksButton;
        [Node("CanvasLayer/Control/ResourcesLabel")]
        private Label resourcesLabel;

        [Export]
        private int startingResources = 5;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.BoardStore.Reset();
            GameState.CreateEffect<BoardActions.TileClicked>(this, nameof(TileClickedEffect));
            GameState.CreateEffect<BoardActions.ResourcesHarvested>(this, nameof(ResourcesHarvestedEffect));
            GameState.CreateEffect<BoardActions.ResourcesUnharvested>(this, nameof(ResourcesUnharvestedEffect));
            GameState.CreateEffect<BoardActions.ResourcesSpent>(this, nameof(ResourcesSpentEffect));
            GameState.CreateEffect<BoardActions.ResourcesRecovered>(this, nameof(ResourcesRecoveredEffect));
            GameState.BoardStore.DispatchAction(new BoardActions.SetBaseResourceCount { Count = startingResources });
            selectVillageButton.Connect("pressed", this, nameof(OnSelectVillagePressed));
            selectTowerButton.Connect("pressed", this, nameof(OnSelectTowerPressed));
            selectBarracksButton.Connect("pressed", this, nameof(OnSelectBarracksPressed));
        }

        public override void _Ready()
        {
            UpdateResourceCount();
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
            return building.GetType() != typeof(Tower) || !TowerHasBuildingsInRadius(building as Tower);
        }

        private bool TowerHasBuildingsInRadius(Tower tower)
        {
            var buildingsInRadius = entities.GetNodesOfType<Building>()
                .Where(x => !(x is Tower) && GridUtils.IsPointWithinRadius(tower.TilePosition, x.TilePosition, tower.Radius));

            if (!buildingsInRadius.Any()) return false;

            var otherTowers = entities.GetNodesOfType<Tower>().Where(x => x != tower);

            var buildingsAreCoveredByOtherTower = buildingsInRadius.All(x => otherTowers.Any(y => GridUtils.IsPointWithinRadius(y.TilePosition, x.TilePosition, y.Radius)));
            return !buildingsAreCoveredByOtherTower;
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

        private void UpdateResourceCount()
        {
            resourcesLabel.Text = $"Resources: {GameState.BoardStore.State.ResourcesAvailable}";
        }

        private void OnSelectVillagePressed()
        {
            var building = resourcePreloader.InstanceSceneOrNull<Village>();
            OnBuildingPressed(building);
        }

        private void OnSelectTowerPressed()
        {
            var building = resourcePreloader.InstanceSceneOrNull<Tower>();
            OnBuildingPressed(building);
        }

        private void OnSelectBarracksPressed()
        {
            var building = resourcePreloader.InstanceSceneOrNull<Barracks>();
            OnBuildingPressed(building);
        }

        private void OnBuildingPressed(Building building)
        {
            GameState.BoardStore.DispatchAction(new BoardActions.BuildingSelected
            {
                SelectedBuildingInfo = SelectedBuildingInfo.FromBuilding(building)
            });
            building.QueueFree();
        }

        private void TileClickedEffect(BoardActions.TileClicked tileClicked)
        {
            HandleTileClick(tileClicked.Tile);
        }

        private void ResourcesHarvestedEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesUnharvestedEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesSpentEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesRecoveredEffect(object _)
        {
            UpdateResourceCount();
        }
    }
}