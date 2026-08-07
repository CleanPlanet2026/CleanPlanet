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

        private readonly Dictionary<string, int> _countsById = new();
        private readonly Dictionary<string, string> _namesById = new();

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
            _countsById.Clear();
            _namesById.Clear();
            _totalCount = 0;
            CollectionInbox.ItemsAdded += HandleItemsAdded;
            CollectionInbox.ItemsRemoved += HandleItemsRemoved;
            UpdateLabels();
        }

        private void OnDisable()
        {
            CollectionInbox.ItemsAdded -= HandleItemsAdded;
            CollectionInbox.ItemsRemoved -= HandleItemsRemoved;
        }

        private void HandleItemsAdded(CollectibleData item, int count)
        {
            string id = item.PersistenceId;
            _namesById[id] = string.IsNullOrWhiteSpace(item.Name) ? "이름 없는 수집물" : item.Name;
            _countsById.TryGetValue(id, out int currentCount);
            _countsById[id] = currentCount + count;
            _totalCount += count;
            UpdateLabels();
        }

        private void HandleItemsRemoved(string id, int count)
        {
            if (!_countsById.TryGetValue(id, out int currentCount))
            {
                return;
            }

            int removed = Mathf.Min(currentCount, count);
            int remaining = currentCount - removed;
            if (remaining > 0)
            {
                _countsById[id] = remaining;
            }
            else
            {
                _countsById.Remove(id);
                _namesById.Remove(id);
            }

            _totalCount -= removed;
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            _totalLabel.text = $"이번 탐험 수집물  {_totalCount}개";
            if (_countsById.Count == 0)
            {
                _breakdownLabel.text = "아직 수집한 물품이 없습니다";
                return;
            }

            var ids = new List<string>(_countsById.Keys);
            ids.Sort((left, right) => string.Compare(_namesById[left], _namesById[right], System.StringComparison.Ordinal));

            var entries = new List<string>();
            int visibleCount = Mathf.Min(ids.Count, 4);
            for (int i = 0; i < visibleCount; i++)
            {
                string id = ids[i];
                entries.Add($"{_namesById[id]} ×{_countsById[id]}");
            }

            if (ids.Count > visibleCount)
            {
                entries.Add($"외 {ids.Count - visibleCount}종");
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
