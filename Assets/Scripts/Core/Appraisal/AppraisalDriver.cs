using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 결과 확정 시 골드 지급 로그를 출력하는 임시 하네스.
    /// 실제 감정 진행은 AppraisalReelSequencer가 맡고, 이 컴포넌트는 그 결과를 구독만 한다.
    /// 정식 재화 시스템이 붙으면 대체된다.
    /// </summary>
    public sealed class AppraisalDriver : MonoBehaviour
    {
        [SerializeField] private AppraisalReelSequencer _sequencer;

        private void OnEnable()
        {
            if (_sequencer == null)
            {
                Debug.LogError($"{nameof(AppraisalDriver)}에 {nameof(AppraisalReelSequencer)} 참조가 없습니다.", this);
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
            Debug.Log($"[AppraisalDriver] {result.Item.Name}: 기본가치 {result.Item.BaseValue} → 배수 x{result.Multiplier} → 골드 {result.Payout}");
        }
    }
}
