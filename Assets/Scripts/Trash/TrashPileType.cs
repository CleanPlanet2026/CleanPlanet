using System;
using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Utils;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// 쓰레기 더미 한 종류의 정의. SpawnWeight로 다른 종류 대비 얼마나 자주 스폰되는지,
    /// 자기 등급의 수집물(OwnItems)과 한 단계 낮은 더미(PreviousTier)를 통해 드랍 후보를
    /// 정의한다. 예를 들어 유리 더미는 자신의 유리 수집물과 일반 더미(PreviousTier)가 갖는
    /// 일반 수집물을 함께 드랍하며, 전자 더미는 유리 더미를 PreviousTier로 잡아 일반·유리·
    /// 전자 수집물을 모두 드랍한다. 각 더미는 자신이 새로 추가하는 수집물만 소유하므로
    /// 하위 등급 목록을 중복으로 나열할 필요가 없다.
    /// </summary>
    [CreateAssetMenu(fileName = "TrashPileType", menuName = "CleanPlanet/Trash/Trash Pile Type")]
    public sealed class TrashPileType : ScriptableObject
    {
        private readonly struct DropEntry
        {
            public readonly CollectibleData Collectible;
            public readonly float Weight;

            public DropEntry(CollectibleData collectible, float weight)
            {
                Collectible = collectible;
                Weight = weight;
            }
        }

        [SerializeField] private string _name;
        [SerializeField] private TrashPile _prefab;
        [SerializeField, Min(0f)] private float _spawnWeight = 1f;

        [SerializeField] private CollectibleData[] _ownItems;
        [SerializeField, Min(0f)] private float _ownWeight = 1f;

        [Tooltip("한 단계 낮은 더미 종류. 그 더미의 드랍 후보 전체를 이어받는다.")]
        [SerializeField] private TrashPileType _previousTier;
        [SerializeField, Min(0f)] private float _inheritedWeight = 3f;

        public string Name => _name;
        public TrashPile Prefab => _prefab;
        public float SpawnWeight => _spawnWeight;
        public bool IsHigherTier => _previousTier != null;

        /// <summary>
        /// 자기 등급 수집물과 PreviousTier 체인의 모든 수집물을 가중치 기준으로 추첨한다.
        /// 후보가 하나도 없으면 null.
        /// </summary>
        public CollectibleData RollReward(float ownTierWeightMultiplier = 1f)
        {
            var pool = new List<DropEntry>();
            CollectDropPool(
                pool,
                isOwnTier: true,
                ownTierWeightMultiplier: ownTierWeightMultiplier);

            if (pool.Count == 0)
            {
                return null;
            }

            return WeightedRandom.Pick(pool, entry => entry.Weight).Collectible;
        }

        private void CollectDropPool(
            List<DropEntry> pool,
            bool isOwnTier,
            float ownTierWeightMultiplier)
        {
            float weight = isOwnTier
                ? _ownWeight * ownTierWeightMultiplier
                : _inheritedWeight;

            if (_ownItems != null)
            {
                foreach (CollectibleData item in _ownItems)
                {
                    if (item != null)
                    {
                        pool.Add(new DropEntry(item, weight));
                    }
                }
            }

            _previousTier?.CollectDropPool(
                pool,
                isOwnTier: false,
                ownTierWeightMultiplier: ownTierWeightMultiplier);
        }
    }
}
