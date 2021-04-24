using Godot;

namespace Game.Util
{
    public static class GridUtils
    {
        public static bool IsPointWithinRadius(Vector2 p1, Vector2 p2, int radius)
        {
            var absX = (int)Mathf.Abs(p1.x - p2.x);
            var absY = (int)Mathf.Abs(p1.y - p2.y);
            return absX <= radius && absY <= radius;
        }
    }
}
