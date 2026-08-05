using CleanPlanet.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// 플레이어가 직접 탐색을 중단할 때(키 입력 또는 3초 이상 눌러야 하는 UI 버튼)
    /// 베이스 복귀로 연결한다. 배터리 소진으로 인한 자동 복귀는 경고 UI를 먼저 보여줘야
    /// 하므로 LowBatteryWarning이 별도로 처리한다.
    /// </summary>
    public sealed class ExplorationReturnTrigger : MonoBehaviour
    {
        [SerializeField] private string _baseSceneName = "BaseScene";
        [SerializeField] private string _stopKeyBinding = "<Keyboard>/r";
        [SerializeField] private HoldToConfirmButton _stopButton;

        private InputAction _stopAction;

        private void Awake()
        {
            _stopAction = new InputAction(binding: _stopKeyBinding);
            _stopAction.performed += OnStopPerformed;
        }

        private void OnEnable()
        {
            _stopAction.Enable();

            if (_stopButton != null)
            {
                _stopButton.Confirmed += ReturnToBase;
            }
        }

        private void OnDisable()
        {
            _stopAction.Disable();

            if (_stopButton != null)
            {
                _stopButton.Confirmed -= ReturnToBase;
            }
        }

        private void OnDestroy()
        {
            _stopAction.performed -= OnStopPerformed;
            _stopAction.Dispose();
        }

        private void OnStopPerformed(InputAction.CallbackContext context)
        {
            ReturnToBase();
        }

        /// <summary>UI 수집 중단 버튼의 OnClick에 직접 연결할 수 있다.</summary>
        public void ReturnToBase()
        {
            SceneManager.LoadScene(_baseSceneName);
        }
    }
}
