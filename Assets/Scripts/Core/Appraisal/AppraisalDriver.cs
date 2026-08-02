using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 코어 로직 검증용 임시 하네스. 빈 GameObject에 붙이고 Play하면
    /// 샘플 수집물을 감정해 Console에 결과를 찍는다.
    /// GridOccupancyTester와 같은 성격의 임시 코드 — 정식 흐름(재화 시스템 구독 등)이
    /// 붙으면 제거한다.
    /// </summary>
    public sealed class AppraisalDriver : MonoBehaviour
    {
        [SerializeField] private AppraisalCore _appraisalCore;

        [SerializeField]
        private AppraisalItem[] _sampleItems =
        {
            new("고철", ItemGrade.Common, 10),
            new("유리 조각", ItemGrade.Uncommon, 40),
            new("전자 부품", ItemGrade.Rare, 150),
            new("보석", ItemGrade.Epic, 600)
        };

        private void OnEnable()
        {
            if (_appraisalCore == null)
            {
                Debug.LogError($"{nameof(AppraisalDriver)}에 {nameof(AppraisalCore)} 참조가 없습니다.", this);
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

        private void Start()
        {
            AppraiseAllSamples();
        }

        [ContextMenu("Test: Re-Appraise All Samples")]
        private void AppraiseAllSamples()
        {
            foreach (var item in _sampleItems)
            {
                _appraisalCore.Appraise(item);
            }
        }

        private void HandleAppraised(AppraisalResult result)
        {
            Debug.Log($"[AppraisalDriver] {result.Item.Name}: 기본가치 {result.Item.BaseValue} → 배수 x{result.Multiplier} → 골드 {result.Payout}");
        }
    }
}
