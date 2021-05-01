using System.Collections.Generic;
using System.Linq;
using Game.Util;
using Godot;
using GodotUtilities;

namespace Game.Level
{
    public class RandomLevel : BaseLevel
    {
        private const int WIDTH = 20;
        private const int HEIGHT = 20;
        private const int SIMULATIONS = 0;

        private Dictionary<Vector2, int> currentMap = new();

        public override void _Ready()
        {
            base._Ready();
            Randomize();
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            base._UnhandledInput(evt);
            if (evt.IsActionPressed("activate"))
            {
                if (evt is InputEventKey key && key.Control)
                {
                    Randomize();
                }
                else
                {
                    DoSimulation();
                }
            }
        }

        private void Randomize()
        {
            CreateTileMapCanvas();
            CreateInitialTiles();
            foreach (var _ in Enumerable.Range(0, SIMULATIONS))
            {
                DoSimulation();
            }
            TileMap.UpdateBitmaskRegion(TileMap.GetUsedRect().Position, TileMap.GetUsedRect().End);

        }

        private void CreateTileMapCanvas()
        {
            TileMap.Clear();
            foreach (var x in Enumerable.Range(0, WIDTH))
            {
                foreach (var y in Enumerable.Range(0, HEIGHT))
                {
                    TileMap.SetCell(x, y, 0);
                }
            }
        }

        private void CreateInitialTiles()
        {
            foreach (var vec in TileMap.GetUsedCells().Cast<Vector2>())
            {
                var chance = MathUtil.RNG.Randf();
                if (chance < .1f)
                {
                    TileMap.SetCellv(vec, 1);
                }
                else if (chance < .2f)
                {
                    TileMap.SetCellv(vec, 2);
                }
            }
            StoreMap();
        }

        private void StoreMap()
        {
            foreach (var vec in TileMap.GetUsedCells().Cast<Vector2>())
            {
                currentMap[vec] = TileMap.GetCellv(vec);
            }
        }

        private void DoSimulation()
        {
            DoTreeLoop();
            // DoGrassLoop();
            StoreMap();
        }

        //determine if the tree lives or dies
        private void DoTreeLoop()
        {
            foreach (var tile in currentMap.Keys)
            {
                if (currentMap[tile] == 2) continue;

                var areaResourceSum = 0;
                GridUtils.ForEachTileInRadius(tile, 2, (vec) =>
                {
                    if (vec == tile) return;
                    if (!currentMap.ContainsKey(vec)) return;
                    if (currentMap[vec] == 1) areaResourceSum++;
                });

                var neighborResourceSum = 0;
                GridUtils.ForEachTileInRadius(tile, 1, (vec) =>
                {
                    if ((tile - vec).LengthSquared() > 1) return;
                    if (vec == tile) return;
                    if (!currentMap.ContainsKey(vec)) return;
                    if (currentMap[vec] == 1) neighborResourceSum++;
                });

                // if (areaResourceSum < 2)
                // {
                //     TileMap.SetCellv(tile, 1);
                // }

                // if (areaResourceSum > 8)
                // {
                //     TileMap.SetCellv(tile, 0);
                // }

                if (neighborResourceSum is > 2 and < 4)
                {
                    TileMap.SetCellv(tile, 0);
                }
                if (areaResourceSum < 3)
                {
                    TileMap.SetCellv(tile, 1);
                }
                // if (neighborResourceSum == 0 && areaResourceSum < 3)
                // {
                //     TileMap.SetCellv(tile, 1);
                // }
                // if (areaResourceSum > 9)
                // {
                //     TileMap.SetCellv(tile, 0);
                // }
                // if (neighborResourceSum)
            }
        }

        private void DoGrassLoop()
        {
            foreach (var tile in currentMap.Keys)
            {
                if (currentMap[tile] != 0) continue;
                var neighborTreeCount = 0;
                GridUtils.ForEachTileInRadius(tile, 1, (vec) =>
                {
                    if (!currentMap.ContainsKey(vec)) return;
                    if (currentMap[vec] == 1) neighborTreeCount++;
                });

                if (neighborTreeCount == 1)
                {
                    TileMap.SetCellv(tile, 1);
                }
            }
        }
    }
}
