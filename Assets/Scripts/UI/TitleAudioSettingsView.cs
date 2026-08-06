using UnityEngine;
using UnityEngine.UI;
using GameAudioSettings = CleanPlanet.Core.Audio.AudioSettings;

namespace CleanPlanet.UI
{
    public sealed class TitleAudioSettingsView : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.035f, 0.09f, 0.12f, 0.96f);
        private static readonly Color ControlColor = new(0.08f, 0.18f, 0.22f, 1f);
        private static readonly Color AccentColor = new(0.22f, 0.85f, 0.77f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.97f, 0.96f, 1f);

        [SerializeField] private Font _font;

        private GameObject _panel;
        private Text _musicValue;
        private Text _buttonSfxValue;

        private void Awake()
        {
            Button settingsButton = CreateSettingsButton();
            _panel = CreatePanel();
            settingsButton.onClick.AddListener(TogglePanel);
            _panel.SetActive(false);
        }

        private Button CreateSettingsButton()
        {
            GameObject buttonObject = CreateUiObject("Audio Settings Button", transform);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-32f, -32f);
            rect.sizeDelta = new Vector2(64f, 64f);

            Image background = buttonObject.AddComponent<Image>();
            background.color = ControlColor;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;

            CreateSliderIcon(buttonObject.transform, 13f, -8f);
            CreateSliderIcon(buttonObject.transform, 0f, 8f);
            CreateSliderIcon(buttonObject.transform, -13f, -3f);
            return button;
        }

        private void CreateSliderIcon(Transform parent, float y, float knobX)
        {
            Image line = CreateImage("Line", parent, AccentColor);
            SetCenterRect(line.rectTransform, new Vector2(30f, 3f), new Vector2(0f, y));

            Image knob = CreateImage("Knob", parent, AccentColor);
            SetCenterRect(knob.rectTransform, new Vector2(8f, 8f), new Vector2(knobX, y));
        }

        private GameObject CreatePanel()
        {
            GameObject panel = CreateUiObject("Audio Settings Panel", transform);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-32f, -108f);
            rect.sizeDelta = new Vector2(420f, 260f);
            panel.AddComponent<Image>().color = PanelColor;

            CreateText("Title", panel.transform, "오디오 설정", 26, TextAnchor.MiddleCenter,
                new Vector2(0f, -30f), new Vector2(360f, 42f));

            CreateText("Music Label", panel.transform, "음악", 19, TextAnchor.MiddleLeft,
                new Vector2(-135f, -86f), new Vector2(100f, 32f));
            Slider musicSlider = CreateSlider("Music Slider", panel.transform, new Vector2(25f, -86f));
            _musicValue = CreateText("Music Value", panel.transform, string.Empty, 17,
                TextAnchor.MiddleRight, new Vector2(160f, -86f), new Vector2(70f, 32f));

            CreateText("Button Sfx Label", panel.transform, "버튼 효과음", 19, TextAnchor.MiddleLeft,
                new Vector2(-135f, -142f), new Vector2(130f, 32f));
            Slider buttonSfxSlider = CreateSlider("Button Sfx Slider", panel.transform, new Vector2(25f, -142f));
            _buttonSfxValue = CreateText("Button Sfx Value", panel.transform, string.Empty, 17,
                TextAnchor.MiddleRight, new Vector2(160f, -142f), new Vector2(70f, 32f));

            Button closeButton = CreateTextButton(panel.transform, "닫기", new Vector2(0f, -210f));
            closeButton.onClick.AddListener(ClosePanel);

            musicSlider.SetValueWithoutNotify(GameAudioSettings.MusicVolume);
            buttonSfxSlider.SetValueWithoutNotify(GameAudioSettings.ButtonSfxVolume);
            UpdateMusicValue(GameAudioSettings.MusicVolume);
            UpdateButtonSfxValue(GameAudioSettings.ButtonSfxVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            buttonSfxSlider.onValueChanged.AddListener(SetButtonSfxVolume);
            return panel;
        }

        private Slider CreateSlider(string name, Transform parent, Vector2 position)
        {
            GameObject sliderObject = CreateUiObject(name, parent);
            SetCenteredRect(sliderObject.GetComponent<RectTransform>(), new Vector2(200f, 32f), position);

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            Image background = CreateImage("Background", sliderObject.transform, ControlColor);
            RectTransform backgroundRect = background.rectTransform;
            SetCenterRect(backgroundRect, new Vector2(200f, 8f), Vector2.zero);
            background.raycastTarget = true;

            GameObject fillArea = CreateUiObject("Fill Area", sliderObject.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.sizeDelta = new Vector2(-12f, 8f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            Image fill = CreateImage("Fill", fillArea.transform, AccentColor);
            Stretch(fill.rectTransform);

            GameObject handleArea = CreateUiObject("Handle Slide Area", sliderObject.transform);
            Stretch(handleArea.GetComponent<RectTransform>());
            Image handle = CreateImage("Handle", handleArea.transform, TextColor);
            SetCenterRect(handle.rectTransform, new Vector2(18f, 24f), Vector2.zero);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private Button CreateTextButton(Transform parent, string label, Vector2 position)
        {
            GameObject buttonObject = CreateUiObject("Close Button", parent);
            SetCenteredRect(buttonObject.GetComponent<RectTransform>(), new Vector2(120f, 40f), position);
            Image image = buttonObject.AddComponent<Image>();
            image.color = ControlColor;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Label", buttonObject.transform, label, 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.zero);
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
            SetCenteredRect(text.rectTransform, size, position);
            text.text = value;
            return text;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void SetCenteredRect(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
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

        private void TogglePanel()
        {
            _panel.SetActive(!_panel.activeSelf);
            _panel.transform.SetAsLastSibling();
        }

        private void ClosePanel()
        {
            _panel.SetActive(false);
        }

        private void SetMusicVolume(float volume)
        {
            GameAudioSettings.SetMusicVolume(volume);
            UpdateMusicValue(volume);
        }

        private void SetButtonSfxVolume(float volume)
        {
            GameAudioSettings.SetButtonSfxVolume(volume);
            UpdateButtonSfxValue(volume);
        }

        private void UpdateMusicValue(float volume)
        {
            _musicValue.text = $"{Mathf.RoundToInt(volume * 100f)}%";
        }

        private void UpdateButtonSfxValue(float volume)
        {
            _buttonSfxValue.text = $"{Mathf.RoundToInt(volume * 100f)}%";
        }
    }
}
