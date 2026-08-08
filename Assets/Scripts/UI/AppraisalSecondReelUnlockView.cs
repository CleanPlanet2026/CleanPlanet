using CleanPlanet.Upgrade;
using UnityEngine;

namespace CleanPlanet.UI
{
    /// <summary>
    /// appraisal_second_reel 업그레이드 구매 여부에 맞춰 2번 감정 릴 UI를 켜고 끄는 작은 토글러.
    /// 구매 전에는 비활성 상태로 시작해 감정로봇 패널에 1번 릴만 보이게 하고, 구매 즉시(또는
    /// 다시 감정 탭으로 돌아올 때) 2번 릴을 활성화한다. 레인 배정 자체는 AppraisalService가
    /// UpgradeEffects.SecondAppraisalReelUnlocked를 직접 읽어 결정하므로, 이 컴포넌트는 순수
    /// 표시 전담이며 감정 진행에는 관여하지 않는다.
    /// </summary>
    public sealed class AppraisalSecondReelUnlockView : MonoBehaviour
    {
        [SerializeField] private GameObject _secondReelRoot;

        private void OnEnable()
        {
            if (_secondReelRoot == null)
            {
                Debug.LogError($"{nameof(AppraisalSecondReelUnlockView)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            ApplyState();
            UpgradeRuntimeState.Shared.LevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            UpgradeRuntimeState.Shared.LevelChanged -= HandleLevelChanged;
        }

        // 어떤 업그레이드가 바뀌었는지 가리지 않고 매번 재적용한다. 이미 활성인 상태에서
        // 다시 SetActive(true)를 호출해도 무해하므로, id 상수를 따로 노출할 필요가 없다.
        private void HandleLevelChanged(string upgradeId, int level) => ApplyState();

        private void ApplyState()
        {
            _secondReelRoot.SetActive(UpgradeEffects.SecondAppraisalReelUnlocked);
        }
    }
}
