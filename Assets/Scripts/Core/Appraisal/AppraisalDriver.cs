using CleanPlanet.Core.Currency;
using CleanPlanet.Core.Collection;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 지급 골드를 지갑에 적립하는 커넥터.
    /// 실제 감정 진행은 AppraisalReelSequencer가 맡고, 이 컴포넌트는 그 결과를 구독만 한다.
    /// </summary>
    public sealed class AppraisalDriver : MonoBehaviour
    {
        [SerializeField] private AppraisalReelSequencer _sequencer;
        [SerializeField] private CurrencyWallet _wallet;

        private void OnEnable()
        {
            if (_sequencer == null || _wallet == null)
            {
                Debug.LogError($"{nameof(AppraisalDriver)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _sequencer.OnPayoutConfirmed += HandlePayoutConfirmed;
        }

        private void OnDisable()
        {
            if (_sequencer != null)
            {
                _sequencer.OnPayoutConfirmed -= HandlePayoutConfirmed;
            }
        }

        private void HandlePayoutConfirmed(AppraisalResult result)
        {
            CollectionInbox.Remove(result.Item);
            _wallet.Add(result.Payout);
        }
    }
}
