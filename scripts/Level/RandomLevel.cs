using System.Collections.Generic;
using System.Linq;
using Game.GameObject;
using Game.Util;
using Godot;
using GodotUtilities;

namespace Game.Level
{
    public class RandomLevel : BaseLevel
    {
        private const int BRANCHES = 3;
        private const int NODES_PER_BRANCH = 5;

        public override void _Ready()
        {
            base._Ready();
            foreach (int x in Enumerable.Range(-20, 40))
            {
                foreach (int y in Enumerable.Range(-20, 40))
                {
                    TileMap.SetCell(x, y, 0);
                }
            }
            Randomize();
            PlaceBuildings();
        }

        private class BuildingModel
        {
            public Vector2 Position;
            public int Radius = 2;
        }

        private class MainModel : BuildingModel
        {

        }

        private class TowerModel : BuildingModel
        {
            public TowerModel()
            {
                Radius = 3;
            }
        }

        private class VillageModel : BuildingModel
        {

        }

        private class BarracksModel : BuildingModel
        {

        }

        private class GoblinCampModel : BuildingModel
        {

        }

        private class RegionNode
        {
            public RegionNode ParentRegion;
            public BuildingModel RootModel;
            public List<VillageModel> VillageModels = new();
            public List<BarracksModel> BarracksModels = new();
            public List<GoblinCampModel> GoblinCampModels = new();
            public List<RegionNode> ConnectedNodes = new();
        }

        private RegionNode rootNode;
        private List<RegionNode> allNodes = new();

        public override void _UnhandledInput(InputEvent evt)
        {
            // base._UnhandledInput(evt);
            // if (evt.IsActionPressed("activate"))
            // {
            //     if (evt is InputEventKey key && key.Control)
            //     {
            //         Randomize();
            //     }
            //     else
            //     {
            //         DoSimulation();
            //     }
            // }
        }

        private void Randomize()
        {
            rootNode = new RegionNode
            {
                RootModel = new MainModel
                {
                    Position = Vector2.Zero,
                }
            };
            allNodes.Add(rootNode);
            PopulateRegions(rootNode);
        }

        private void PlaceBuildings()
        {
            foreach (var region in allNodes)
            {
                Node2D scene = region.RootModel switch
                {
                    TowerModel => resourcePreloader.InstanceSceneOrNull<Tower>(),
                    MainModel => resourcePreloader.InstanceSceneOrNull<MainBuilding>(),
                    _ => null,
                };
                if (scene != null)
                {
                    scene.GlobalPosition = region.RootModel.Position * TileMap.CellSize;
                    Entities.AddChild(scene);
                }

                foreach (var connected in region.ConnectedNodes)
                {
                    var line2d = new Line2D
                    {
                        Points = new Vector2[] {
                            region.RootModel.Position * TileMap.CellSize,
                            connected.RootModel.Position * TileMap.CellSize,
                        },
                        Width = 2
                    };
                    AddChild(line2d);
                }
            }
        }

        private void PopulateRegions(RegionNode region)
        {
            var placementPos = GetRandomBorderTileInRadius(region.RootModel);
            var newRegion = new RegionNode
            {
                RootModel = new TowerModel
                {
                    Position = placementPos,
                },
                ParentRegion = region,
            };

            region.ConnectedNodes.Add(newRegion);
            allNodes.Add(newRegion);

            if (CountNodesInBranch(newRegion) < NODES_PER_BRANCH)
            {
                PopulateRegions(newRegion);
            }
            else if (GetBranchCount() < BRANCHES)
            {
                PopulateRegions(ChooseBranchNode());
            }
        }

        private Vector2 GetRandomBorderTileInRadius(BuildingModel buildingModel)
        {
            var borderTiles = GridUtils.GetBorderTilesInRadius(buildingModel.Position, buildingModel.Radius);
            borderTiles = borderTiles.Where(x => !allNodes.Any(y => y.RootModel.Position == x)).ToList();
            var oneRootTiles = borderTiles.Where(x => CountRootModelsInRadius(x, buildingModel.Radius) == 1);
            var useList = oneRootTiles.Any() ? oneRootTiles : borderTiles;
            return useList.OrderBy(x => MathUtil.RNG.Randf()).First();
        }

        private int GetBranchCount()
        {
            return allNodes.Sum(x => Mathf.Max(0, x.ConnectedNodes.Count - 1));
        }

        private int CountNodesInBranch(RegionNode region)
        {
            var nodesInBranch = 1;
            var parent = region.ParentRegion;
            while (parent?.ConnectedNodes.Count == 1)
            {
                nodesInBranch++;
                parent = parent.ParentRegion;
                if (parent?.ConnectedNodes.Count > 1)
                {
                    nodesInBranch++;
                    break;
                }
            }
            return nodesInBranch;
        }

        private RegionNode ChooseBranchNode()
        {
            return allNodes.Where(x => x.ConnectedNodes.Count == 1).OrderBy(x => MathUtil.RNG.Randf()).First();
        }

        private int CountRootModelsInRadius(Vector2 p, int radius)
        {
            return allNodes.Count(x => GridUtils.IsPointWithinRadius(x.RootModel.Position, p, radius));
        }
    }
}
