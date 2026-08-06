using System;
using CleanPlanet.Core.Persistence;
using CleanPlanet.Upgrade;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// 로봇 배터리 잔량을 씬 전환 간에도 유지하는 정적 보관소.
    /// GameScene의 소모와 BaseScene의 충전이 같은 값을 공유한다.
    /// </summary>
    public static class RobotBattery
    {
        public const float BaseMaxCharge = 100f;

        private static float _charge;

        public static event Action<float> ChargeChanged;
        public static event Action Depleted;

        public static float Charge => _charge;
        public static float MaxCharge => BaseMaxCharge * UpgradeEffects.BatteryCapacityMultiplier;
        public static bool IsFull => _charge >= MaxCharge;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadState()
        {
            _charge = Math.Min(MaxCharge, Math.Max(0f, GameSaveSystem.Data.BatteryCharge));
        }

        public static void Drain(float amount)
        {
            if (amount <= 0f || _charge <= 0f)
            {
                return;
            }

            _charge = Math.Max(0f, _charge - amount);
            SaveCharge();
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
            SaveCharge();
            ChargeChanged?.Invoke(_charge);
        }

        private static void SaveCharge()
        {
            GameSaveSystem.Data.BatteryCharge = _charge;
            GameSaveSystem.MarkDirty();
        }

        internal static void ResetProgress()
        {
            _charge = BaseMaxCharge;
        }
    }
}
