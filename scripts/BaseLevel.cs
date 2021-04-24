using System.Linq;
using Game.Data;
using Game.GameObject;
using Game.State;
using Game.Util;
using Godot;
using GodotUtilities;

namespace Game
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
            GameState.CreateEffect<BoardActions.ResourcesGained>(this, nameof(ResourcesGainedEffect));
            GameState.CreateEffect<BoardActions.ResourcesSpent>(this, nameof(ResourcesSpentEffect));
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesGained { Count = startingResources });
            selectVillageButton.Connect("pressed", this, nameof(OnSelectVillagePressed));
            selectTowerButton.Connect("pressed", this, nameof(OnSelectTowerPressed));
            selectBarracksButton.Connect("pressed", this, nameof(OnSelectBarracksPressed));
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

            if (selectedBuilding.Cost > GameState.BoardStore.State.ResourceCount)
            {
                valid = false;
            }

            var buildings = entities.GetNodesOfType<Building>();
            var hasProximityToBuilding = buildings.Any(x => GridUtils.IsPointWithinRadius(x.TilePosition, hoveredTile, x.Radius));

            if (!hasProximityToBuilding)
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

            GameState.BoardStore.DispatchAction(new BoardActions.SetPlacementValid { Valid = valid });
        }

        private void HandleTileClick(Vector2 tile)
        {
            if (TileMap.GetCellv(tile) == -1) return;
            if (GameState.BoardStore.State.SelectedBuildingInfo == null) return;
            if (!GameState.BoardStore.State.TilePlacementValid) return;

            var building = GD.Load<PackedScene>(GameState.BoardStore.State.SelectedBuildingInfo.ScenePath).InstanceOrNull<Building>();
            entities.AddChild(building);
            building.SetTilePosition(tile);
            GameState.BoardStore.DispatchAction(new BoardActions.BuildingDeselected());
        }

        private void UpdateResourceCount()
        {
            resourcesLabel.Text = $"Resources: {GameState.BoardStore.State.ResourceCount}";
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

        private void ResourcesGainedEffect(object _)
        {
            UpdateResourceCount();
        }

        private void ResourcesSpentEffect(object _)
        {
            UpdateResourceCount();
        }
    }
}
