using System;
using CleanPlanet.Core.Persistence;
using UnityEngine;

namespace CleanPlanet.Core.Currency
{
    /// <summary>
    /// 골드를 직접 소유·저장하는 단일 소유자. 다른 시스템은 이벤트로만 구독하고
    /// 재화 값을 직접 갱신하지 않는다. AppraisalService처럼 씬 인스턴스 없이 지급해야
    /// 하는 시스템을 위해 정적 지급 경로(AddStatic)와 정적 이벤트를 함께 제공하며,
    /// 각 씬의 CurrencyWallet 인스턴스는 그 정적 이벤트를 인스턴스 이벤트로 중계하는
    /// 어댑터 역할을 겸해 기존 구독자(CurrencyHudView 등)는 그대로 동작한다.
    /// </summary>
    public sealed class CurrencyWallet : MonoBehaviour
    {
        private static int _gold;

        public static event Action<int> GoldChangedStatic;
        public static event Action<int> GoldAddedStatic;

        public event Action<int> GoldChanged;
        public event Action<int> GoldAdded;

        public int Gold => _gold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            _gold = Mathf.Max(0, GameSaveSystem.Data.Gold);
            GoldChangedStatic = null;
            GoldAddedStatic = null;
        }

        private void OnEnable()
        {
            GoldChangedStatic += HandleGoldChangedStatic;
            GoldAddedStatic += HandleGoldAddedStatic;
        }

        private void OnDisable()
        {
            GoldChangedStatic -= HandleGoldChangedStatic;
            GoldAddedStatic -= HandleGoldAddedStatic;
        }

        private void HandleGoldChangedStatic(int gold) => GoldChanged?.Invoke(gold);

        private void HandleGoldAddedStatic(int amount) => GoldAdded?.Invoke(amount);

        public void Add(int amount) => AddStatic(amount);

        /// <summary>
        /// 씬 인스턴스 없이 골드를 지급하는 정적 경로. 인스턴스의 Add도 내부적으로
        /// 이 메서드를 호출해 지급 로직을 한 곳에 둔다.
        /// </summary>
        public static void AddStatic(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _gold += amount;
            SaveGold();
            GoldChangedStatic?.Invoke(_gold);
            GoldAddedStatic?.Invoke(amount);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || _gold < amount)
            {
                return false;
            }

            if (amount == 0)
            {
                return true;
            }

            _gold -= amount;
            SaveGold();
            GoldChanged?.Invoke(_gold);
            return true;
        }

        private static void SaveGold()
        {
            GameSaveSystem.Data.Gold = _gold;
            GameSaveSystem.MarkDirty();
        }

        internal static void ResetProgress()
        {
            _gold = 0;
        }
    }
}
