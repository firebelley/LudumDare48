using Game.Data;
using Game.State;
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

        [Export]
        private int startingResources = 5;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.BoardStore.Reset();
            GameState.CreateEffect<BoardActions.TileClicked>(this, nameof(TileClickedEffect));
            selectVillageButton.Connect("pressed", this, nameof(OnSelectVillagePressed));
            GameState.BoardStore.DispatchAction(new BoardActions.ResourcesGained { Count = startingResources });
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
            if (GameState.BoardStore.State.SelectedBuildingInfo == null) return;
            var hoveredTile = GameState.BoardStore.State.HoveredTile;
            var valid = true;
            if (TileMap.GetCellv(hoveredTile) != 0)
            {
                valid = false;
            }

            if (valid != GameState.BoardStore.State.TilePlacementValid)
            {
                GameState.BoardStore.DispatchAction(new BoardActions.SetPlacementValid { Valid = valid });
            }
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

        private void OnSelectVillagePressed()
        {
            var building = resourcePreloader.InstanceSceneOrNull<Building>();
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
    }
}
