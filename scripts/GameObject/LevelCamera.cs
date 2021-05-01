using Game.Level;
using Game.State;
using Godot;
using GodotUtilities;
using GodotUtilities.Util;

namespace Game
{
    public class LevelCamera : Camera2D
    {
        private const int PAN_SPEED = 250;
        private const float SHAKE_DECAY = 5f;
        private const float NOISE_GROWTH = 5f;
        private const float SHAKE_AMPLITUDE = 8f;

        private const string INPUT_MOVE_LEFT = "move_left";
        private const string INPUT_MOVE_RIGHT = "move_right";
        private const string INPUT_MOVE_UP = "move_up";
        private const string INPUT_MOVE_DOWN = "move_down";

        private Rect2 boundingRect;
        private float shakeMagnitude = 0;
        private Vector2 noiseSample = new(0, 25);

        public override void _EnterTree()
        {
            GameState.CreateEffect<BoardActions.ResourcesRecovered>(this, nameof(ShakeEffect));
            GameState.CreateEffect<BoardActions.Complete>(this, nameof(ShakeEffect));
            GameState.CreateEffect<BoardActions.BuildingPlaced>(this, nameof(ShakeEffect));
        }

        public override void _Ready()
        {
            CallDeferred(nameof(SetupBounds));
        }

        public override void _Process(float delta)
        {
            if (shakeMagnitude > 0)
            {
                shakeMagnitude = Mathf.Clamp(shakeMagnitude - (SHAKE_DECAY * delta), 0f, 1f);
                noiseSample += Vector2.One * NOISE_GROWTH * delta;
            }

            var xOffset = PerlinNoise.Noise(noiseSample.x, 0);
            var yOffset = PerlinNoise.Noise(0, noiseSample.y);
            Offset = new Vector2(xOffset, yOffset) * SHAKE_AMPLITUDE * shakeMagnitude * shakeMagnitude;

            var moveVec = new Vector2
            {
                x = Input.GetActionStrength(INPUT_MOVE_RIGHT) - Input.GetActionStrength(INPUT_MOVE_LEFT),
                y = Input.GetActionStrength(INPUT_MOVE_DOWN) - Input.GetActionStrength(INPUT_MOVE_UP),
            };

            GlobalPosition += moveVec * PAN_SPEED * delta;

            if (GlobalPosition.x < boundingRect.Position.x)
            {
                GlobalPosition = new Vector2(boundingRect.Position.x, GlobalPosition.y);
            }
            if (GlobalPosition.x > boundingRect.End.x)
            {
                GlobalPosition = new Vector2(boundingRect.End.x, GlobalPosition.y);
            }
            if (GlobalPosition.y < boundingRect.Position.y)
            {
                GlobalPosition = new Vector2(GlobalPosition.x, boundingRect.Position.y);
            }
            if (GlobalPosition.y > boundingRect.End.y)
            {
                GlobalPosition = new Vector2(GlobalPosition.x, boundingRect.End.y);
            }
        }

        private void SetupBounds()
        {
            var tileMap = this.GetAncestor<BaseLevel>().TileMap;
            boundingRect = tileMap.GetUsedRect();
            boundingRect = new Rect2
            {
                Position = boundingRect.Position * tileMap.CellSize,
                Size = boundingRect.Size * tileMap.CellSize,
            };
        }

        private void ShakeEffect(object _)
        {
            shakeMagnitude = 1f;
        }
    }
}
