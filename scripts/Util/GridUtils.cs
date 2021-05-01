using System;
using System.Collections.Generic;
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

        public static void ForEachTileInRadius(Vector2 p, int radius, Action<Vector2> action, bool excludeSelf = false)
        {
            for (int x = (int)p.x - radius; x <= p.x + radius; x++)
            {
                for (int y = (int)p.y - radius; y <= p.y + radius; y++)
                {
                    var vec = new Vector2(x, y);
                    if (excludeSelf && vec == p)
                    {
                        continue;
                    }
                    action(vec);
                }
            }
        }

        public static List<Vector2> GetTilesInRadius(Vector2 p, int radius, bool excludeSelf = false)
        {
            var result = new List<Vector2>();
            ForEachTileInRadius(p, radius, (vec) => result.Add(vec), excludeSelf);
            return result;
        }

        public static List<Vector2> GetBorderTilesInRadius(Vector2 p, int radius)
        {
            var result = new List<Vector2>();
            ForEachTileInRadius(p, radius, (vec) =>
            {
                var absX = (int)Mathf.Abs(p.x - vec.x);
                var absY = (int)Mathf.Abs(p.y - vec.y);
                if (absX == radius || absY == radius)
                {
                    result.Add(vec);
                }
            });
            return result;
        }
    }
}
