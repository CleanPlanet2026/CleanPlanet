using System.Collections;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Core.Currency;
using CleanPlanet.Utils;
using UnityEngine;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 지급액(payout)을 6개 티어 중 하나로 판정해 코인·사운드·화면흔들림·플래시·배너를
    /// 순서대로 지휘하는 연출 총괄. 감정 배수·payout 계산에는 관여하지 않고 그 결과를
    /// CurrencyWallet.GoldAdded로만 받아 연출한다. 티어 데이터는 AppraisalTierTable 애셋에
    /// 있으므로 코인 수·배너 문구 조정은 애셋만 수정하면 된다.
    /// </summary>
    public sealed class AppraisalEffectDirector : MonoBehaviour
    {
        [SerializeField] private AppraisalTierTable _tierTable;
        [SerializeField] private CurrencyWallet _wallet;
        [SerializeField] private CoinBurstSpawner _spawner;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private CameraShake _cameraShake;
        [SerializeField] private ScreenFlash _screenFlash;
        [SerializeField] private TierBanner _banner;
        [SerializeField] private AppraisalDisplay _display;
        [SerializeField, Min(0f)] private float _extraSfxDelay = 0.3f;
        [SerializeField, Min(0f)] private float _extraSfxVolume = 2f;

        private void OnEnable()
        {
            if (_wallet == null || _spawner == null || _audioSource == null
                || _tierTable == null || _tierTable.Tiers == null || _tierTable.Tiers.Count == 0)
            {
                Debug.LogError($"{nameof(AppraisalEffectDirector)}에 필요한 참조 또는 티어 목록이 없습니다.", this);
                enabled = false;
                return;
            }

            _wallet.GoldAdded += HandleGoldAdded;
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.GoldAdded -= HandleGoldAdded;
            }
        }

        /// <summary>
        /// 디버그 패널에서 임의 payout으로 티어 이펙트를 즉시 재생해보기 위한 진입점.
        /// 실제 payout 계산은 그대로 CurrencyWallet.Add를 거치므로 골드도 정상적으로 쌓인다.
        /// </summary>
        public void DebugAddPayout(int payout) => _wallet.Add(payout);

        private void HandleGoldAdded(int payout)
        {
            // 감정 화면이 보이지 않을 때(업그레이드 탭 등)는 골드만 오르고 연출은 생략한다.
            // 골드 값은 CurrencyWallet.Add의 GoldChanged로 이미 HUD에 반영된다.
            if (_display != null && !_display.IsReady)
            {
                return;
            }

            AppraisalTier tier = _tierTable.DetermineTier(payout);
            if (tier == null)
            {
                return;
            }

            PlayTier(tier);
        }

        private IEnumerator PlayExtraSfxDelayed(AudioClip clip)
        {
            if (_extraSfxDelay > 0f)
            {
                yield return new WaitForSeconds(_extraSfxDelay);
            }

            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(clip, _extraSfxVolume);
            }
        }

        private void PlayTier(AppraisalTier tier)
        {
            _spawner.Burst(tier.CoinCount, tier.Duration, tier.CoinScale, tier.Explode);

            if (tier.CoinSfx != null)
            {
                _audioSource.PlayOneShot(tier.CoinSfx);
            }

            if (tier.AccentSfx != null)
            {
                _audioSource.PlayOneShot(tier.AccentSfx);
            }

            if (tier.ExtraSfx != null)
            {
                // 코인·액센트가 동시에 크게 터지는 순간을 피해 살짝 늦게, 조금 크게 울려
                // 큰 소리에 묻히지 않게 한다.
                StartCoroutine(PlayExtraSfxDelayed(tier.ExtraSfx));
            }

            if (tier.ShakeIntensity > 0f && _cameraShake != null)
            {
                _cameraShake.Shake(tier.ShakeIntensity, tier.Duration);
            }

            if (tier.Flash && _screenFlash != null)
            {
                _screenFlash.Play();
            }

            if (!string.IsNullOrEmpty(tier.BannerText) && _banner != null)
            {
                _banner.Show(tier.BannerText);
            }
        }
    }
}
