using Game.Level;
using Game.State;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Building : Node2D
    {
        public Vector2 TilePosition { get; private set; }

        [Export]
        public int Radius { get; private set; } = 1;
        [Export]
        public int ResourceCost { get; private set; } = 2;
        [Export]
        public Texture GhostTexture { get; private set; }

        protected TileMap tileMap;

        public override void _EnterTree()
        {
            tileMap = this.GetAncestor<BaseLevel>().TileMap;
        }

        public override void _Ready()
        {
            if (Owner == null)
            {

                GameState.BoardStore.DispatchAction(new BoardActions.ResourcesSpent { Count = ResourceCost });
            }

            SetTilePositionFromGlobalPosition();
        }

        public void SetTilePositionFromGlobalPosition()
        {
            TilePosition = tileMap.WorldToMap(GlobalPosition);
            Placed();
        }

        public void SetTilePosition(Vector2 tilePos)
        {
            GlobalPosition = tilePos * tileMap.CellSize;
            SetTilePositionFromGlobalPosition();
        }

        protected virtual void Placed() { }
    }
}
