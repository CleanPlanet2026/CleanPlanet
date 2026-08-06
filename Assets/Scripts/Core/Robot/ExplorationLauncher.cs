using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// 배터리가 기준치 이상 충전됐을 때만 GameScene으로 돌아가 다시 탐색할 수 있게 막는다.
    /// </summary>
    public sealed class ExplorationLauncher : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private string _launchKeyBinding = "<Keyboard>/r";
        [SerializeField, Min(0f)] private float _minChargeToExplore = RobotBattery.MaxCharge;
        [SerializeField] private Button _launchButton;

        private InputAction _launchAction;

        private void Awake()
        {
            _launchAction = new InputAction(binding: _launchKeyBinding);
            _launchAction.performed += OnLaunchPerformed;
        }

        private void OnEnable()
        {
            _launchAction.Enable();

            if (_launchButton != null)
            {
                _launchButton.onClick.AddListener(TryLaunchExploration);
            }
        }

        private void OnDisable()
        {
            _launchAction.Disable();

            if (_launchButton != null)
            {
                _launchButton.onClick.RemoveListener(TryLaunchExploration);
            }
        }

        private void OnDestroy()
        {
            _launchAction.performed -= OnLaunchPerformed;
            _launchAction.Dispose();
        }

        private void OnLaunchPerformed(InputAction.CallbackContext context)
        {
            TryLaunchExploration();
        }

        /// <summary>UI 탐색 시작 버튼의 OnClick에 직접 연결할 수 있다.</summary>
        public void TryLaunchExploration()
        {
            if (RobotBattery.Charge < _minChargeToExplore)
            {
                Debug.Log("[ExplorationLauncher] 배터리가 아직 충전되지 않아 탐색을 시작할 수 없습니다.");
                return;
            }

            SceneManager.LoadScene(_gameSceneName);
        }
    }
}
