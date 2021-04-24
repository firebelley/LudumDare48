using System;
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

        public static void ForEachTileInRadius(Vector2 p, int radius, Action<Vector2> action)
        {
            for (int x = (int)p.x - radius; x <= p.x + radius; x++)
            {
                for (int y = (int)p.y - radius; y <= p.y + radius; y++)
                {
                    var vec = new Vector2(x, y);
                    action(vec);
                }
            }
        }
    }
}
