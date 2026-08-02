using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 감정 지급액(payout) 구간별 연출 프리셋 6종을 담는 애셋. AppraisalEffectDirector가
    /// 이 애셋을 참조해 판정과 연출을 수행하므로, 코인 수·배너 문구 등을 바꿀 때 씬의
    /// Director를 건드리지 않고 이 애셋만 수정하면 된다.
    /// </summary>
    [CreateAssetMenu(fileName = "AppraisalTierTable", menuName = "CleanPlanet/Appraisal/Tier Table")]
    public sealed class AppraisalTierTable : ScriptableObject
    {
        [SerializeField] private List<AppraisalTier> _tiers;

        public IReadOnlyList<AppraisalTier> Tiers => _tiers;

        /// <summary>
        /// minPayout이 payout 이하인 티어 중 minPayout이 가장 높은 것을 반환한다.
        /// 해당하는 티어가 없으면 null.
        /// </summary>
        public AppraisalTier DetermineTier(int payout)
        {
            AppraisalTier best = null;

            foreach (AppraisalTier tier in _tiers)
            {
                if (tier.MinPayout <= payout && (best == null || tier.MinPayout > best.MinPayout))
                {
                    best = tier;
                }
            }

            return best;
        }

        private void Reset()
        {
            _tiers = new List<AppraisalTier>
            {
                new AppraisalTier(minPayout: 0, coinCount: 2, duration: 1f, shakeIntensity: 0f,
                    flash: false, bannerText: "", coinScale: 1f, explode: false),
                new AppraisalTier(minPayout: 100, coinCount: 6, duration: 1f, shakeIntensity: 0f,
                    flash: false, bannerText: "", coinScale: 1f, explode: false),
                new AppraisalTier(minPayout: 500, coinCount: 20, duration: 2f, shakeIntensity: 0f,
                    flash: false, bannerText: "", coinScale: 1f, explode: true),
                new AppraisalTier(minPayout: 1000, coinCount: 60, duration: 2f, shakeIntensity: 0.1f,
                    flash: false, bannerText: "대박!", coinScale: 1f, explode: true),
                new AppraisalTier(minPayout: 3000, coinCount: 150, duration: 10f, shakeIntensity: 0.2f,
                    flash: false, bannerText: "초대박!!", coinScale: 1.2f, explode: true),
                new AppraisalTier(minPayout: 7000, coinCount: 400, duration: 10f, shakeIntensity: 0.35f,
                    flash: true, bannerText: "잭팟!!!", coinScale: 1.5f, explode: true),
            };
        }
    }
}
