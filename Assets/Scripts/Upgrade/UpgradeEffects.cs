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
        private const string AppraisalSecondReelId = "appraisal_second_reel";
        private const string AppraisalSpeedId = "appraisal_speed";
        private const string CollectionGripId = "collection_grip";
        private const string CollectionScannerId = "collection_storage";
        private const string CollectionRecoveryId = "collection_recovery";
        private const string ExplorationWideScanId = "exploration_wide_scan";
        private const string ExplorationSignalBoostId = "exploration_signal_boost";
        private const string ExplorationPrecisionScanId = "exploration_precision_scan";

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

        /// <summary>두 번째 감정 레인(동시 처리)이 열렸는지 여부. AppraisalService가 매 폴마다 읽어 레인1 배정 여부를 결정한다.</summary>
        public static bool SecondAppraisalReelUnlocked => IsPurchased(AppraisalSecondReelId);

        /// <summary>감정 전체(진행 시간·릴 스핀·연출·사운드 pitch)에 곱해지는 속도 배수. 항상 1 이상이라 0나눗셈이 없다.</summary>
        public static float AppraisalSpeedMultiplier => IsPurchased(AppraisalSpeedId) ? 1.3f : 1f;

        public static float CollectionRewardMultiplier =>
            1f
            + GetPurchasedBonus(CollectionGripId, 0.2f)
            + GetPurchasedBonus(CollectionRecoveryId, 0.5f);

        public static float CollectionQteWidthMultiplier =>
            IsPurchased(CollectionScannerId) ? 1.35f : 1f;

        public static float ExplorationSpawnCountMultiplier =>
            IsPurchased(ExplorationWideScanId) ? 1.2f : 1f;

        public static float ExplorationHighTierWeightMultiplier =>
            IsPurchased(ExplorationSignalBoostId) ? 1.2f : 1f;

        public static float ExplorationOwnTierWeightMultiplier =>
            IsPurchased(ExplorationPrecisionScanId) ? 1.3f : 1f;

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
