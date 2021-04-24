using Game.State;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class BuildingGhost : Node
    {
        [Node]
        private Sprite ghost;

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
        }

        private void BuildingSelectedEffect(object _)
        {
            ghost.Texture = GD.Load<Texture>("res://assets/placeholder-building.png");
        }

        private void BuildingDeselectedEffect(object _)
        {
            ghost.Texture = null;
        }
    }
}
