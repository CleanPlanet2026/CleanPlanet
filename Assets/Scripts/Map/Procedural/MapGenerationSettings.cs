using System;
using UnityEngine;

namespace CleanPlanet.Map.Procedural
{
    [Serializable]
    public sealed class MapGenerationSettings
    {
        [Min(1)] public int Width = 40;
        [Min(1)] public int Height = 40;
        [Min(0.01f)] public float TileSize = 1f;

        [Range(0f, 1f)] public float ObstacleDensity = 0.42f;
        [Min(0)] public int SmoothingIterations = 4;

        [Min(0)] public int PlayerStartEdgeMargin = 3;
        [Min(0)] public int PlayerStartOpenRadius = 2;
        [Range(0f, 1f)] public float PlayerStartMinOpenRatio = 0.6f;

        [Range(0f, 1f)] public float MinConnectedFloorRatio = 0.7f;
        [Min(1)] public int MaxGenerationAttempts = 5;

        [Min(0)] public int ObjectSpawnExclusionRadius = 2;

        public int Seed = 0;
        public bool UseFixedSeed = false;
    }
}
