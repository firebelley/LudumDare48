using Game.Level;
using Game.State;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Building : Node2D
    {
        private const string INPUT_CLICK_ALTERNATE = "click_alternate";

        public Vector2 TilePosition { get; private set; }

        [Node]
        protected Area2D area2D;

        [Export]
        public int Radius { get; private set; } = 1;
        [Export]
        public int ResourceCost { get; private set; } = 2;
        [Export]
        public Texture GhostTexture { get; private set; }
        [Export]
        public bool Deletable { get; private set; } = true;

        protected TileMap tileMap;

        public override void _EnterTree()
        {
            this.WireNodes();
            tileMap = this.GetAncestor<BaseLevel>().TileMap;
        }

        public override void _Ready()
        {
            if (Owner == null)
            {
                GameState.BoardStore.DispatchAction(new BoardActions.ResourcesSpent { Count = ResourceCost });
            }

            SetTilePositionFromGlobalPosition();

            area2D.Connect("input_event", this, nameof(OnAreaInputEvent));
        }

        public void SetTilePositionFromGlobalPosition()
        {
            TilePosition = tileMap.WorldToMap(GlobalPosition);
            Placed();
        }

        protected virtual void Placed() { }
        protected virtual void Destroyed() { }

        private void OnAreaInputEvent(object _, InputEvent inputEvent, object __)
        {
            if (inputEvent.IsActionPressed(INPUT_CLICK_ALTERNATE) && Deletable)
            {
                var canDelete = this.GetAncestor<BaseLevel>()?.CanDeleteBuilding(this) ?? false;
                if (canDelete)
                {
                    GameState.BoardStore.DispatchAction(new BoardActions.ResourcesRecovered { Count = ResourceCost });
                    Destroyed();
                    QueueFree();
                }
            }
        }
    }
}
