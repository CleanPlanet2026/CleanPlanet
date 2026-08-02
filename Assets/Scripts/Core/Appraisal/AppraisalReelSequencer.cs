using System;
using System.Collections;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 슬롯 릴 연출을 순차 진행하는 시퀀서. 큐에 담긴 수집물을 한 건씩,
    /// 한 건이 끝나야 다음 건을 시작하는 방식으로 처리한다(큐 소진 시 정지, 순환 없음).
    /// 각 건의 실제 결과는 AppraisalCore.Appraise로 미리 확정하고, 릴 스핀 동안 보여주는
    /// 왼쪽 아이콘/오른쪽 배수는 연출용 코스메틱일 뿐 이 값에 영향을 주지 않는다.
    /// </summary>
    public sealed class AppraisalReelSequencer : MonoBehaviour
    {
        [SerializeField] private AppraisalCore _appraisalCore;
        [SerializeField] private AppraisalDisplay _display;
        [SerializeField] private AppraisalTank _tank;
        [SerializeField] private Sprite[] _decoyIcons;

        [SerializeField, Min(0f)] private float _leftSpinDuration = 1f;
        [SerializeField, Min(0f)] private float _rightSpinDuration = 3f;
        [SerializeField, Min(0.01f)] private float _spinTickInterval = 0.05f;
        [SerializeField, Min(0f)] private float _resultHoldDuration = 0.5f;

        public event Action<AppraisalResult> OnPayoutConfirmed;

        private Coroutine _sequenceCoroutine;

        private void Start()
        {
            if (_appraisalCore == null || _display == null || _tank == null
                || _decoyIcons == null || _decoyIcons.Length == 0)
            {
                Debug.LogError($"{nameof(AppraisalReelSequencer)}에 필요한 참조 또는 디코이 아이콘이 없습니다.", this);
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

            var tick = new WaitForSeconds(_spinTickInterval);
            float elapsed = 0f;
            bool leftLocked = false;

            while (elapsed < _rightSpinDuration)
            {
                if (!leftLocked)
                {
                    if (elapsed < _leftSpinDuration)
                    {
                        _display.SetItemSprite(GetRandomDecoyIcon());
                    }
                    else
                    {
                        _display.SetItemSprite(result.Item.Icon);
                        leftLocked = true;
                    }
                }

                _display.SetMultiplier(GetRandomDecoyMultiplier().ToString());

                yield return tick;
                elapsed += _spinTickInterval;
            }

            // 최종값 보장: 누적 오차나 지속 시간 0 설정과 무관하게 실제 값으로 강제 고정.
            _display.SetItemSprite(result.Item.Icon);
            _display.SetMultiplier(result.Multiplier.ToString());

            OnPayoutConfirmed?.Invoke(result);

            yield return new WaitForSeconds(_resultHoldDuration);
        }

        private Sprite GetRandomDecoyIcon()
        {
            return _decoyIcons[UnityEngine.Random.Range(0, _decoyIcons.Length)];
        }

        private static int GetRandomDecoyMultiplier()
        {
            // Core의 가중치 테이블과 무관한 순수 코스메틱 디코이 숫자.
            return UnityEngine.Random.Range(1, 17);
        }
    }
}
