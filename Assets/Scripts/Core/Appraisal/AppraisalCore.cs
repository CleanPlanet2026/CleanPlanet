using System;
using System.Collections.Generic;
using CleanPlanet.Upgrade;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 코어 로직. 수집물 하나를 받아 가중치 기반으로 배수를 추첨하고
    /// 지급 재화를 계산해 이벤트로만 알린다. 재화를 직접 소유·저장하지 않는다.
    /// </summary>
    public sealed class AppraisalCore : MonoBehaviour
    {
        [Serializable]
        private sealed class AppraisalMultiplierEntry
        {
            [SerializeField, Min(1)] private int _multiplier = 1;
            [SerializeField, Min(0f)] private float _weight = 1f;

            public int Multiplier => _multiplier;
            public float Weight => _weight;

            public AppraisalMultiplierEntry(int multiplier, float weight)
            {
                _multiplier = multiplier;
                _weight = weight;
            }
        }

        [SerializeField] private List<AppraisalMultiplierEntry> _multiplierTable;

        public event Action<AppraisalResult> OnAppraised;

        private void Reset()
        {
            // 낮은 배수일수록 흔하고 높은 배수일수록 희귀하게 — 인크리멘탈 게임 통상 분포.
            _multiplierTable = new List<AppraisalMultiplierEntry>
            {
                new(1, 50f),
                new(2, 25f),
                new(4, 15f),
                new(8, 7f),
                new(16, 3f)
            };
        }

        public AppraisalResult Appraise(CollectibleData item)
        {
            int multiplier = PickMultiplier();
            int payout = Mathf.RoundToInt(
                item.BaseValue * multiplier * UpgradeEffects.AppraisalPayoutMultiplier);
            var result = new AppraisalResult(item, multiplier, payout);

            OnAppraised?.Invoke(result);
            return result;
        }

        private int PickMultiplier()
        {
            if (_multiplierTable == null || _multiplierTable.Count == 0)
            {
                Debug.LogWarning($"{nameof(AppraisalCore)}: 배수 테이블이 비어 있어 기본 배수 1을 사용합니다.", this);
                return 1;
            }

            float totalWeight = 0f;
            float baseMultiplierWeight = 0f;
            float highMultiplierWeight = 0f;
            foreach (var entry in _multiplierTable)
            {
                totalWeight += entry.Weight;
                if (entry.Multiplier > 1)
                {
                    highMultiplierWeight += entry.Weight;
                }
                else
                {
                    baseMultiplierWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                Debug.LogWarning($"{nameof(AppraisalCore)}: 배수 테이블의 총 가중치가 0이라 기본 배수 1을 사용합니다.", this);
                return 1;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            float bonusWeight = Mathf.Min(
                totalWeight * UpgradeEffects.AppraisalHighMultiplierChanceBonus,
                baseMultiplierWeight);
            foreach (var entry in _multiplierTable)
            {
                cumulative += GetAdjustedWeight(
                    entry,
                    baseMultiplierWeight,
                    highMultiplierWeight,
                    bonusWeight);
                if (roll <= cumulative)
                {
                    return entry.Multiplier;
                }
            }

            return _multiplierTable[^1].Multiplier;
        }

        private static float GetAdjustedWeight(
            AppraisalMultiplierEntry entry,
            float baseMultiplierWeight,
            float highMultiplierWeight,
            float bonusWeight)
        {
            if (entry.Multiplier <= 1)
            {
                if (baseMultiplierWeight <= 0f)
                {
                    return entry.Weight;
                }

                return entry.Weight - bonusWeight * entry.Weight / baseMultiplierWeight;
            }

            if (highMultiplierWeight <= 0f)
            {
                return entry.Weight;
            }

            return entry.Weight + bonusWeight * entry.Weight / highMultiplierWeight;
        }
    }
}
