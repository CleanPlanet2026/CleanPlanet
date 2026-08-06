using UnityEngine;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// GameScene에 있는 동안 매 프레임 배터리를 소모한다. 소진되면 RobotBattery.Depleted가
    /// 발행되고, 이를 구독한 ExplorationReturnTrigger가 베이스로 복귀시킨다.
    /// </summary>
    public sealed class BatteryDrainer : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _drainPerSecond = 2f;

        private void Update()
        {
            RobotBattery.Drain(_drainPerSecond * Time.deltaTime);
        }
    }
}
