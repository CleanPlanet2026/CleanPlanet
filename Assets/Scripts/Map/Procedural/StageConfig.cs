using System;
using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Trash;

namespace CleanPlanet.Map.Procedural
{
    [CreateAssetMenu(fileName = "StageConfig", menuName = "CleanPlanet/Map/Stage Config")]
    public sealed class StageConfig : ScriptableObject
    {
        [Serializable]
        private sealed class PileSpawnWeightEntry
        {
            [SerializeField] private TrashPileType _pileType;
            [SerializeField, Min(0f)] private float _weight = 1f;

            public TrashPileType PileType => _pileType;
            public float Weight => _weight;
        }

        [SerializeField] private int _stageId = 1;
        [SerializeField] private string _displayName = "Stage";
        [SerializeField, TextArea] private string _shortDescription;

        [SerializeField] private MapGenerationSettings _mapSettings = new();
        [SerializeField] private TileSet _tileSet;

        [Tooltip("이 스테이지를 클리어(Earth Clean 누적치 도달)하면 다음 스테이지가 열린다. 마지막 스테이지는 의미 없음.")]
        [SerializeField] private float _cleanGoalToUnlockNext = 100f;

        [Tooltip("이 스테이지에서 각 쓰레기 더미 종류가 스폰될 가중치. 등록되지 않은 종류는 TrashPileType의 기본 SpawnWeight를 사용한다.")]
        [SerializeField] private List<PileSpawnWeightEntry> _pileSpawnWeights;

        [Tooltip("이 스테이지에서 한 번에 스폰할 쓰레기 더미 총 개수.")]
        [SerializeField, Min(0)] private int _pileSpawnCount = 5;

        public int StageId => _stageId;
        public string DisplayName => _displayName;
        public string ShortDescription => _shortDescription;
        public MapGenerationSettings MapSettings => _mapSettings;
        public TileSet TileSet => _tileSet;
        public float CleanGoalToUnlockNext => _cleanGoalToUnlockNext;
        public int PileSpawnCount => _pileSpawnCount;

        /// <summary>
        /// 이 스테이지에 등록된 더미 종류별 스폰 가중치. 등록되어 있지 않으면 null을 반환해
        /// 호출측이 TrashPileType의 기본 SpawnWeight로 대체할 수 있게 한다.
        /// </summary>
        public float? GetPileSpawnWeight(TrashPileType pileType)
        {
            if (_pileSpawnWeights == null) return null;

            foreach (var entry in _pileSpawnWeights)
            {
                if (entry.PileType == pileType) return entry.Weight;
            }

            return null;
        }
    }
}
