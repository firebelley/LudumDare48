using Game.Level;
using Game.State;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class BuildingGhost : Node
    {
        [Node]
        private Sprite ghost;

        [Export]
        private Color validColor;
        [Export]
        private Color invalidColor;

        private TileMap tileMap;

        public override void _EnterTree()
        {
            this.WireNodes();
            tileMap = this.GetAncestor<BaseLevel>().TileMap;
            GameState.CreateEffect<BoardActions.BuildingSelected>(this, nameof(BuildingSelectedEffect));
            GameState.CreateEffect<BoardActions.BuildingDeselected>(this, nameof(BuildingDeselectedEffect));
        }

        public override void _Process(float delta)
        {
            if (tileMap == null) return;

            var tilePos = tileMap.WorldToMap(tileMap.GetGlobalMousePosition());
            ghost.Visible = tileMap.GetCellv(tilePos) > -1;

            ghost.GlobalPosition = tileMap.WorldToMap(tileMap.GetGlobalMousePosition()) * tileMap.CellSize;

            var valid = GameState.BoardStore.State.TilePlacementValid;
            ghost.Modulate = valid ? validColor : invalidColor;
        }

        private void BuildingSelectedEffect(object _)
        {
            ghost.Texture = GameState.BoardStore.State.SelectedBuildingInfo?.Texture;
        }

        private void BuildingDeselectedEffect(object _)
        {
            ghost.Texture = null;
        }
    }
}
