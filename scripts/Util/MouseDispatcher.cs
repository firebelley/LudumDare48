using Game.State;
using Godot;
using GodotUtilities;
using Game.Level;

namespace Game.Util
{
    public class MouseDispatcher : Node
    {
        private Vector2 hoveredTile = Vector2.One * -1;

        private TileMap tileMap;

        public override void _EnterTree()
        {
            this.WireNodes();
            tileMap = this.GetAncestor<BaseLevel>().TileMap;
        }

        public override void _Process(float delta)
        {
            var mouseTilePos = tileMap.WorldToMap(tileMap.GetGlobalMousePosition());
            if (mouseTilePos != hoveredTile)
            {
                hoveredTile = mouseTilePos;
                GameState.BoardStore.DispatchAction(new BoardActions.TileHovered { Tile = hoveredTile });
            }
        }
    }
}
