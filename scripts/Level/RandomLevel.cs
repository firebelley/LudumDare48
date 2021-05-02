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
        private const int BRANCHES = 0;
        private const int NODES_PER_BRANCH = 2;
        private const int MIN_VILLAGE_RESOURCES = 3;
        private const int MAX_VILLAGE_RESOURCES = 8;

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
            public List<BuildingModel> AllBuildings => new List<BuildingModel>()
                .Concat(VillageModels)
                .Concat(BarracksModels)
                .Append(RootModel)
                .ToList();

            public HashSet<Vector2> GetOccupiedTiles()
            {
                var allTiles = VillageModels.Select(x => x.Position)
                .Concat(BarracksModels.Select(x => x.Position))
                .Concat(GoblinCampModels.Select(x => x.Position))
                .Append(RootModel.Position).ToHashSet();
                return allTiles;
            }

            public int CountNetResources(HashSet<Vector2> resourceTiles)
            {
                var sum = 0;
                foreach (var village in VillageModels)
                {
                    GridUtils.ForEachTileInRadius(village.Position, village.Radius, (vec) =>
                    {
                        if (resourceTiles.Contains(vec)) sum++;
                    });
                }

                foreach (var building in AllBuildings)
                {
                    sum -= building.Cost;
                }
                return sum;
            }
        }

        private RegionNode rootNode;
        private List<RegionNode> allNodes = new();

        // Resources that were available to the Node represented by the key
        // For example, the starting node starts with startingResources accruedResources
        // When the next node is visited, the accrued resources will be the net resources from its parent node
        private Dictionary<RegionNode, int> accruedResources = new();
        private HashSet<Vector2> resourceTiles = new();

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
            accruedResources[rootNode] = startingResources;
            PopulateRegion(rootNode);
        }

        private void PlaceBuildings()
        {
            var idx = 0;

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
                    scene.AddChild(new Label
                    {
                        Text = idx.ToString(),
                    });
                    idx++;
                }

                foreach (var tile in resourceTiles)
                {
                    TileMap.SetCellv(tile, 1);
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
            AccrueResources(region);

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

            accruedResources[newRegion] = parentRegion.CountNetResources(resourceTiles) + accruedResources[parentRegion];

            parentRegion.ConnectedNodes.Add(newRegion);
            PopulateRegion(newRegion);
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

        private List<Vector2> GetAvailableResourceTilesInRadius(Vector2 p, int radius)
        {
            var unavailableTiles = new HashSet<Vector2>();
            unavailableTiles.UnionWith(resourceTiles);
            unavailableTiles.UnionWith(GetAllOccupiedTiles());

            var validTiles = new List<Vector2>();
            GridUtils.ForEachTileInRadius(p, radius, (vec) =>
            {
                if (!unavailableTiles.Contains(vec))
                {
                    validTiles.Add(vec);
                }
            });

            return validTiles;
        }

        private void AccrueResources(RegionNode region)
        {
            var availableResources = accruedResources[region];
            if (availableResources <= 6)
            {
                var villageCount = MathUtil.RNG.RandiRange(1, availableResources / 2);
                foreach (var _ in Enumerable.Range(0, villageCount))
                {
                    var targetMin = Mathf.Max(6 - availableResources + new VillageModel().Cost, MIN_VILLAGE_RESOURCES);
                    PlaceVillage(region, targetMin);
                }
            }
            else
            {
                if (MathUtil.RNG.Randf() < .5f)
                {

                }
            }
        }

        private void PlaceVillage(RegionNode region, int targetMin = MIN_VILLAGE_RESOURCES, int targetMax = MAX_VILLAGE_RESOURCES)
        {
            var validPlacementTiles = GetAvailableResourceTilesInRadius(region.RootModel.Position, region.RootModel.Radius);
            if (validPlacementTiles.Count == 0) return;

            var placementTile = validPlacementTiles.OrderBy(Shuffle).First();
            var villageModel = new VillageModel
            {
                Position = placementTile,
            };

            var validResourceTiles = GetAvailableResourceTilesInRadius(villageModel.Position, villageModel.Radius);
            validResourceTiles.Remove(villageModel.Position);

            if (validResourceTiles.Count < targetMin)
            {
                GD.Print("OOPS");
                return;
            }

            var targetResourceCount = MathUtil.RNG.RandiRange(targetMin, targetMax);
            var placementResources = validResourceTiles.OrderBy(Shuffle).Take(targetResourceCount).ToHashSet();

            resourceTiles.UnionWith(placementResources);
            region.VillageModels.Add(villageModel);
        }

        private float Shuffle<T>(T x)
        {
            return MathUtil.RNG.Randf();
        }
    }
}
