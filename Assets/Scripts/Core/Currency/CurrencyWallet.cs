using System;
using UnityEngine;

namespace CleanPlanet.Core.Currency
{
    /// <summary>
    /// 골드를 직접 소유·저장하는 단일 소유자. 다른 시스템은 이벤트로만 구독하고
    /// 재화 값을 직접 갱신하지 않는다.
    /// </summary>
    public sealed class CurrencyWallet : MonoBehaviour
    {
        private static int _gold;

        public event Action<int> GoldChanged;
        public event Action<int> GoldAdded;

        public int Gold => _gold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            _gold = 0;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _gold += amount;
            GoldChanged?.Invoke(_gold);
            GoldAdded?.Invoke(amount);
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
            GoldChanged?.Invoke(_gold);
            return true;
        }
    }
}
