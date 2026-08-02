using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 결과를 3분할(감정물 아이콘 / x / 배수)로 표시하는 쓰기 전용 순수 뷰.
    /// 값을 스스로 계산하거나 구독하지 않고, 호출자가 넘긴 값을 그대로 표시한다.
    /// 왼쪽 아이콘은 랜덤 선택 없이 호출자가 넘긴 스프라이트를 그대로 세팅한다.
    /// 중앙 "x" 라벨은 씬에 고정 배치된 텍스트를 그대로 쓴다.
    /// </summary>
    public sealed class AppraisalDisplay : MonoBehaviour
    {
        [SerializeField] private Image _itemIconImage;
        [SerializeField] private Text _multiplierText;

        public void SetItemSprite(Sprite icon)
        {
            if (_itemIconImage == null)
            {
                return;
            }

            _itemIconImage.sprite = icon;
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
