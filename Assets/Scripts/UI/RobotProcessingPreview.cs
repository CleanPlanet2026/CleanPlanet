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

        [SerializeField] private RectTransform _externalItem;
        [SerializeField] private Image _externalItemImage;
        [SerializeField] private Text _externalItemLabel;
        [SerializeField] private RectTransform _externalDropStart;
        [SerializeField] private RectTransform _externalDropEnd;
        [SerializeField] private RectTransform _internalItem;
        [SerializeField] private Image _internalItemImage;
        [SerializeField] private Text _internalItemLabel;
        [SerializeField] private RectTransform _internalDropStart;
        [SerializeField] private RectTransform _internalDropEnd;
        [SerializeField] private Text _rouletteText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _goldGainText;
        [SerializeField, Min(0.05f)] private float _externalDropDuration = 0.45f;
        [SerializeField, Min(0.05f)] private float _internalDropDuration = 0.35f;
        [SerializeField, Min(0.02f)] private float _rouletteStep = 0.04f;
        [SerializeField, Min(0f)] private float _resultDuration = 0.25f;

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
                yield return AnimateItemAndRoulette();
                CompleteConversion(itemTypeIndex, resultMultiplierIndex);
                yield return new WaitForSeconds(_resultDuration);
                _itemIndex++;
            }
        }

        private IEnumerator AnimateItemAndRoulette()
        {
            float totalDuration = _externalDropDuration + _internalDropDuration;
            float elapsed = 0f;
            float rouletteElapsed = 0f;
            int rouletteIndex = 0;

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                rouletteElapsed += Time.deltaTime;

                if (elapsed < _externalDropDuration)
                {
                    float progress = Mathf.Clamp01(elapsed / _externalDropDuration);
                    _externalItem.anchoredPosition = Vector2.Lerp(
                        _externalDropStart.anchoredPosition,
                        _externalDropEnd.anchoredPosition,
                        progress);
                }
                else
                {
                    ShowInternalItem();
                    float internalElapsed = elapsed - _externalDropDuration;
                    float progress = Mathf.Clamp01(internalElapsed / _internalDropDuration);
                    _internalItem.anchoredPosition = Vector2.Lerp(
                        _internalDropStart.anchoredPosition,
                        _internalDropEnd.anchoredPosition,
                        progress);
                }

                if (rouletteElapsed >= _rouletteStep)
                {
                    rouletteElapsed = 0f;
                    rouletteIndex = (rouletteIndex + 1) % MultiplierLabels.Length;
                    _rouletteText.text = MultiplierLabels[rouletteIndex];
                }

                yield return null;
            }
        }

        private void PrepareItem(int itemTypeIndex)
        {
            string itemName = ItemNames[itemTypeIndex];
            Color itemColor = ItemColors[itemTypeIndex];

            _externalItemLabel.text = itemName;
            _externalItemImage.color = itemColor;
            _externalItem.anchoredPosition = _externalDropStart.anchoredPosition;
            _externalItem.gameObject.SetActive(true);

            _internalItemLabel.text = itemName;
            _internalItemImage.color = itemColor;
            _internalItem.anchoredPosition = _internalDropStart.anchoredPosition;
            _internalItem.gameObject.SetActive(false);
            _goldGainText.text = string.Empty;
        }

        private void ShowInternalItem()
        {
            if (_internalItem.gameObject.activeSelf)
            {
                return;
            }

            _externalItem.gameObject.SetActive(false);
            _internalItem.gameObject.SetActive(true);
        }

        private void CompleteConversion(int itemTypeIndex, int multiplierIndex)
        {
            int gainedGold = (itemTypeIndex + 1) * 10 * Multipliers[multiplierIndex];
            _previewGold += gainedGold;
            _externalItem.gameObject.SetActive(false);
            _internalItem.gameObject.SetActive(false);
            _rouletteText.text = MultiplierLabels[multiplierIndex];
            _goldGainText.text = $"+{gainedGold} 골드";
            _goldText.text = $"골드 {_previewGold:N0}";
        }

        private void ResetVisuals()
        {
            _externalItem.gameObject.SetActive(false);
            _internalItem.gameObject.SetActive(false);
            _rouletteText.text = "x1";
            _goldGainText.text = string.Empty;
            _goldText.text = $"골드 {_previewGold:N0}";
        }

        private bool HasRequiredReferences()
        {
            return _externalItem != null &&
                _externalItemImage != null &&
                _externalItemLabel != null &&
                _externalDropStart != null &&
                _externalDropEnd != null &&
                _internalItem != null &&
                _internalItemImage != null &&
                _internalItemLabel != null &&
                _internalDropStart != null &&
                _internalDropEnd != null &&
                _rouletteText != null &&
                _goldText != null &&
                _goldGainText != null;
        }
    }
}
