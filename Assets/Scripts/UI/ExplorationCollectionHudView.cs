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

        private Text _countLabel;
        private int _initialCount;

        private void Awake()
        {
            GameObject panel = new("Exploration Collection HUD", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -24f);
            panelRect.sizeDelta = new Vector2(320f, 52f);
            panel.GetComponent<Image>().color = PanelColor;

            GameObject labelObject = new("Count Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(panel.transform, false);
            _countLabel = labelObject.GetComponent<Text>();
            _countLabel.font = _font;
            _countLabel.fontSize = 22;
            _countLabel.fontStyle = FontStyle.Bold;
            _countLabel.alignment = TextAnchor.MiddleCenter;
            _countLabel.color = TextColor;
            _countLabel.raycastTarget = false;

            RectTransform labelRect = _countLabel.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private void OnEnable()
        {
            _initialCount = CollectionInbox.Count;
            CollectionInbox.CountChanged += HandleCountChanged;
            UpdateCount(CollectionInbox.Count);
        }

        private void OnDisable()
        {
            CollectionInbox.CountChanged -= HandleCountChanged;
        }

        private void HandleCountChanged(int totalCount)
        {
            UpdateCount(totalCount);
        }

        private void UpdateCount(int totalCount)
        {
            int expeditionCount = Mathf.Max(0, totalCount - _initialCount);
            _countLabel.text = $"이번 탐험 수집물  {expeditionCount}개";
        }
    }
}
