using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 결과를 3분할(감정물 / x / 배수)로 정적 표시하는 UI 컨트롤러.
    /// 애니메이션 없이 값만 즉시 갱신한다. 중앙 "x" 라벨은 씬에 고정 배치된 텍스트를 그대로 쓴다.
    /// </summary>
    public sealed class AppraisalDisplay : MonoBehaviour
    {
        [SerializeField] private AppraisalCore _appraisalCore;
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _multiplierText;

        private void OnEnable()
        {
            if (_appraisalCore == null || _itemNameText == null || _multiplierText == null)
            {
                Debug.LogError($"{nameof(AppraisalDisplay)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _appraisalCore.OnAppraised += HandleAppraised;
        }

        private void OnDisable()
        {
            if (_appraisalCore != null)
            {
                _appraisalCore.OnAppraised -= HandleAppraised;
            }
        }

        private void HandleAppraised(AppraisalResult result)
        {
            _itemNameText.text = result.Item.Name;
            _multiplierText.text = $"x{result.Multiplier}";
        }
    }
}
