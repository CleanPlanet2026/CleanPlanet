using System;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// 로봇 배터리 잔량을 씬 전환 간에도 유지하는 정적 보관소.
    /// GameScene의 소모와 BaseScene의 충전이 같은 값을 공유한다.
    /// </summary>
    public static class RobotBattery
    {
        public const float MaxCharge = 100f;

        private static float _charge = MaxCharge;

        public static event Action<float> ChargeChanged;
        public static event Action Depleted;

        public static float Charge => _charge;
        public static bool IsFull => _charge >= MaxCharge;

        public static void Drain(float amount)
        {
            if (amount <= 0f || _charge <= 0f)
            {
                return;
            }

            _charge = Math.Max(0f, _charge - amount);
            ChargeChanged?.Invoke(_charge);

            if (_charge <= 0f)
            {
                Depleted?.Invoke();
            }
        }

        public static void Recharge(float amount)
        {
            if (amount <= 0f || _charge >= MaxCharge)
            {
                return;
            }

            _charge = Math.Min(MaxCharge, _charge + amount);
            ChargeChanged?.Invoke(_charge);
        }
    }
}
