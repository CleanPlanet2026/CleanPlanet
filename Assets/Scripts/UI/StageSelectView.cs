using CleanPlanet.Core;
using CleanPlanet.Core.Persistence;
using CleanPlanet.Map.Procedural;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class StageSelectView : MonoBehaviour
    {
        private static readonly Color BackgroundColor = new(0.015f, 0.055f, 0.065f, 1f);
        private static readonly Color PanelColor = new(0.035f, 0.11f, 0.13f, 0.98f);
        private static readonly Color ActiveColor = new(0.12f, 0.48f, 0.42f, 1f);
        private static readonly Color LockedColor = new(0.12f, 0.16f, 0.17f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.97f, 0.96f, 1f);

        private const float CardSpacingMin = 400f;
        private const float CardSpacingMax = 440f;

        [SerializeField] private Font _font;
        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField] private StageConfig[] _stages;
        [SerializeField] private string _stageOneSceneName = "GameScene";
        [SerializeField] private string _baseSceneName = "BaseScene";

        private void Awake()
        {
            Image background = gameObject.AddComponent<Image>();
            background.color = _backgroundSprite != null ? Color.white : BackgroundColor;
            background.sprite = _backgroundSprite;

            CreateText("Title", transform, "탐험 스테이지 선택", 42, FontStyle.Bold,
                new Vector2(0f, 390f), new Vector2(900f, 70f));
            CreateText("Description", transform, "탐험할 구역을 선택하세요", 22, FontStyle.Normal,
                new Vector2(0f, 330f), new Vector2(700f, 44f));

            CreateStageCards();

            Button backButton = CreateButton("Back To Base Button", transform, "베이스로 돌아가기",
                new Vector2(0f, -390f), new Vector2(260f, 62f), PanelColor, true);
            backButton.onClick.AddListener(ReturnToBase);
        }

        private void CreateStageCards()
        {
            int count = _stages?.Length ?? 0;
            if (count == 0)
            {
                Debug.LogWarning($"{nameof(StageSelectView)}에 연결된 StageConfig가 없습니다.", this);
                return;
            }

            float spacing = count > 1
                ? Mathf.Clamp(1600f / (count - 1), CardSpacingMin, CardSpacingMax)
                : 0f;
            float startX = -(count - 1) * spacing / 2f;

            float earthClean = GameSaveSystem.Data.EarthClean;
            float unlockThreshold = 0f;

            for (int i = 0; i < count; i++)
            {
                StageConfig stage = _stages[i];
                if (stage == null) continue;

                bool unlocked = earthClean >= unlockThreshold;
                int stageIndex = i;
                Vector2 position = new(startX + i * spacing, 20f);

                CreateStageCard($"Stage {stage.StageId}", stage.DisplayName, stage.ShortDescription,
                    position, unlocked, unlocked ? () => LoadStage(stageIndex) : null);

                unlockThreshold = stage.CleanGoalToUnlockNext;
            }
        }

        private void CreateStageCard(string title, string subtitle, string details,
            Vector2 position, bool unlocked, UnityEngine.Events.UnityAction onClick)
        {
            GameObject card = CreateUiObject($"{title} Card", transform);
            SetCenterRect(card.GetComponent<RectTransform>(), new Vector2(380f, 500f), position);
            card.AddComponent<Image>().color = unlocked ? ActiveColor : LockedColor;

            CreateText("Stage Number", card.transform, title, 34, FontStyle.Bold,
                new Vector2(0f, 170f), new Vector2(330f, 58f));
            CreateText("Stage Name", card.transform, subtitle, 25, FontStyle.Bold,
                new Vector2(0f, 105f), new Vector2(330f, 50f));

            string status = unlocked
                ? "탐험 가능\n\n" + details
                : "잠김\n\n" + details;
            CreateText("Stage Details", card.transform, status, 20, FontStyle.Normal,
                new Vector2(0f, -10f), new Vector2(320f, 150f));

            Button selectButton = CreateButton("Select Button", card.transform,
                unlocked ? "탐험하기" : "잠김",
                new Vector2(0f, -185f), new Vector2(240f, 64f),
                unlocked ? new Color(0.18f, 0.78f, 0.67f, 1f) : LockedColor, unlocked);
            selectButton.interactable = unlocked;
            if (unlocked && onClick != null)
            {
                selectButton.onClick.AddListener(onClick);
            }
        }

        private void LoadStage(int stageIndex)
        {
            StageSessionState.SelectedStageIndex = stageIndex;
            SceneManager.LoadScene(_stageOneSceneName);
        }

        private void ReturnToBase()
        {
            SceneManager.LoadScene(_baseSceneName);
        }

        private Button CreateButton(string name, Transform parent, string label,
            Vector2 position, Vector2 size, Color color, bool interactable)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            SetCenterRect(buttonObject.GetComponent<RectTransform>(), size, position);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.interactable = interactable;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            colors.pressedColor = new Color(0.72f, 0.8f, 0.78f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            buttonObject.AddComponent<StageSelectButtonHover>().Initialize(button);

            Text text = CreateText("Label", buttonObject.transform, label, 22, FontStyle.Bold,
                Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform);
            return button;
        }

        private Text CreateText(string name, Transform parent, string value, int fontSize,
            FontStyle style, Vector2 position, Vector2 size)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
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
