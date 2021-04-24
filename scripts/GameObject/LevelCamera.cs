using Game.Level;
using Godot;
using GodotUtilities;

namespace Game
{
    public class LevelCamera : Camera2D
    {
        private const int PAN_SPEED = 250;

        private const string INPUT_MOVE_LEFT = "move_left";
        private const string INPUT_MOVE_RIGHT = "move_right";
        private const string INPUT_MOVE_UP = "move_up";
        private const string INPUT_MOVE_DOWN = "move_down";

        private Rect2 boundingRect;

        public override void _Ready()
        {
            var tileMap = this.GetAncestor<BaseLevel>().TileMap;
            boundingRect = tileMap.GetUsedRect();
            boundingRect = new Rect2
            {
                Position = boundingRect.Position * tileMap.CellSize,
                Size = boundingRect.Size * tileMap.CellSize,
            };

            // LimitBottom = (int)((rect.Position.y + rect.Size.y) * tileMap.CellSize.y);
            // LimitTop = (int)(rect.Position.y * tileMap.CellSize.y);
            // LimitLeft = (int)(rect.Position.x * tileMap.CellSize.x);
            // LimitRight = (int)((rect.Position.x + rect.Size.x) * tileMap.CellSize.x);
        }

        public override void _Process(float delta)
        {
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
    }
}
