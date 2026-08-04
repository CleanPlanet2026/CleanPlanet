using CleanPlanet.Core.Currency;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 보유 골드를 표시하는 HUD. 지갑의 변경 이벤트를 받아 실제 잔액과 표시값을 동기화한다.
    /// </summary>
    public sealed class CurrencyHudView : MonoBehaviour
    {
        [SerializeField] private CurrencyWallet _wallet;
        [SerializeField] private Text _label;

        private int _displayedGold;

        private void OnEnable()
        {
            if (_wallet == null || _label == null)
            {
                Debug.LogError($"{nameof(CurrencyHudView)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _displayedGold = _wallet.Gold;
            UpdateLabel(_displayedGold);
            _wallet.GoldChanged += HandleGoldChanged;
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.GoldChanged -= HandleGoldChanged;
            }
        }

        private void HandleGoldChanged(int gold)
        {
            _displayedGold = gold;
            UpdateLabel(_displayedGold);
        }

        private void UpdateLabel(int value)
        {
            _label.text = value.ToString("N0");
        }
    }
}
