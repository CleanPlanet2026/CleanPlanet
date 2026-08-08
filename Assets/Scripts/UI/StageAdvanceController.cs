using System.Collections;
using CleanPlanet.Core;
using CleanPlanet.Core.Progress;
using CleanPlanet.Map.Procedural;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 현재 스테이지의 Earth Clean 목표치를 넘으면 버튼을 노출한다. 클릭하면 씬 전환 없이
    /// 화면을 잠깐 가린 뒤 다음 StageConfig로 GameScene의 맵을 다시 생성한다.
    /// </summary>
    public sealed class StageAdvanceController : MonoBehaviour
    {
        [SerializeField] private ProceduralMapGenerator _mapGenerator;
        [SerializeField] private StageConfig[] _stages;
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _buttonGroup;
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField, Min(0f)] private float _coverDuration = 1f;

        private bool _isTransitioning;

        private void OnEnable()
        {
            EarthCleanMeter.EarthCleanChanged += HandleEarthCleanChanged;
            _button.onClick.AddListener(HandleButtonClicked);
            SetOverlayVisible(false);
            UpdateButtonVisibility();
        }

        private void OnDisable()
        {
            EarthCleanMeter.EarthCleanChanged -= HandleEarthCleanChanged;
            _button.onClick.RemoveListener(HandleButtonClicked);
        }

        private void HandleEarthCleanChanged(float _)
        {
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            bool canAdvance = !_isTransitioning
                && HasNextStage(out StageConfig currentStage)
                && EarthCleanMeter.EarthClean >= currentStage.CleanGoalToUnlockNext;

            SetButtonVisible(canAdvance);
        }

        private bool HasNextStage(out StageConfig currentStage)
        {
            currentStage = null;
            int index = StageSessionState.SelectedStageIndex;
            if (_stages == null || index < 0 || index >= _stages.Length) return false;

            currentStage = _stages[index];
            return index + 1 < _stages.Length;
        }

        private void HandleButtonClicked()
        {
            if (_isTransitioning) return;
            StartCoroutine(AdvanceStage());
        }

        private IEnumerator AdvanceStage()
        {
            _isTransitioning = true;
            SetButtonVisible(false);
            SetOverlayVisible(true);

            yield return new WaitForSeconds(_coverDuration);

            StageSessionState.SelectedStageIndex++;
            _mapGenerator.RegenerateMap();

            SetOverlayVisible(false);
            _isTransitioning = false;
            UpdateButtonVisibility();
        }

        private void SetButtonVisible(bool visible)
        {
            _buttonGroup.alpha = visible ? 1f : 0f;
            _buttonGroup.interactable = visible;
            _buttonGroup.blocksRaycasts = visible;
        }

        private void SetOverlayVisible(bool visible)
        {
            _overlay.alpha = visible ? 1f : 0f;
            _overlay.blocksRaycasts = visible;
        }
    }
}
