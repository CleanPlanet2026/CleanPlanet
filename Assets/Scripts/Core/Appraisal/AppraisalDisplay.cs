using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 결과를 왼쪽 아이콘 릴과 오른쪽 배수 릴 두 개로 나눠 스핀시키는 코디네이터.
    /// 릴 자체의 세로 스크롤·정착 연출은 각 AppraisalReel이 전담하고, 이 클래스는 하나의
    /// 감정 결과를 두 릴에 동시에 넘겨 시작시키고 진행 상태만 취합해 알려준다.
    /// </summary>
    public sealed class AppraisalDisplay : MonoBehaviour
    {
        [SerializeField] private AppraisalReel _iconReel;
        [SerializeField] private AppraisalReel _multiplierReel;

        public bool IsSpinning => (_iconReel != null && _iconReel.IsSpinning)
            || (_multiplierReel != null && _multiplierReel.IsSpinning);

        public void BeginAppraisal(AppraisalResult result, float leftDuration, float rightDuration)
        {
            if (_iconReel == null || _multiplierReel == null)
            {
                Debug.LogError($"{nameof(AppraisalDisplay)}에 릴 참조가 없습니다.", this);
                return;
            }

            _iconReel.SpinToIcon(result.Item.Icon, leftDuration);
            _multiplierReel.SpinToNumber(result.Multiplier, rightDuration);
        }
    }
}
