using CleanPlanet.Core.Robot;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 로봇 배터리 잔량을 채움 바와 퍼센트 텍스트로 표시하는 HUD.
    /// RobotBattery가 정적이라 GameScene/BaseScene 어디에 둬도 동일하게 동작한다.
    /// </summary>
    public sealed class BatteryHudView : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Text _label;

        private void OnEnable()
        {
            if (_fillImage == null || _label == null)
            {
                Debug.LogError($"{nameof(BatteryHudView)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            UpdateDisplay(RobotBattery.Charge);
            RobotBattery.ChargeChanged += UpdateDisplay;
        }

        private void OnDisable()
        {
            RobotBattery.ChargeChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(float charge)
        {
            float ratio = Mathf.Clamp01(charge / RobotBattery.MaxCharge);
            _fillImage.fillAmount = ratio;
            _label.text = $"{Mathf.RoundToInt(ratio * 100f)}%";
        }
    }
}
