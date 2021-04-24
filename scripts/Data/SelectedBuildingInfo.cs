using System;
using Game.GameObject;
using Godot;

namespace Game.Data
{
    public class SelectedBuildingInfo : Reference
    {
        public string ScenePath;
        public int Radius;
        public int Cost;
        public Texture Texture;
        public Type Type;

        public static SelectedBuildingInfo FromBuilding(Building building)
        {
            return new SelectedBuildingInfo
            {
                ScenePath = building.Filename,
                Radius = building.Radius,
                Cost = building.ResourceCost,
                Texture = building.GhostTexture,
                Type = building.GetType(),
            };
        }
    }
}
