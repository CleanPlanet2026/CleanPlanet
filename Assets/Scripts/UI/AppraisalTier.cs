using System;
using UnityEngine;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 감정 지급액(payout) 구간별 연출 프리셋 1건. minPayout 이상인 티어 중
    /// minPayout이 가장 높은 것이 적용되는 판정 규칙은 AppraisalTierTable이 담당한다.
    /// </summary>
    [Serializable]
    public sealed class AppraisalTier
    {
        [SerializeField] private int _minPayout;
        [SerializeField] private int _coinCount;
        [SerializeField] private float _duration;
        [SerializeField] private AudioClip _coinSfx;
        [SerializeField] private AudioClip _accentSfx;
        [SerializeField] private AudioClip _extraSfx;
        [SerializeField] private float _shakeIntensity;
        [SerializeField] private bool _flash;
        [SerializeField] private string _bannerText;
        [SerializeField] private float _coinScale = 1f;
        [SerializeField] private bool _explode;

        public AppraisalTier()
        {
        }

        /// <summary>
        /// AppraisalTierTable.Reset()이 표에 정의된 기본값을 채울 때 쓰는 생성자.
        /// AudioClip은 에셋 참조라 여기서 채울 수 없어 인스펙터에서 직접 배선한다.
        /// </summary>
        public AppraisalTier(int minPayout, int coinCount, float duration, float shakeIntensity,
            bool flash, string bannerText, float coinScale, bool explode)
        {
            _minPayout = minPayout;
            _coinCount = coinCount;
            _duration = duration;
            _shakeIntensity = shakeIntensity;
            _flash = flash;
            _bannerText = bannerText;
            _coinScale = coinScale;
            _explode = explode;
        }

        public int MinPayout => _minPayout;
        public int CoinCount => _coinCount;
        public float Duration => _duration;
        public AudioClip CoinSfx => _coinSfx;
        public AudioClip AccentSfx => _accentSfx;
        public AudioClip ExtraSfx => _extraSfx;
        public float ShakeIntensity => _shakeIntensity;
        public bool Flash => _flash;
        public string BannerText => _bannerText;
        public float CoinScale => _coinScale;
        public bool Explode => _explode;
    }
}
