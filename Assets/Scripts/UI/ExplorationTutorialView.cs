using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class ExplorationTutorialView : MonoBehaviour
    {
        private const string TutorialSeenKey = "CleanPlanet.ExplorationTutorialSeen";

        private static readonly Color OverlayColor = new(0.01f, 0.03f, 0.04f, 0.78f);
        private static readonly Color PanelColor = new(0.035f, 0.09f, 0.12f, 0.98f);
        private static readonly Color ControlColor = new(0.08f, 0.18f, 0.22f, 1f);
        private static readonly Color AccentColor = new(0.22f, 0.85f, 0.77f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.97f, 0.96f, 1f);

        [SerializeField] private Font _font;

        private GameObject _overlay;
        private bool _isOpen;
        private float _previousTimeScale = 1f;

        private void Awake()
        {
            Button helpButton = CreateButton("Tutorial Help Button", transform, "?",
                new Vector2(-24f, -24f), new Vector2(52f, 52f), ControlColor, true);
            helpButton.onClick.AddListener(OpenTutorial);

            _overlay = CreateTutorialOverlay();
            _overlay.SetActive(false);

            if (PlayerPrefs.GetInt(TutorialSeenKey, 0) == 0)
            {
                OpenTutorial();
            }
        }

        private void OnDisable()
        {
            RestoreTimeScale();
        }

        private GameObject CreateTutorialOverlay()
        {
            GameObject overlay = CreateUiObject("Exploration Tutorial Overlay", transform);
            Stretch(overlay.GetComponent<RectTransform>());
            overlay.AddComponent<Image>().color = OverlayColor;

            GameObject panel = CreateUiObject("Tutorial Panel", overlay.transform);
            SetCenterRect(panel.GetComponent<RectTransform>(), new Vector2(700f, 430f), Vector2.zero);
            panel.AddComponent<Image>().color = PanelColor;

            CreateText("Title", panel.transform, "탐험 방법", 32, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Vector2(0f, 150f), new Vector2(620f, 52f));

            Text description = CreateText("Instructions", panel.transform,
                "1. 쓰레기 더미를 클릭하면 로봇이 이동합니다.\n\n" +
                "2. 타이밍에 맞춰 스페이스바를 누르세요.\n\n" +
                "3. 배터리가 모두 소진되면 자동으로 베이스로 복귀합니다.",
                22, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Vector2(0f, 12f), new Vector2(580f, 220f));
            description.lineSpacing = 1.15f;

            Button startButton = CreateButton("Close Tutorial Button", panel.transform, "탐험 시작",
                new Vector2(0f, -154f), new Vector2(200f, 56f), AccentColor);
            startButton.onClick.AddListener(CloseTutorial);
            return overlay;
        }

        private void OpenTutorial()
        {
            if (_isOpen)
            {
                return;
            }

            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _isOpen = true;
            _overlay.SetActive(true);
            _overlay.transform.SetAsLastSibling();
        }

        private void CloseTutorial()
        {
            PlayerPrefs.SetInt(TutorialSeenKey, 1);
            PlayerPrefs.Save();
            _overlay.SetActive(false);
            RestoreTimeScale();
        }

        private void RestoreTimeScale()
        {
            if (!_isOpen)
            {
                return;
            }

            Time.timeScale = _previousTimeScale;
            _isOpen = false;
        }

        private Button CreateButton(string name, Transform parent, string label,
            Vector2 position, Vector2 size, Color color, bool topRight = false)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            if (topRight)
            {
                rect.anchorMin = Vector2.one;
                rect.anchorMax = Vector2.one;
                rect.pivot = Vector2.one;
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            else
            {
                SetCenterRect(rect, size, position);
            }

            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", buttonObject.transform, label, 24, FontStyle.Bold,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform);
            return button;
        }

        private Text CreateText(string name, Transform parent, string value, int fontSize,
            FontStyle fontStyle, TextAnchor alignment, Vector2 position, Vector2 size)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = TextColor;
            text.raycastTarget = false;
            text.text = value;
            SetCenterRect(text.rectTransform, size, position);
            return text;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void SetCenterRect(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
