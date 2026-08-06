using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class BaseMenuView : MonoBehaviour
    {
        private static readonly Color OverlayColor = new(0.01f, 0.03f, 0.04f, 0.72f);
        private static readonly Color PanelColor = new(0.035f, 0.09f, 0.12f, 0.98f);
        private static readonly Color ControlColor = new(0.08f, 0.18f, 0.22f, 1f);
        private static readonly Color AccentColor = new(0.22f, 0.85f, 0.77f, 1f);
        private static readonly Color DangerColor = new(0.75f, 0.22f, 0.18f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.97f, 0.96f, 1f);

        [SerializeField] private Font _font;
        [SerializeField] private string _titleSceneName = "TitleScene";

        private GameObject _menuPanel;
        private GameObject _confirmationOverlay;

        private void Awake()
        {
            Button menuButton = CreateButton("Menu Button", transform, "메뉴",
                new Vector2(24f, -24f), new Vector2(120f, 64f), ControlColor, true);
            _menuPanel = CreateMenuPanel();
            _confirmationOverlay = CreateConfirmationOverlay();

            menuButton.onClick.AddListener(ToggleMenu);
            _menuPanel.SetActive(false);
            _confirmationOverlay.SetActive(false);
        }

        private GameObject CreateMenuPanel()
        {
            GameObject panel = CreateUiObject("Base Menu Panel", transform);
            RectTransform rect = panel.GetComponent<RectTransform>();
            SetTopLeftRect(rect, new Vector2(300f, 72f), new Vector2(24f, -104f));
            panel.AddComponent<Image>().color = PanelColor;

            Button returnButton = CreateButton("Return To Title Button", panel.transform,
                "타이틀로 돌아가기", new Vector2(0f, -36f), new Vector2(244f, 48f), ControlColor);
            returnButton.onClick.AddListener(OpenConfirmation);
            return panel;
        }

        private GameObject CreateConfirmationOverlay()
        {
            GameObject overlay = CreateUiObject("Return To Title Confirmation", transform);
            Stretch(overlay.GetComponent<RectTransform>());
            overlay.AddComponent<Image>().color = OverlayColor;

            GameObject dialog = CreateUiObject("Dialog", overlay.transform);
            SetCenterRect(dialog.GetComponent<RectTransform>(), new Vector2(520f, 240f), Vector2.zero);
            dialog.AddComponent<Image>().color = PanelColor;

            CreateText("Title", dialog.transform, "타이틀로 돌아갈까요?", 28,
                TextAnchor.MiddleCenter, new Vector2(0f, 62f), new Vector2(440f, 44f));
            CreateText("Description", dialog.transform, "현재 진행 상태는 유지됩니다.", 20,
                TextAnchor.MiddleCenter, new Vector2(0f, 12f), new Vector2(440f, 36f));

            Button confirmButton = CreateButton("Confirm Return Button", dialog.transform,
                "돌아가기", new Vector2(-105f, -68f), new Vector2(170f, 52f), DangerColor);
            confirmButton.onClick.AddListener(ReturnToTitle);

            Button cancelButton = CreateButton("Cancel Return Button", dialog.transform,
                "취소", new Vector2(105f, -68f), new Vector2(170f, 52f), ControlColor);
            cancelButton.onClick.AddListener(CloseConfirmation);
            return overlay;
        }

        private void ToggleMenu()
        {
            _menuPanel.SetActive(!_menuPanel.activeSelf);
            _menuPanel.transform.SetAsLastSibling();
        }

        private void OpenConfirmation()
        {
            _menuPanel.SetActive(false);
            _confirmationOverlay.SetActive(true);
            _confirmationOverlay.transform.SetAsLastSibling();
        }

        private void CloseConfirmation()
        {
            _confirmationOverlay.SetActive(false);
        }

        private void ReturnToTitle()
        {
            SceneManager.LoadScene(_titleSceneName);
        }

        private Button CreateButton(string name, Transform parent, string label,
            Vector2 position, Vector2 size, Color color, bool topLeft = false)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            if (topLeft)
            {
                SetTopLeftRect(rect, size, position);
            }
            else
            {
                SetCenterRect(rect, size, position);
            }

            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", buttonObject.transform, label, 20,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform);
            return button;
        }

        private Text CreateText(string name, Transform parent, string value, int fontSize,
            TextAnchor alignment, Vector2 position, Vector2 size)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = TextColor;
            text.raycastTarget = false;
            SetCenterRect(text.rectTransform, size, position);
            text.text = value;
            return text;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void SetTopLeftRect(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
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
