using System;
using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Core.Appraisal;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// 쓰레기 더미 한 종류의 정의. SpawnWeight로 다른 종류 대비 얼마나 자주 스폰되는지,
    /// GradeDropWeights로 이 더미에서 어떤 등급의 수집물이 얼마나 자주 나오는지 정의한다.
    /// 등급별 실제 수집물 후보는 자기 등급의 수집물(OwnItems)과 한 단계 낮은 더미
    /// (PreviousTier) 체인을 따라 찾는다. 예를 들어 유리 더미는 자신의 유리 수집물과
    /// 일반 더미(PreviousTier)가 갖는 일반 수집물을 후보로 갖고, 전자 더미는 유리 더미를
    /// PreviousTier로 잡아 일반·유리·전자 수집물을 모두 후보로 갖는다. 각 더미는 자신이
    /// 새로 추가하는 수집물만 소유하므로 하위 등급 목록을 중복으로 나열할 필요가 없다.
    /// </summary>
    [CreateAssetMenu(fileName = "TrashPileType", menuName = "CleanPlanet/Trash/Trash Pile Type")]
    public sealed class TrashPileType : ScriptableObject
    {
        [Serializable]
        private sealed class GradeWeightEntry
        {
            [SerializeField] private ItemGrade _grade;
            [SerializeField, Min(0f)] private float _weight = 1f;

            public ItemGrade Grade => _grade;
            public float Weight => _weight;
        }

        [SerializeField] private string _name;
        [SerializeField] private TrashPile _prefab;
        [SerializeField, Min(0f)] private float _spawnWeight = 1f;

        [SerializeField] private CollectibleData[] _ownItems;

        [Tooltip("한 단계 낮은 더미 종류. 그 더미의 드랍 후보 전체를 이어받는다.")]
        [SerializeField] private TrashPileType _previousTier;

        [Tooltip("이 더미에서 각 수집물 등급이 나올 가중치. 예: 유리 더미는 Common 75, Uncommon 25.")]
        [SerializeField] private List<GradeWeightEntry> _gradeDropWeights;

        public string Name => _name;
        public TrashPile Prefab => _prefab;
        public float SpawnWeight => _spawnWeight;
        public bool IsHigherTier => _previousTier != null;

        // OwnItems/PreviousTier는 런타임에 바뀌지 않는 애셋 데이터이므로, 등급별 후보 목록을
        // 스폰마다 다시 모으는 대신 처음 한 번만 만들어 재사용해 스폰 개수가 많아져도
        // GC 할당이 늘지 않게 한다.
        private Dictionary<ItemGrade, List<CollectibleData>> _dropPoolByGrade;
        private ItemGrade? _ownGrade;

        /// <summary>
        /// GradeDropWeights로 등급을 먼저 추첨하고, 그 등급에 해당하는 수집물을
        /// (자기 OwnItems + PreviousTier 체인에서) 균등하게 골라 반환한다.
        /// ownTierWeightMultiplier는 이 더미의 고유 등급(OwnItems의 등급)이 나올 가중치에만
        /// 곱해져 업그레이드로 자기 등급 드랍률을 끌어올릴 수 있게 한다. 후보가 없으면 null.
        /// </summary>
        public CollectibleData RollReward(float ownTierWeightMultiplier = 1f)
        {
            if (_gradeDropWeights == null || _gradeDropWeights.Count == 0)
            {
                Debug.LogWarning($"{nameof(TrashPileType)}({_name}): 등급 드랍 가중치가 비어 있습니다.", this);
                return null;
            }

            EnsureDropPoolBuilt();

            ItemGrade? grade = PickGrade(ownTierWeightMultiplier);
            if (grade == null || !_dropPoolByGrade.TryGetValue(grade.Value, out List<CollectibleData> candidates))
            {
                return null;
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private ItemGrade? PickGrade(float ownTierWeightMultiplier)
        {
            float total = 0f;
            for (int i = 0; i < _gradeDropWeights.Count; i++)
            {
                total += EntryWeight(_gradeDropWeights[i], ownTierWeightMultiplier);
            }

            if (total <= 0f) return null;

            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < _gradeDropWeights.Count; i++)
            {
                GradeWeightEntry entry = _gradeDropWeights[i];
                cumulative += EntryWeight(entry, ownTierWeightMultiplier);
                if (roll <= cumulative) return entry.Grade;
            }

            return _gradeDropWeights[^1].Grade;
        }

        private float EntryWeight(GradeWeightEntry entry, float ownTierWeightMultiplier)
        {
            return entry.Grade == _ownGrade ? entry.Weight * ownTierWeightMultiplier : entry.Weight;
        }

        private void EnsureDropPoolBuilt()
        {
            if (_dropPoolByGrade != null) return;

            _dropPoolByGrade = new Dictionary<ItemGrade, List<CollectibleData>>();
            for (TrashPileType tier = this; tier != null; tier = tier._previousTier)
            {
                if (tier._ownItems == null) continue;

                foreach (CollectibleData item in tier._ownItems)
                {
                    if (item == null) continue;

                    if (!_dropPoolByGrade.TryGetValue(item.Grade, out List<CollectibleData> list))
                    {
                        list = new List<CollectibleData>();
                        _dropPoolByGrade[item.Grade] = list;
                    }

                    list.Add(item);

                    if (tier == this && _ownGrade == null)
                    {
                        _ownGrade = item.Grade;
                    }
                }
            }
        }
    }
}
