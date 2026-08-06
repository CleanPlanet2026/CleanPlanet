using System.Collections.Generic;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Core.Collection;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class ExplorationCollectionHudView : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.035f, 0.09f, 0.12f, 0.9f);
        private static readonly Color TextColor = new(0.92f, 0.97f, 0.96f, 1f);

        [SerializeField] private Font _font;

        private readonly Dictionary<string, int> _countsByName = new();

        private Text _totalLabel;
        private Text _breakdownLabel;
        private int _totalCount;

        private void Awake()
        {
            GameObject panel = new("Exploration Collection HUD", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -24f);
            panelRect.sizeDelta = new Vector2(620f, 78f);
            panel.GetComponent<Image>().color = PanelColor;

            _totalLabel = CreateLabel("Total Label", panel.transform, 21, FontStyle.Bold,
                new Vector2(0f, 14f), new Vector2(590f, 30f));
            _breakdownLabel = CreateLabel("Breakdown Label", panel.transform, 17, FontStyle.Normal,
                new Vector2(0f, -17f), new Vector2(590f, 28f));
        }

        private void OnEnable()
        {
            _countsByName.Clear();
            _totalCount = 0;
            CollectionInbox.ItemsAdded += HandleItemsAdded;
            UpdateLabels();
        }

        private void OnDisable()
        {
            CollectionInbox.ItemsAdded -= HandleItemsAdded;
        }

        private void HandleItemsAdded(CollectibleData item, int count)
        {
            string itemName = string.IsNullOrWhiteSpace(item.Name) ? "이름 없는 수집물" : item.Name;
            _countsByName.TryGetValue(itemName, out int currentCount);
            _countsByName[itemName] = currentCount + count;
            _totalCount += count;
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            _totalLabel.text = $"이번 탐험 수집물  {_totalCount}개";
            if (_countsByName.Count == 0)
            {
                _breakdownLabel.text = "아직 수집한 물품이 없습니다";
                return;
            }

            var names = new List<string>(_countsByName.Keys);
            names.Sort();

            var entries = new List<string>();
            int visibleCount = Mathf.Min(names.Count, 4);
            for (int i = 0; i < visibleCount; i++)
            {
                string itemName = names[i];
                entries.Add($"{itemName} ×{_countsByName[itemName]}");
            }

            if (names.Count > visibleCount)
            {
                entries.Add($"외 {names.Count - visibleCount}종");
            }

            _breakdownLabel.text = string.Join("   ·   ", entries);
        }

        private Text CreateLabel(string name, Transform parent, int fontSize, FontStyle fontStyle,
            Vector2 position, Vector2 size)
        {
            GameObject labelObject = new(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            Text label = labelObject.GetComponent<Text>();
            label.font = _font;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = TextColor;
            label.raycastTarget = false;

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return label;
        }
    }
}
