namespace CleanPlanet.Upgrade
{
    public static class UpgradeEffects
    {
        private const string MovementCalibrationId = "movement_calibration";
        private const string MovementWheelsId = "movement_wheels";
        private const string MovementTurboId = "movement_turbo";
        private const string BatterySavingId = "battery_saving";
        private const string BatteryFastChargeId = "battery_fast_charge";
        private const string BatteryHighDensityId = "battery_high_density";
        private const string AppraisalSensorId = "appraisal_sensor";
        private const string AppraisalPatternLearningId = "appraisal_pattern_learning";
        private const string AppraisalRareDetectionId = "appraisal_rare_detection";
        private const string CollectionGripId = "collection_grip";
        private const string CollectionScannerId = "collection_storage";
        private const string CollectionRecoveryId = "collection_recovery";

        public static float MovementSpeedMultiplier =>
            1f
            + GetPurchasedBonus(MovementCalibrationId, 0.1f)
            + GetPurchasedBonus(MovementWheelsId, 0.2f)
            + GetPurchasedBonus(MovementTurboId, 0.4f);

        public static float BatteryDrainMultiplier =>
            IsPurchased(BatterySavingId) ? 1f / 1.2f : 1f;

        public static float BatteryChargeMultiplier =>
            IsPurchased(BatteryFastChargeId) ? 1f / 0.7f : 1f;

        public static float BatteryCapacityMultiplier =>
            IsPurchased(BatteryHighDensityId) ? 1.5f : 1f;

        public static float AppraisalHighMultiplierChanceBonus =>
            GetPurchasedBonus(AppraisalSensorId, 0.04f)
            + GetPurchasedBonus(AppraisalPatternLearningId, 0.07f);

        public static float AppraisalPayoutMultiplier =>
            IsPurchased(AppraisalRareDetectionId) ? 1.12f : 1f;

        public static float CollectionRewardMultiplier =>
            1f
            + GetPurchasedBonus(CollectionGripId, 0.2f)
            + GetPurchasedBonus(CollectionRecoveryId, 0.5f);

        public static float CollectionQteWidthMultiplier =>
            IsPurchased(CollectionScannerId) ? 1.35f : 1f;

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
