using System;
using UnityEngine;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Utils;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// 쓰레기 더미 한 종류의 정의. SpawnWeight로 다른 종류 대비 얼마나 자주 스폰되는지,
    /// DropTable로 수집 성공 시 어떤 CollectibleData가 나올지 확률을 정의한다.
    /// </summary>
    [CreateAssetMenu(fileName = "TrashPileType", menuName = "CleanPlanet/Trash/Trash Pile Type")]
    public sealed class TrashPileType : ScriptableObject
    {
        [Serializable]
        public struct DropEntry
        {
            public CollectibleData Collectible;
            [Min(0f)] public float Weight;
        }

        [SerializeField] private string _name;
        [SerializeField] private TrashPile _prefab;
        [SerializeField, Min(0f)] private float _spawnWeight = 1f;
        [SerializeField] private DropEntry[] _dropTable;

        public string Name => _name;
        public TrashPile Prefab => _prefab;
        public float SpawnWeight => _spawnWeight;

        /// <summary>
        /// DropTable을 가중치 기준으로 추첨해 CollectibleData 하나를 반환한다.
        /// DropTable이 비어있거나 가중치 합이 0이면 null.
        /// </summary>
        public CollectibleData RollReward()
        {
            if (_dropTable == null || _dropTable.Length == 0)
            {
                return null;
            }

            return WeightedRandom.Pick(_dropTable, entry => entry.Weight).Collectible;
        }
    }
}
