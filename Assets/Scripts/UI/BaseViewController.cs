using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class BaseViewController : MonoBehaviour
    {
        [SerializeField] private GameObject _emotionRobotPanel;
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private Button _emotionRobotButton;
        [SerializeField] private Button _upgradeButton;

        private void OnEnable()
        {
            if (_emotionRobotPanel == null ||
                _upgradePanel == null ||
                _emotionRobotButton == null ||
                _upgradeButton == null)
            {
                Debug.LogError($"{nameof(BaseViewController)} requires all panel and button references.", this);
                enabled = false;
                return;
            }

            _emotionRobotButton.onClick.AddListener(ShowEmotionRobot);
            _upgradeButton.onClick.AddListener(ShowUpgrade);
            ShowEmotionRobot();
        }

        private void OnDisable()
        {
            if (_emotionRobotButton != null)
            {
                _emotionRobotButton.onClick.RemoveListener(ShowEmotionRobot);
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveListener(ShowUpgrade);
            }
        }

        private void ShowEmotionRobot()
        {
            SetActivePanel(showEmotionRobot: true);
        }

        private void ShowUpgrade()
        {
            SetActivePanel(showEmotionRobot: false);
        }

        private void SetActivePanel(bool showEmotionRobot)
        {
            _emotionRobotPanel.SetActive(showEmotionRobot);
            _upgradePanel.SetActive(!showEmotionRobot);
            _emotionRobotButton.interactable = !showEmotionRobot;
            _upgradeButton.interactable = showEmotionRobot;
        }
    }
}
