using System.Collections;
using CleanPlanet.Core.Currency;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 보유 골드를 표시하는 HUD. 표시값을 직접 소유하지 않고, 초기값만 지갑에서
    /// 읽어 온다. 이후 카운트업 연출은 AppraisalEffectDirector가 PlayCountUp으로 지휘한다.
    /// </summary>
    public sealed class CurrencyHudView : MonoBehaviour
    {
        [SerializeField] private CurrencyWallet _wallet;
        [SerializeField] private Text _label;

        private Coroutine _countUpCoroutine;
        private int _displayedGold;

        private void OnEnable()
        {
            if (_wallet == null || _label == null)
            {
                Debug.LogError($"{nameof(CurrencyHudView)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _displayedGold = _wallet.Gold;
            UpdateLabel(_displayedGold);
            _wallet.GoldSpent += HandleGoldSpent;
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.GoldSpent -= HandleGoldSpent;
            }

            if (_countUpCoroutine != null)
            {
                StopCoroutine(_countUpCoroutine);
                _countUpCoroutine = null;
            }
        }

        private void HandleGoldSpent(int _)
        {
            PlayCountUp(_wallet.Gold, 0f);
        }

        /// <summary>
        /// 현재 표시값에서 target까지 duration 동안 카운트업한다. duration이 0 이하이면 즉시 반영한다.
        /// </summary>
        public void PlayCountUp(int target, float duration)
        {
            if (_countUpCoroutine != null)
            {
                StopCoroutine(_countUpCoroutine);
            }

            if (duration <= 0f)
            {
                _displayedGold = target;
                UpdateLabel(_displayedGold);
                return;
            }

            _countUpCoroutine = StartCoroutine(RunCountUp(target, duration));
        }

        private IEnumerator RunCountUp(int target, float duration)
        {
            int start = _displayedGold;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _displayedGold = Mathf.RoundToInt(Mathf.Lerp(start, target, t));
                UpdateLabel(_displayedGold);
                yield return null;
            }

            _displayedGold = target;
            UpdateLabel(_displayedGold);
            _countUpCoroutine = null;
        }

        private void UpdateLabel(int value)
        {
            _label.text = value.ToString("N0");
        }
    }
}
