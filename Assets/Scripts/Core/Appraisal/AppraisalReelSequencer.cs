using System;
using System.Collections;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 슬롯 릴 연출을 순차 진행하는 시퀀서. 큐에 담긴 수집물을 한 건씩,
    /// 한 건이 끝나야 다음 건을 시작하는 방식으로 처리한다(큐 소진 시 정지, 순환 없음).
    /// 각 건의 실제 결과는 AppraisalCore.Appraise로 미리 확정하고, 릴이 스핀하는 동안
    /// 보여주는 중간 값은 각 AppraisalReel이 관리하는 연출용 코스메틱일 뿐이다.
    /// </summary>
    public sealed class AppraisalReelSequencer : MonoBehaviour
    {
        [SerializeField] private AppraisalCore _appraisalCore;
        [SerializeField] private AppraisalDisplay _display;
        [SerializeField] private AppraisalTank _tank;

        [SerializeField, Min(0f)] private float _leftSpinDuration = 1f;
        [SerializeField, Min(0f)] private float _rightSpinDuration = 3f;
        [SerializeField, Min(0f)] private float _resultHoldDuration = 0.5f;

        public event Action<AppraisalResult> OnPayoutConfirmed;

        private Coroutine _sequenceCoroutine;

        private void Start()
        {
            if (_appraisalCore == null || _display == null || _tank == null)
            {
                Debug.LogError($"{nameof(AppraisalReelSequencer)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _sequenceCoroutine = StartCoroutine(RunSequence());
        }

        private void OnDisable()
        {
            if (_sequenceCoroutine != null)
            {
                StopCoroutine(_sequenceCoroutine);
                _sequenceCoroutine = null;
            }
        }

        private IEnumerator RunSequence()
        {
            while (_tank.HasRemaining)
            {
                if (_tank.TryTakeBottomItem(out CollectibleData item))
                {
                    yield return RunOneAppraisal(item);
                }
                else
                {
                    // 집을 수 있는(바닥에 안정된) 아이콘이 아직 없을 뿐이므로 다음 프레임에 재시도.
                    yield return null;
                }
            }

            _sequenceCoroutine = null;
        }

        private IEnumerator RunOneAppraisal(CollectibleData item)
        {
            AppraisalResult result = _appraisalCore.Appraise(item);

            // 감정은 화면이 보이든(감정 탭) 안 보이든(업그레이드 탭) 항상 같은 속도로
            // 진행돼 자동으로 돈이 벌린다. 릴을 실제로 돌릴 수 있을 때만 스핀 연출을 켜고,
            // 숨겨져 있으면 스핀 대신 같은 시간만큼 기다려 지급 속도를 동일하게 유지한다.
            // (스핀 없이 즉시 지급되던 버그 방지)
            float spinTime = Mathf.Max(_leftSpinDuration, _rightSpinDuration);
            float elapsed = 0f;
            bool wasReady = _display.IsReady;

            if (wasReady)
            {
                _display.BeginAppraisal(result, _leftSpinDuration, _rightSpinDuration);
            }

            while (elapsed < spinTime)
            {
                bool ready = _display.IsReady;

                // 숨겼다가 감정 탭으로 다시 돌아오면 릴이 비활성화되며 셀이 숨겨져 빈
                // 릴처럼 보인다. 이 전환을 감지해 남은 시간만큼 스핀을 재시작하여
                // 진행 중이던 아이콘·사운드를 복원한다.
                if (ready && !wasReady)
                {
                    float remaining = spinTime - elapsed;
                    _display.BeginAppraisal(result,
                        Mathf.Min(_leftSpinDuration, remaining),
                        Mathf.Min(_rightSpinDuration, remaining));
                }

                wasReady = ready;
                elapsed += Time.deltaTime;
                yield return null;
            }

            OnPayoutConfirmed?.Invoke(result);

            yield return new WaitForSeconds(_resultHoldDuration);
        }
    }
}
