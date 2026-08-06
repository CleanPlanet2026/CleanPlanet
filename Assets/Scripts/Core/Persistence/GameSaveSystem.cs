using UnityEngine;
using CleanPlanet.Core.Collection;
using CleanPlanet.Core.Currency;
using CleanPlanet.Core.Robot;
using CleanPlanet.Upgrade;

namespace CleanPlanet.Core.Persistence
{
    public static class GameSaveSystem
    {
        private const string SaveKey = "CleanPlanet.GameSave";
        private static GameSaveData _data;
        private static bool _isDirty;

        public static GameSaveData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            EnsureLoaded();

            GameObject runnerObject = new("Game Save System");
            Object.DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<GameSaveRunner>();
        }

        public static void MarkDirty()
        {
            _isDirty = true;
        }

        public static void SaveNow()
        {
            EnsureLoaded();
            if (!_isDirty)
            {
                return;
            }

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
            _isDirty = false;
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            _data = new GameSaveData();
            _isDirty = false;
            CurrencyWallet.ResetProgress();
            CollectionInbox.ResetProgress();
            UpgradeRuntimeState.Shared.ResetProgress();
            RobotBattery.ResetProgress();
            GameSessionState.ResetProgress();
        }

        private static void EnsureLoaded()
        {
            if (_data != null)
            {
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                _data = new GameSaveData();
                return;
            }

            try
            {
                _data = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
                _data.PendingCollectibleIds ??= new System.Collections.Generic.List<string>();
                _data.Upgrades ??= new System.Collections.Generic.List<UpgradeSaveEntry>();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to load save data. A new save will be used. {exception.Message}");
                _data = new GameSaveData();
            }
        }

    }
}
