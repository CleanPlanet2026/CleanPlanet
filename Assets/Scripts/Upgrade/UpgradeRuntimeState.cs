using System;
using System.Collections.Generic;

namespace CleanPlanet.Upgrade
{
    public sealed class UpgradeRuntimeState
    {
        private readonly Dictionary<string, int> _levels = new();

        public event Action<string, int> LevelChanged;

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
            LevelChanged?.Invoke(upgradeId, nextLevel);
            return true;
        }
    }
}
