using Game.Effect;
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
        [Node]
        protected ResourcePreloader resourcePreloader;
        [Node]
        protected AudioStreamPlayer buildAudioStreamPlayer;

        [Export]
        public int Radius { get; private set; } = 1;
        [Export]
        public int ResourceCost { get; private set; } = 2;
        [Export]
        public Texture GhostTexture { get; private set; }
        [Export]
        public bool Deletable { get; private set; } = true;
        [Export]
        public string DisplayName { get; private set; }

        protected TileMap tileMap;

        public bool PlayerPlaced;

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
            area2D.Connect("mouse_entered", this, nameof(OnAreaMouseEntered));
            area2D.Connect("mouse_exited", this, nameof(OnAreaMouseExited));

            if (PlayerPlaced)
            {
                buildAudioStreamPlayer.PlayWithPitchRange(.8f, 1f);
            }
        }

        public void SetTilePositionFromGlobalPosition()
        {
            TilePosition = tileMap.WorldToMap(GlobalPosition);
            Placed();
        }

        protected virtual void Placed()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.BuildingPlaced());
        }

        protected virtual void Destroyed()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.ClearTooltip { Owner = this, Force = true });
        }

        private void OnAreaInputEvent(object _, InputEvent inputEvent, object __)
        {
            if (inputEvent.IsActionPressed(INPUT_CLICK_ALTERNATE) && Deletable)
            {
                var canDelete = this.GetAncestor<BaseLevel>()?.CanDeleteBuilding(this) ?? false;
                if (canDelete)
                {
                    GameState.BoardStore.DispatchAction(new BoardActions.ResourcesRecovered { Count = ResourceCost });
                    Destroyed();
                    var destruction = resourcePreloader.InstanceSceneOrNull<BuildingDestruction>();
                    GetParent().AddChild(destruction);
                    destruction.GlobalPosition = GlobalPosition;
                    destruction.Texture = GhostTexture;
                    QueueFree();
                }
            }
        }

        private void OnAreaMouseEntered()
        {
            if (this is not MainBuilding && GameState.BoardStore.State.SelectedBuildingInfo == null)
            {
                var canDelete = this.GetAncestor<BaseLevel>()?.CanDeleteBuilding(this) ?? false;
                if (canDelete)
                {
                    GameState.BoardStore.DispatchAction(new BoardActions.ShowTooltip { Text = "Right click: Destroy", Owner = this, ShowDestroy = true });
                }
                else
                {
                    GameState.BoardStore.DispatchAction(new BoardActions.ShowTooltip { Text = "Can't destroy right now!", Owner = this });
                }
            }
        }

        private void OnAreaMouseExited()
        {
            GameState.BoardStore.DispatchAction(new BoardActions.ClearTooltip { Owner = this });
        }
    }
}
