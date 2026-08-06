using System;
using System.Collections.Generic;
using CleanPlanet.Upgrade;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 로직(배수 테이블·소요 시간·수집물 카탈로그)을 담은 설정 애셋.
    /// 씬과 무관하게 백그라운드로 동작하는 AppraisalService가 이 값을 읽어 감정을
    /// 계산하므로, Project Settings의 Preloaded Assets에 등록해 씬 참조 없이도
    /// <see cref="Instance"/>로 접근할 수 있게 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "AppraisalConfig", menuName = "CleanPlanet/Appraisal/Appraisal Config")]
    public sealed class AppraisalConfig : ScriptableObject
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

        [Tooltip("감정 한 건이 시작해서 지급되기까지 걸리는 시간(초). 릴 연출이 보이지 않아도 이 속도로 진행된다.")]
        [SerializeField, Min(0f)] private float _appraisalDuration = 3f;

        [Tooltip("지급 직후 다음 감정을 시작하기 전 대기하는 시간(초).")]
        [SerializeField, Min(0f)] private float _resultHoldDuration = 0.5f;

        [Tooltip("CollectionInbox에 저장된 id(PersistenceId)를 CollectibleData로 되돌리기 위한 카탈로그.")]
        [SerializeField] private CollectibleData[] _collectibleCatalog;

        /// <summary>
        /// Preloaded Assets(Project Settings)에 등록된 인스턴스를 가리키는 정적 접근자.
        /// 씬 로드와 무관하게 애셋이 메모리에 올라오는 시점에 OnEnable에서 스스로 등록한다.
        /// </summary>
        public static AppraisalConfig Instance { get; private set; }

        public float AppraisalDuration => _appraisalDuration;
        public float ResultHoldDuration => _resultHoldDuration;
        public IReadOnlyList<CollectibleData> Catalog => _collectibleCatalog;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

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

        /// <summary>
        /// 수집물 하나를 감정해 배수 추첨과 지급액 계산을 한 번에 수행한다.
        /// AppraisalService가 씬 없이 헤드리스로 호출하는 순수 계산 진입점이다.
        /// </summary>
        public AppraisalResult Appraise(CollectibleData item)
        {
            int multiplier = PickMultiplier();
            int payout = Mathf.RoundToInt(
                item.BaseValue * multiplier * UpgradeEffects.AppraisalPayoutMultiplier);
            return new AppraisalResult(item, multiplier, payout);
        }

        private int PickMultiplier()
        {
            if (_multiplierTable == null || _multiplierTable.Count == 0)
            {
                Debug.LogWarning($"{nameof(AppraisalConfig)}: 배수 테이블이 비어 있어 기본 배수 1을 사용합니다.", this);
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
                Debug.LogWarning($"{nameof(AppraisalConfig)}: 배수 테이블의 총 가중치가 0이라 기본 배수 1을 사용합니다.", this);
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
