using CleanPlanet.Core.Appraisal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 각 티어 이펙트를 버튼 클릭으로 즉시 재생해보는 디버그 패널.
    /// 실제 감정 흐름을 거치지 않고 AppraisalEffectDirector.DebugAddPayout으로
    /// 표본 payout을 직접 주입해 티어 판정·연출만 확인한다.
    /// </summary>
    public sealed class AppraisalDebugPanel : MonoBehaviour
    {
        [SerializeField] private AppraisalEffectDirector _director;
        [SerializeField] private AppraisalTank _tank;
        [SerializeField] private int[] _tierSamplePayouts = { 20, 160, 600, 2400, 4800, 9600 };
        [SerializeField] private Button[] _tierButtons;
        [SerializeField] private Button _clearTankButton;

        private UnityAction[] _tierButtonHandlers;
        private UnityAction _clearHandler;

        private void OnEnable()
        {
            if (_director == null || _tank == null || _tierButtons == null)
            {
                Debug.LogError($"{nameof(AppraisalDebugPanel)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            int sampleCount = _tierSamplePayouts != null ? _tierSamplePayouts.Length : 0;
            int wireCount = Mathf.Min(_tierButtons.Length, sampleCount);

            if (_tierButtons.Length != sampleCount)
            {
                Debug.LogWarning($"{nameof(AppraisalDebugPanel)}: 버튼 수({_tierButtons.Length})와 " +
                    $"표본 payout 수({sampleCount})가 달라 {wireCount}개만 배선합니다.", this);
            }

            _tierButtonHandlers = new UnityAction[_tierButtons.Length];

            for (int i = 0; i < wireCount; i++)
            {
                if (_tierButtons[i] == null)
                {
                    continue;
                }

                int payout = _tierSamplePayouts[i];
                UnityAction handler = () => _director.DebugAddPayout(payout);
                _tierButtonHandlers[i] = handler;
                _tierButtons[i].onClick.AddListener(handler);
            }

            if (_clearTankButton != null)
            {
                _clearHandler = _tank.Clear;
                _clearTankButton.onClick.AddListener(_clearHandler);
            }
        }

        private void OnDisable()
        {
            if (_tierButtons != null && _tierButtonHandlers != null)
            {
                for (int i = 0; i < _tierButtons.Length; i++)
                {
                    if (_tierButtons[i] != null && _tierButtonHandlers[i] != null)
                    {
                        _tierButtons[i].onClick.RemoveListener(_tierButtonHandlers[i]);
                    }
                }
            }

            if (_clearTankButton != null && _clearHandler != null)
            {
                _clearTankButton.onClick.RemoveListener(_clearHandler);
            }
        }
    }
}
