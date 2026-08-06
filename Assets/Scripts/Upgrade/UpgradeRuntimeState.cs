using System;
using System.Collections.Generic;
using CleanPlanet.Core.Persistence;

namespace CleanPlanet.Upgrade
{
    public sealed class UpgradeRuntimeState
    {
        public static UpgradeRuntimeState Shared { get; } = new();

        private readonly Dictionary<string, int> _levels = new();

        public event Action<string, int> LevelChanged;

        private UpgradeRuntimeState()
        {
            foreach (UpgradeSaveEntry entry in GameSaveSystem.Data.Upgrades)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.UpgradeId))
                {
                    _levels[entry.UpgradeId] = Math.Max(0, entry.Level);
                }
            }
        }

        public int GetLevel(string upgradeId, int initialLevel = 0)
        {
            if (_levels.TryGetValue(upgradeId, out int level))
            {
                return level;
            }

            _levels.Add(upgradeId, initialLevel);
            return initialLevel;
        }

        public bool TryUpgrade(string upgradeId, int initialLevel, int maxLevel)
        {
            int currentLevel = GetLevel(upgradeId, initialLevel);
            if (currentLevel >= maxLevel)
            {
                return false;
            }

            int nextLevel = currentLevel + 1;
            _levels[upgradeId] = nextLevel;
            SaveLevels();
            LevelChanged?.Invoke(upgradeId, nextLevel);
            return true;
        }

        private void SaveLevels()
        {
            GameSaveSystem.Data.Upgrades.Clear();
            foreach (KeyValuePair<string, int> entry in _levels)
            {
                GameSaveSystem.Data.Upgrades.Add(new UpgradeSaveEntry(entry.Key, entry.Value));
            }

            GameSaveSystem.MarkDirty();
        }

        internal void ResetProgress()
        {
            _levels.Clear();
        }
    }
}
