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
        private const int NODES_PER_BRANCH = 5;
        private const int MIN_VILLAGE_RESOURCES = 3;
        private const int MAX_VILLAGE_RESOURCES = 8;

        [Export]
        private bool debugMode;

        public override void _Ready()
        {
            base._Ready();
            Randomize();

            var allUsedTiles = GetAllOccupiedTiles();
            allUsedTiles.UnionWith(resourceTiles);
            allUsedTiles.Add(goalTile);
            var rect = new Rect2();
            foreach (var tile in allUsedTiles)
            {
                rect = rect.Expand(tile);
            }

            for (int x = (int)rect.Position.x; x <= rect.End.x; x++)
            {
                for (int y = (int)rect.Position.y; y <= rect.End.y; y++)
                {
                    TileMap.SetCell(x, y, 0);
                }
            }

            TileMap.UpdateBitmaskRegion(rect.Position, rect.End);

            PlaceBuildings();
            PlaceResources();
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
            public string Label;
            public RegionNode ParentRegion;
            public BuildingModel RootModel;
            public List<VillageModel> VillageModels = new();
            public List<BarracksModel> BarracksModels = new();
            public List<GoblinCampModel> GoblinCampModels = new();
            public List<RegionNode> ConnectedNodes = new();
            public List<BuildingModel> AllBuildings => new List<BuildingModel>()
                .Concat(VillageModels)
                .Concat(BarracksModels)
                .Concat(GoblinCampModels)
                .Append(RootModel)
                .ToList();
            public HashSet<Vector2> OwnedResourceTiles = new();

            public HashSet<Vector2> GetOccupiedTiles()
            {
                return AllBuildings.Select(x => x.Position).ToHashSet();
            }

            public int CountNetResources(HashSet<Vector2> resourceTiles)
            {
                var sum = 0;
                foreach (var village in VillageModels)
                {
                    GridUtils.ForEachTileInRadius(village.Position, village.Radius, (vec) =>
                    {
                        if (OwnedResourceTiles.Contains(vec)) sum++;
                    });
                }

                foreach (var building in AllBuildings)
                {
                    sum -= building.Cost;
                }
                return sum;
            }

            public int CountBuildingCost()
            {
                return AllBuildings.Sum(x => x.Cost);
            }
        }

        private RegionNode rootNode;
        private List<RegionNode> allNodes = new();
        private HashSet<Vector2> resourceTiles = new();
        private Vector2 goalTile;

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
            PopulateResources(rootNode);
        }

        private void PlaceBuildings()
        {
            if (!debugMode)
            {
                var goalScene = resourcePreloader.InstanceSceneOrNull<Goal>();
                goalScene.GlobalPosition = TileMap.MapToWorld(goalTile);
                Entities.AddChild(goalScene);
            }

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
                    scene.AddChild(new Label
                    {
                        Text = region.Label,
                        Modulate = Colors.Red,
                    });
                    var label = new Label
                    {
                        Text = $"{region.CountNetResources(resourceTiles)}",
                        RectPosition = Vector2.Down * 16f,
                        Modulate = Colors.Blue,
                    };
                    scene.AddChild(label);
                    if (!debugMode)
                    {
                        break;
                    }
                }

                foreach (var village in region.VillageModels)
                {
                    scene = resourcePreloader.InstanceSceneOrNull<Village>();
                    scene.GlobalPosition = village.Position * TileMap.CellSize;
                    Entities.AddChild(scene);
                    scene.AddChild(new Label
                    {
                        Text = region.Label,
                        Modulate = Colors.Red,
                    });
                }

                foreach (var connected in region.ConnectedNodes)
                {
                    var line2d = new Line2D
                    {
                        Points = new Vector2[] {
                            TileMap.MapToWorld(region.RootModel.Position) + (TileMap.CellSize / 2),
                            TileMap.MapToWorld(connected.RootModel.Position) + (TileMap.CellSize / 2),
                        },
                        Width = 2
                    };
                    AddChild(line2d);
                }
            }
        }

        private void PlaceResources()
        {
            foreach (var tile in resourceTiles)
            {
                TileMap.SetCellv(tile, 1);
            }
        }

        private void PopulateRegion(RegionNode region)
        {
            allNodes.Add(region);
            region.Label = allNodes.IndexOf(region).ToString();
            // AccrueResources(region);
            // TODO: place barracks, camps here

            if (CountNodesInBranch(region) < NODES_PER_BRANCH)
            {
                CreateNewRegion(region);
            }
            else if (GetBranchCount() < BRANCHES)
            {
                CreateNewRegion(ChooseBranchNode());
            }
            else
            {
                goalTile = GetRandomBorderTileInRadius(region.RootModel);
            }
        }

        private void PopulateResources(RegionNode region)
        {
            var availableResources = CountTotalNetResourcesAvailable(region);

            // TODO: Generate enough resources to cover minimum building (tower + village)
            // TODO: if there is additional cost not covered, generate those resources in the node itself

            foreach (var child in region.ConnectedNodes)
            {
                var targetNetResourceGain = Mathf.Max(6, child.CountBuildingCost());
                while (availableResources < targetNetResourceGain)
                {
                    PlaceVillage(region, targetNetResourceGain);
                    availableResources = CountTotalNetResourcesAvailable(region);
                }
            }

            foreach (var child in region.ConnectedNodes)
            {
                PopulateResources(child);
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

        private Vector2 GetRandomBorderTileInRadius(BuildingModel buildingModel)
        {
            var availableTiles = GetOpenTilesInRadius(buildingModel.Position, buildingModel.Radius);
            var borderTiles = GridUtils.GetBorderTilesInRadius(buildingModel.Position, buildingModel.Radius);
            borderTiles = borderTiles.Where(availableTiles.Contains).ToList();
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

        private List<Vector2> GetOpenTilesInRadius(Vector2 p, int radius)
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

        private void PlaceVillage(RegionNode region, int targetMinNet = MIN_VILLAGE_RESOURCES, int targetMaxNet = MAX_VILLAGE_RESOURCES)
        {
            var validPlacementTiles = new List<Vector2>();
            while (validPlacementTiles.Count == 0)
            {
                validPlacementTiles = GetOpenTilesInRadius(region.RootModel.Position, region.RootModel.Radius);
                if (region.ParentRegion != null)
                {
                    validPlacementTiles = validPlacementTiles
                        .Where(x => !GridUtils.IsPointWithinRadius(region.ParentRegion.RootModel.Position, x, region.ParentRegion.RootModel.Radius))
                        .ToList();
                }
            }

            var placementTile = validPlacementTiles.OrderBy(Shuffle).First();
            var villageModel = new VillageModel
            {
                Position = placementTile,
            };
            region.VillageModels.Add(villageModel);

            var validResourceTiles = GetOpenTilesInRadius(villageModel.Position, villageModel.Radius);

            var targetResourceCount = MathUtil.RNG.RandiRange(targetMinNet, Mathf.Max(targetMinNet, targetMaxNet));
            var placementResources = validResourceTiles.OrderBy(Shuffle).Take(targetResourceCount + villageModel.Cost).ToHashSet();

            resourceTiles.UnionWith(placementResources);
            region.OwnedResourceTiles.UnionWith(placementResources);
        }

        private int CountTotalNetResourcesAvailable(RegionNode toNode)
        {
            var parent = toNode.ParentRegion;
            var sum = startingResources;
            while (parent != null)
            {
                sum += parent.CountNetResources(resourceTiles);
                parent = parent.ParentRegion;
            }
            sum += toNode.CountNetResources(resourceTiles);
            return sum;
        }

        private float Shuffle<T>(T x)
        {
            return MathUtil.RNG.Randf();
        }
    }
}
