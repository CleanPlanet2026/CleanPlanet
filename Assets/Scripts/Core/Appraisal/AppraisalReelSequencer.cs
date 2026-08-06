using System.Collections;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 슬롯 릴의 연출만 담당하는 뷰. 실제 감정 계산과 지급 타이밍은 AppraisalService가
    /// 씬과 무관하게 소유하며, 이 시퀀서는 서비스가 발행하는 OnAppraisalStarted를 받아
    /// 릴을 돌릴 뿐 결과 계산이나 지급에는 관여하지 않는다. 릴이 화면에 보일 때만(감정 탭)
    /// 스핀하고, 숨겨진 동안(업그레이드 탭 등)에는 서비스 페이스대로 골드만 조용히 오르며,
    /// 다시 감정 탭으로 돌아오면 진행 중이던 감정의 남은 시간만큼 스핀을 재시작해
    /// 아이콘·사운드를 복원한다.
    /// </summary>
    public sealed class AppraisalReelSequencer : MonoBehaviour
    {
        [SerializeField] private AppraisalDisplay _display;

        [SerializeField, Min(0f)] private float _leftSpinDuration = 1f;
        [SerializeField, Min(0f)] private float _rightSpinDuration = 3f;

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _itemEnterSfx;

        private Coroutine _visualCoroutine;

        private void Awake()
        {
            if (_display == null)
            {
                Debug.LogError($"{nameof(AppraisalReelSequencer)}에 필요한 참조가 없습니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            AppraisalService.OnAppraisalStarted += HandleAppraisalStarted;
        }

        private void OnDisable()
        {
            AppraisalService.OnAppraisalStarted -= HandleAppraisalStarted;
            StopVisual();
        }

        private void Start()
        {
            // 이 오브젝트가 활성화되기 전(다른 씬에 있었거나 방금 로드됐거나)에 이미
            // 서비스가 감정을 시작했다면 그 남은 시간만큼만 스핀을 이어 붙인다.
            if (AppraisalService.TryGetActiveAppraisal(out AppraisalResult activeResult, out float remaining))
            {
                BeginVisual(activeResult, remaining);
            }
        }

        private void HandleAppraisalStarted(CollectibleData item, AppraisalResult result, float duration)
        {
            // 수집물이 새로 감정에 들어오는 순간의 사운드. 화면이 보일 때만(감정 탭) 울리고,
            // 씬 로드 복원(Start)이나 탭 복귀 재연출은 새 진입이 아니므로 여기서 처리하지 않는다.
            if (_display.IsReady && _sfxSource != null && _itemEnterSfx != null)
            {
                _sfxSource.PlayOneShot(_itemEnterSfx);
            }

            BeginVisual(result, duration);
        }

        private void BeginVisual(AppraisalResult result, float windowDuration)
        {
            StopVisual();
            _visualCoroutine = StartCoroutine(RunVisual(result, windowDuration));
        }

        private void StopVisual()
        {
            if (_visualCoroutine != null)
            {
                StopCoroutine(_visualCoroutine);
                _visualCoroutine = null;
            }
        }

        private IEnumerator RunVisual(AppraisalResult result, float windowDuration)
        {
            float elapsed = 0f;
            bool wasReady = _display.IsReady;

            if (wasReady)
            {
                _display.BeginAppraisal(result,
                    Mathf.Min(_leftSpinDuration, windowDuration),
                    Mathf.Min(_rightSpinDuration, windowDuration));
            }

            while (elapsed < windowDuration)
            {
                bool ready = _display.IsReady;

                // 숨겼다가 감정 탭으로 다시 돌아오면 릴이 비활성화되며 셀이 숨겨져 빈
                // 릴처럼 보인다. 이 전환을 감지해 남은 시간만큼 스핀을 재시작하여
                // 진행 중이던 아이콘·사운드를 복원한다.
                if (ready && !wasReady)
                {
                    float remaining = windowDuration - elapsed;
                    _display.BeginAppraisal(result,
                        Mathf.Min(_leftSpinDuration, remaining),
                        Mathf.Min(_rightSpinDuration, remaining));
                }

                wasReady = ready;
                elapsed += Time.deltaTime;
                yield return null;
            }

            _visualCoroutine = null;
        }
    }
}
