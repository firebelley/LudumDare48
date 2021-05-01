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
        private const int BRANCHES = 2;
        private const int NODES_PER_BRANCH = 2;

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
            public int Cost = 0;
        }

        private class MainModel : BuildingModel
        {

        }

        private class TowerModel : BuildingModel
        {
            public TowerModel()
            {
                Radius = 3;
                Cost = 4;
            }
        }

        private class VillageModel : BuildingModel
        {
            public VillageModel()
            {
                Cost = 2;
            }
        }

        private class BarracksModel : BuildingModel
        {
            public BarracksModel()
            {
                Cost = 8;
            }
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

            public HashSet<Vector2> GetOccupiedTiles()
            {
                var allTiles = VillageModels.Select(x => x.Position)
                .Concat(BarracksModels.Select(x => x.Position))
                .Concat(GoblinCampModels.Select(x => x.Position))
                .Append(RootModel.Position).ToHashSet();
                return allTiles;
            }
        }

        private RegionNode rootNode;
        private List<RegionNode> allNodes = new();

        public override void _UnhandledInput(InputEvent evt)
        {
            base._UnhandledInput(evt);
            if (evt.IsActionPressed("activate"))
            {
                GetTree().ChangeScene(Filename);
            }
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
            PopulateRegion(rootNode);
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

                foreach (var village in region.VillageModels)
                {
                    scene = resourcePreloader.InstanceSceneOrNull<Village>();
                    scene.GlobalPosition = village.Position * TileMap.CellSize;
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

        private void PopulateRegion(RegionNode region)
        {
            allNodes.Add(region);
            CreateVillages(region);

            if (CountNodesInBranch(region) < NODES_PER_BRANCH)
            {
                CreateNewRegion(region);
            }
            else if (GetBranchCount() < BRANCHES)
            {
                CreateNewRegion(ChooseBranchNode());
            }
        }

        private void CreateNewRegion(RegionNode parentRegion)
        {
            var placementPos = GetRandomBorderTileInRadius(parentRegion.RootModel);
            var newRegion = new RegionNode
            {
                RootModel = new TowerModel
                {
                    Position = placementPos,
                },
                ParentRegion = parentRegion,
            };

            parentRegion.ConnectedNodes.Add(newRegion);
            PopulateRegion(newRegion);
        }

        private void CreateVillages(RegionNode region)
        {
            var occupiedTiles = GetAllOccupiedTiles();
            var validTilesInRadius = new List<Vector2>();
            GridUtils.ForEachTileInRadius(region.RootModel.Position, region.RootModel.Radius, (vec) =>
            {
                if (!occupiedTiles.Contains(vec))
                {
                    validTilesInRadius.Add(vec);
                }
            });

            validTilesInRadius = validTilesInRadius.OrderBy(x => MathUtil.RNG.Randf()).ToList();

            if (validTilesInRadius.Count == 0) return;

            if (MathUtil.RNG.Randf() < .75f)
            {
                region.VillageModels.Add(new VillageModel
                {
                    Position = validTilesInRadius[0],
                });

                if (validTilesInRadius.Count > 1 && MathUtil.RNG.Randf() < .5f)
                {
                    region.VillageModels.Add(new VillageModel
                    {
                        Position = validTilesInRadius[1],
                    });
                }
            }
        }

        private Vector2 GetRandomBorderTileInRadius(BuildingModel buildingModel)
        {
            var occupiedTiles = GetAllOccupiedTiles();
            var borderTiles = GridUtils.GetBorderTilesInRadius(buildingModel.Position, buildingModel.Radius);
            borderTiles = borderTiles.Where(x => !occupiedTiles.Contains(x)).ToList();
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

        private HashSet<Vector2> GetAllOccupiedTiles()
        {
            return allNodes.SelectMany(x => x.GetOccupiedTiles()).ToHashSet();
        }
    }
}
