using Game.Data;
using Game.GameObject;
using Game.State;
using Godot;
using GodotUtilities;

namespace Game
{
    public class BuildingButton : VBoxContainer
    {
        [Node]
        private TextureRect textureRect;
        [Node]
        private Button button;

        [Export]
        private PackedScene buildingScene;

        public override void _EnterTree()
        {
            this.WireNodes();

        }

        public override void _Ready()
        {
            Configure();
            button.Connect("pressed", this, nameof(OnButtonPressed));
        }

        private void Configure()
        {
            var building = buildingScene.InstanceOrNull<Building>();
            textureRect.Texture = building.GhostTexture;
            button.Text = $"Cost: {building.ResourceCost}";
            building.QueueFree();
        }

        private void OnButtonPressed()
        {
            var building = buildingScene.InstanceOrNull<Building>();
            GameState.BoardStore.DispatchAction(new BoardActions.BuildingSelected { SelectedBuildingInfo = SelectedBuildingInfo.FromBuilding(building) });
            building.QueueFree();
        }
    }
}
