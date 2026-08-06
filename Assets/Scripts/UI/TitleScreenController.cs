using CleanPlanet.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private string _baseSceneName = "BaseScene";
        [SerializeField] private Font _font;

        private void Awake()
        {
            if (GameSessionState.HasReachedBase)
            {
                CreateBaseButton();
            }
        }

        public void StartGame()
        {
            SceneManager.LoadScene(_gameSceneName);
        }

        private void MoveToBase()
        {
            SceneManager.LoadScene(_baseSceneName);
        }

        private void CreateBaseButton()
        {
            GameObject buttonObject = new("Base Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(transform, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -245f);
            rect.sizeDelta = new Vector2(320f, 78f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.08f, 0.18f, 0.22f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.45f, 1f, 0.86f, 1f);
            colors.pressedColor = new Color(0.28f, 0.72f, 0.6f, 1f);
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1.25f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.onClick.AddListener(MoveToBase);

            GameObject labelObject = new("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            Text label = labelObject.GetComponent<Text>();
            label.font = _font;
            label.fontSize = 25;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.92f, 0.97f, 0.96f, 1f);
            label.raycastTarget = false;
            label.text = "베이스로 이동";

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }
    }
}
