using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Map;
using CleanPlanet.Map.Procedural;
using CleanPlanet.Upgrade;
using CleanPlanet.Utils;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// 플레이어 위치와 이미 점유된 Cell을 제외한 빈 Cell 중 무작위로 골라 쓰레기 더미를 배치한다.
    /// 더미 종류(TrashPileType)는 SpawnWeight 기준으로 추첨하고, 보상은 선택된 종류의
    /// DropTable에서 다시 추첨한다.
    /// </summary>
    public class TrashSpawner : MonoBehaviour
    {
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private TrashPileType[] _pileTypes;
        [SerializeField, Min(0)] private int _spawnCount = 5;

        [Tooltip("연결하면 전체 그리드 스캔 대신 이 생성기의 SpawnableCells를 사용하고, 맵 생성이 끝난 뒤에만 스폰한다.")]
        [SerializeField] private ProceduralMapGenerator _mapGenerator;

        private readonly List<TrashPile> _spawnedPiles = new();

        private void OnEnable()
        {
            if (_mapGenerator != null)
            {
                _mapGenerator.OnMapGenerated += SpawnPiles;
            }
        }

        private void OnDisable()
        {
            if (_mapGenerator != null)
            {
                _mapGenerator.OnMapGenerated -= SpawnPiles;
            }
        }

        private void Start()
        {
            if (_mapGenerator == null)
            {
                SpawnPiles();
            }
        }

        private void SpawnPiles()
        {
            if (_pileTypes == null || _pileTypes.Length == 0)
            {
                Debug.LogError($"{nameof(TrashSpawner)}에 필요한 참조가 없습니다.", this);
                return;
            }

            ClearSpawnedPiles();

            GridSystem grid = _gridManager.Grid;
            GridOccupancy occupancy = _gridManager.Occupancy;

            List<Vector2Int> freeCells = _mapGenerator != null
                ? CollectFreeCellsFromGenerator(occupancy)
                : CollectFreeCells(grid, occupancy, CurrentPlayerIndex());
            Shuffle(freeCells);

            int upgradedSpawnCount = Mathf.RoundToInt(
                _spawnCount * UpgradeEffects.ExplorationSpawnCountMultiplier);
            int count = Mathf.Min(upgradedSpawnCount, freeCells.Count);
            for (int i = 0; i < count; i++)
            {
                SpawnOne(freeCells[i], grid, occupancy);
            }
        }

        private Vector2Int CurrentPlayerIndex()
        {
            return _gridManager.Player != null
                ? _gridManager.Player.CurrentIndex
                : new Vector2Int(-1, -1);
        }

        private void ClearSpawnedPiles()
        {
            foreach (var pile in _spawnedPiles)
            {
                if (pile != null) Destroy(pile.gameObject);
            }

            _spawnedPiles.Clear();
        }

        private List<Vector2Int> CollectFreeCellsFromGenerator(GridOccupancy occupancy)
        {
            var freeCells = new List<Vector2Int>();

            foreach (var index in _mapGenerator.SpawnableCells)
            {
                if (occupancy.IsOccupied(index)) continue;
                freeCells.Add(index);
            }

            return freeCells;
        }

        private void SpawnOne(Vector2Int index, GridSystem grid, GridOccupancy occupancy)
        {
            TrashPileType type = WeightedRandom.Pick(
                _pileTypes,
                GetSpawnWeight);
            if (type == null || type.Prefab == null)
            {
                Debug.LogWarning($"{nameof(TrashSpawner)}: 스폰할 TrashPileType 또는 Prefab을 선택하지 못했습니다.", this);
                return;
            }

            TrashPile pile = Instantiate(type.Prefab, transform);
            pile.Grid = grid;
            pile.Occupancy = occupancy;
            pile.SetReward(type.RollReward(UpgradeEffects.ExplorationOwnTierWeightMultiplier));
            pile.Spawn(index);
            _spawnedPiles.Add(pile);
        }

        private float GetSpawnWeight(TrashPileType type)
        {
            StageConfig stage = _mapGenerator != null ? _mapGenerator.ActiveStageConfig : null;
            float baseWeight = stage?.GetPileSpawnWeight(type) ?? type.SpawnWeight;

            if (!type.IsHigherTier) return baseWeight;

            return baseWeight * UpgradeEffects.ExplorationHighTierWeightMultiplier;
        }

        private static List<Vector2Int> CollectFreeCells(GridSystem grid, GridOccupancy occupancy, Vector2Int excludeIndex)
        {
            var freeCells = new List<Vector2Int>();

            for (int col = 0; col < grid.Columns; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    var index = new Vector2Int(col, row);
                    if (index == excludeIndex) continue;
                    if (occupancy.IsOccupied(index)) continue;
                    freeCells.Add(index);
                }
            }

            return freeCells;
        }

        private static void Shuffle(List<Vector2Int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
