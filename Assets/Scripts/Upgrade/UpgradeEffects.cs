namespace CleanPlanet.Upgrade
{
    public static class UpgradeEffects
    {
        private const string MovementCalibrationId = "movement_calibration";
        private const string MovementWheelsId = "movement_wheels";
        private const string MovementTurboId = "movement_turbo";
        private const string BatterySavingId = "battery_saving";
        private const string BatteryFastChargeId = "battery_fast_charge";

        public static float MovementSpeedMultiplier =>
            1f
            + GetPurchasedBonus(MovementCalibrationId, 0.1f)
            + GetPurchasedBonus(MovementWheelsId, 0.2f)
            + GetPurchasedBonus(MovementTurboId, 0.4f);

        public static float BatteryDrainMultiplier =>
            IsPurchased(BatterySavingId) ? 1f / 1.2f : 1f;

        public static float BatteryChargeMultiplier =>
            IsPurchased(BatteryFastChargeId) ? 1f / 0.7f : 1f;

        private static float GetPurchasedBonus(string upgradeId, float bonus)
        {
            return IsPurchased(upgradeId) ? bonus : 0f;
        }

        private static bool IsPurchased(string upgradeId)
        {
            return UpgradeRuntimeState.Shared.GetLevel(upgradeId) > 0;
        }
    }
}
