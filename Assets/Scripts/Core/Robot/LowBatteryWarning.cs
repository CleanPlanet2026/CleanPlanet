using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// 배터리가 소진되면 경고 패널을 잠시 보여준 뒤 베이스로 복귀한다.
    /// </summary>
    public sealed class LowBatteryWarning : MonoBehaviour
    {
        [SerializeField] private GameObject _warningPanel;
        [SerializeField] private string _baseSceneName = "BaseScene";
        [SerializeField, Min(0f)] private float _warningDuration = 2f;

        private void OnEnable()
        {
            if (_warningPanel == null)
            {
                Debug.LogError($"{nameof(LowBatteryWarning)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _warningPanel.SetActive(false);
            RobotBattery.Depleted += HandleDepleted;
        }

        private void OnDisable()
        {
            RobotBattery.Depleted -= HandleDepleted;
        }

        private void HandleDepleted()
        {
            StartCoroutine(ShowWarningThenReturn());
        }

        private IEnumerator ShowWarningThenReturn()
        {
            _warningPanel.SetActive(true);
            yield return new WaitForSeconds(_warningDuration);
            SceneManager.LoadScene(_baseSceneName);
        }
    }
}
