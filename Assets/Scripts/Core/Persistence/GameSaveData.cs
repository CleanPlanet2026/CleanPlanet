using System;
using System.Collections.Generic;

namespace CleanPlanet.Core.Persistence
{
    [Serializable]
    public sealed class UpgradeSaveEntry
    {
        public string UpgradeId;
        public int Level;

        public UpgradeSaveEntry(string upgradeId, int level)
        {
            UpgradeId = upgradeId;
            Level = level;
        }
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public int Version = 1;
        public int Gold;
        public float BatteryCharge = 100f;
        public bool HasReachedBase;
        public float EarthClean;
        public List<string> PendingCollectibleIds = new();
        public List<UpgradeSaveEntry> Upgrades = new();
    }
}
