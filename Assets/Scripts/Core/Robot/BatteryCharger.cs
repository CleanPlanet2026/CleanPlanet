using UnityEngine;
using CleanPlanet.Upgrade;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// BaseScene에 있는 동안 매 프레임 배터리를 충전한다.
    /// </summary>
    public sealed class BatteryCharger : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _rechargePerSecond = 10f;

        private void Update()
        {
            RobotBattery.Recharge(
                _rechargePerSecond * UpgradeEffects.BatteryChargeMultiplier * Time.deltaTime);
        }
    }
}
