using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class RobotProcessingPreview : MonoBehaviour
    {
        private static readonly string[] ItemNames =
        {
            "고철",
            "유리",
            "전자품",
            "보석"
        };

        private static readonly Color[] ItemColors =
        {
            new(0.55f, 0.60f, 0.66f),
            new(0.36f, 0.78f, 0.90f),
            new(0.96f, 0.62f, 0.20f),
            new(0.78f, 0.36f, 0.92f)
        };

        private static readonly int[] Multipliers = { 1, 2, 4, 8, 16 };
        private static readonly string[] MultiplierLabels = { "x1", "x2", "x4", "x8", "x16" };

        [SerializeField] private RectTransform _fallingItem;
        [SerializeField] private Image _fallingItemImage;
        [SerializeField] private Text _fallingItemLabel;
        [SerializeField] private RectTransform _dropStart;
        [SerializeField] private RectTransform _dropEnd;
        [SerializeField] private Text _currentItemText;
        [SerializeField] private Text _rouletteText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _goldGainText;
        [SerializeField, Min(0.1f)] private float _dropDuration = 1.8f;
        [SerializeField, Min(0.02f)] private float _rouletteStep = 0.1f;
        [SerializeField, Min(0f)] private float _resultDuration = 0.8f;

        private Coroutine _previewCoroutine;
        private int _itemIndex;
        private int _previewGold;

        private void OnEnable()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError($"{nameof(RobotProcessingPreview)} requires all UI references.", this);
                enabled = false;
                return;
            }

            ResetVisuals();
            _previewCoroutine = StartCoroutine(PlayPreview());
        }

        private void OnDisable()
        {
            if (_previewCoroutine != null)
            {
                StopCoroutine(_previewCoroutine);
                _previewCoroutine = null;
            }
        }

        private IEnumerator PlayPreview()
        {
            while (true)
            {
                int itemTypeIndex = _itemIndex % ItemNames.Length;
                int resultMultiplierIndex = _itemIndex % Multipliers.Length;
                PrepareItem(itemTypeIndex);

                float elapsed = 0f;
                float rouletteElapsed = 0f;
                int rouletteIndex = 0;

                while (elapsed < _dropDuration)
                {
                    elapsed += Time.deltaTime;
                    rouletteElapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / _dropDuration);
                    _fallingItem.anchoredPosition = Vector2.Lerp(
                        _dropStart.anchoredPosition,
                        _dropEnd.anchoredPosition,
                        progress);

                    if (rouletteElapsed >= _rouletteStep)
                    {
                        rouletteElapsed = 0f;
                        rouletteIndex = (rouletteIndex + 1) % MultiplierLabels.Length;
                        _rouletteText.text = MultiplierLabels[rouletteIndex];
                    }

                    yield return null;
                }

                CompleteConversion(itemTypeIndex, resultMultiplierIndex);
                yield return new WaitForSeconds(_resultDuration);
                _itemIndex++;
            }
        }

        private void PrepareItem(int itemTypeIndex)
        {
            string itemName = ItemNames[itemTypeIndex];
            _currentItemText.text = itemName;
            _fallingItemLabel.text = itemName;
            _fallingItemImage.color = ItemColors[itemTypeIndex];
            _fallingItem.anchoredPosition = _dropStart.anchoredPosition;
            _fallingItem.gameObject.SetActive(true);
            _goldGainText.text = string.Empty;
        }

        private void CompleteConversion(int itemTypeIndex, int multiplierIndex)
        {
            int gainedGold = (itemTypeIndex + 1) * 10 * Multipliers[multiplierIndex];
            _previewGold += gainedGold;
            _fallingItem.gameObject.SetActive(false);
            _rouletteText.text = MultiplierLabels[multiplierIndex];
            _goldGainText.text = $"+{gainedGold} 골드";
            _goldText.text = $"골드 {_previewGold:N0}";
        }

        private void ResetVisuals()
        {
            _fallingItem.gameObject.SetActive(false);
            _currentItemText.text = "대기 중";
            _rouletteText.text = "x1";
            _goldGainText.text = string.Empty;
            _goldText.text = $"골드 {_previewGold:N0}";
        }

        private bool HasRequiredReferences()
        {
            return _fallingItem != null &&
                _fallingItemImage != null &&
                _fallingItemLabel != null &&
                _dropStart != null &&
                _dropEnd != null &&
                _currentItemText != null &&
                _rouletteText != null &&
                _goldText != null &&
                _goldGainText != null;
        }
    }
}
