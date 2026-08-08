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

        [Tooltip("셀룰러 오토마타 초기 노이즈에서 벽(동굴 지형)이 될 확률.")]
        [Range(0f, 1f)] public float WallDensity = 0.42f;
        [Min(0)] public int SmoothingIterations = 4;

        [Tooltip("Wall 지형과 별개로, Floor 위에 흩뿌리는 독립 장애물(Obstacle) 개수. 이동을 막는다.")]
        [Min(0)] public int ObstacleCount = 20;

        [Tooltip("Floor 위에 흩뿌리는 장식(Decoration) 개수. 이동을 막지 않는 순수 시각 요소다.")]
        [Min(0)] public int DecorationCount = 30;

        [Min(0)] public int PlayerStartEdgeMargin = 3;
        [Min(0)] public int PlayerStartOpenRadius = 2;
        [Range(0f, 1f)] public float PlayerStartMinOpenRatio = 0.6f;

        [Range(0f, 1f)] public float MinConnectedFloorRatio = 0.7f;
        [Min(1)] public int MaxGenerationAttempts = 5;

        [Min(0)] public int ObjectSpawnExclusionRadius = 2;

        public int Seed = 0;
        public bool UseFixedSeed = false;

        /// <summary>
        /// 얕은 복사본을 만든다. StageConfig에 연결된 인스턴스를 그대로 참조해 쓰면
        /// GenerateWithSeed 등이 에셋 자체를 영구적으로 수정해버리므로, 런타임에
        /// 사용할 땐 항상 복제본을 써야 한다.
        /// </summary>
        public MapGenerationSettings Clone()
        {
            return (MapGenerationSettings)MemberwiseClone();
        }
    }
}
