using System;
using System.Collections;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 슬롯 릴 연출을 순차 진행하는 시퀀서. 큐에 담긴 수집물을 한 건씩,
    /// 한 건이 끝나야 다음 건을 시작하는 방식으로 처리한다(큐 소진 시 정지, 순환 없음).
    /// 각 건의 실제 결과는 AppraisalCore.Appraise로 미리 확정하고, 릴 스핀 동안 보여주는
    /// 왼쪽 이름/오른쪽 배수는 연출용 코스메틱일 뿐 이 값에 영향을 주지 않는다.
    /// </summary>
    public sealed class AppraisalReelSequencer : MonoBehaviour
    {
        [SerializeField] private AppraisalCore _appraisalCore;
        [SerializeField] private AppraisalDisplay _display;
        [SerializeField] private AppraisalItem[] _items;

        [SerializeField, Min(0f)] private float _leftSpinDuration = 1f;
        [SerializeField, Min(0f)] private float _rightSpinDuration = 3f;
        [SerializeField, Min(0.01f)] private float _spinTickInterval = 0.05f;
        [SerializeField, Min(0f)] private float _resultHoldDuration = 0.5f;

        public event Action<AppraisalResult> OnPayoutConfirmed;

        private Coroutine _sequenceCoroutine;

        private void Start()
        {
            if (_appraisalCore == null || _display == null || _items == null || _items.Length == 0)
            {
                Debug.LogError($"{nameof(AppraisalReelSequencer)}에 필요한 참조 또는 샘플 큐가 없습니다.", this);
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
            foreach (var item in _items)
            {
                yield return RunOneAppraisal(item);
            }

            _sequenceCoroutine = null;
        }

        private IEnumerator RunOneAppraisal(AppraisalItem item)
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
                        _display.SetItemName(GetRandomDecoyItemName());
                    }
                    else
                    {
                        _display.SetItemName(result.Item.Name);
                        leftLocked = true;
                    }
                }

                _display.SetMultiplier(GetRandomDecoyMultiplier().ToString());

                yield return tick;
                elapsed += _spinTickInterval;
            }

            // 최종값 보장: 누적 오차나 지속 시간 0 설정과 무관하게 실제 값으로 강제 고정.
            _display.SetItemName(result.Item.Name);
            _display.SetMultiplier(result.Multiplier.ToString());

            OnPayoutConfirmed?.Invoke(result);

            yield return new WaitForSeconds(_resultHoldDuration);
        }

        private string GetRandomDecoyItemName()
        {
            return _items[UnityEngine.Random.Range(0, _items.Length)].Name;
        }

        private static int GetRandomDecoyMultiplier()
        {
            // Core의 가중치 테이블과 무관한 순수 코스메틱 디코이 숫자.
            return UnityEngine.Random.Range(1, 17);
        }
    }
}
