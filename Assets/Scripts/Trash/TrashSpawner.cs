using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Map;
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

        private void Start()
        {
            SpawnPiles();
        }

        private void SpawnPiles()
        {
            if (_pileTypes == null || _pileTypes.Length == 0)
            {
                Debug.LogError($"{nameof(TrashSpawner)}에 필요한 참조가 없습니다.", this);
                return;
            }

            GridSystem grid = _gridManager.Grid;
            GridOccupancy occupancy = _gridManager.Occupancy;
            Vector2Int playerIndex = _gridManager.Player != null
                ? _gridManager.Player.CurrentIndex
                : new Vector2Int(-1, -1);

            List<Vector2Int> freeCells = CollectFreeCells(grid, occupancy, playerIndex);
            Shuffle(freeCells);

            int upgradedSpawnCount = Mathf.RoundToInt(
                _spawnCount * UpgradeEffects.ExplorationSpawnCountMultiplier);
            int count = Mathf.Min(upgradedSpawnCount, freeCells.Count);
            for (int i = 0; i < count; i++)
            {
                SpawnOne(freeCells[i], grid, occupancy);
            }
        }

        private void SpawnOne(Vector2Int index, GridSystem grid, GridOccupancy occupancy)
        {
            TrashPileType type = WeightedRandom.Pick(
                _pileTypes,
                t => t.SpawnWeight * GetTierWeightMultiplier(t));
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
        }

        private static float GetTierWeightMultiplier(TrashPileType type)
        {
            return type.IsHigherTier
                ? UpgradeEffects.ExplorationHighTierWeightMultiplier
                : 1f;
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
