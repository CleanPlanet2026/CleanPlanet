using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 결과를 3분할(감정물 / x / 배수)로 표시하는 텍스트 쓰기 전용 순수 뷰.
    /// 값을 스스로 계산하거나 구독하지 않고, 호출자가 넘긴 문자열을 그대로 표시한다.
    /// 중앙 "x" 라벨은 씬에 고정 배치된 텍스트를 그대로 쓴다.
    /// </summary>
    public sealed class AppraisalDisplay : MonoBehaviour
    {
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _multiplierText;

        public void SetItemName(string itemName)
        {
            if (_itemNameText == null)
            {
                return;
            }

            _itemNameText.text = itemName;
        }

        public void SetMultiplier(string multiplierText)
        {
            if (_multiplierText == null)
            {
                return;
            }

            _multiplierText.text = multiplierText;
        }
    }
}
