using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanPlanet.Core.Robot
{
    /// <summary>
    /// 플레이어가 직접 탐색을 중단할 때 확인한 뒤 베이스 복귀로 연결한다.
    /// 배터리 소진으로 인한 자동 복귀는 경고 UI를 먼저 보여줘야
    /// 하므로 LowBatteryWarning이 별도로 처리한다.
    /// </summary>
    public sealed class ExplorationReturnTrigger : MonoBehaviour
    {
        [SerializeField] private string _baseSceneName = "BaseScene";
        [SerializeField] private string _stopKeyBinding = "<Keyboard>/r";
        [SerializeField] private Button _stopButton;
        [SerializeField] private RectTransform _uiRoot;
        [SerializeField] private Font _font;

        private InputAction _stopAction;
        private GameObject _confirmationOverlay;

        private void Awake()
        {
            _stopAction = new InputAction(binding: _stopKeyBinding);
            _stopAction.performed += OnStopPerformed;
            _confirmationOverlay = CreateConfirmationOverlay();
            _confirmationOverlay.SetActive(false);
        }

        private void OnEnable()
        {
            _stopAction.Enable();

            if (_stopButton != null)
            {
                _stopButton.onClick.AddListener(RequestReturnToBase);
            }
        }

        private void OnDisable()
        {
            _stopAction.Disable();

            if (_stopButton != null)
            {
                _stopButton.onClick.RemoveListener(RequestReturnToBase);
            }
        }

        private void OnDestroy()
        {
            _stopAction.performed -= OnStopPerformed;
            _stopAction.Dispose();
        }

        private void OnStopPerformed(InputAction.CallbackContext context)
        {
            RequestReturnToBase();
        }

        public void RequestReturnToBase()
        {
            _confirmationOverlay.SetActive(true);
            _confirmationOverlay.transform.SetAsLastSibling();
        }

        private void ConfirmReturnToBase()
        {
            SceneManager.LoadScene(_baseSceneName);
        }

        private void CancelReturnToBase()
        {
            _confirmationOverlay.SetActive(false);
        }

        private GameObject CreateConfirmationOverlay()
        {
            GameObject overlay = CreateUiObject("Exploration Exit Confirmation", _uiRoot);
            Stretch(overlay.GetComponent<RectTransform>());
            overlay.AddComponent<Image>().color = new Color(0.01f, 0.03f, 0.04f, 0.72f);

            GameObject dialog = CreateUiObject("Dialog", overlay.transform);
            SetCenterRect(dialog.GetComponent<RectTransform>(), new Vector2(520f, 240f), Vector2.zero);
            dialog.AddComponent<Image>().color = new Color(0.035f, 0.09f, 0.12f, 0.98f);

            CreateText(dialog.transform, "탐험을 종료할까요?", 28, new Vector2(0f, 60f), new Vector2(440f, 44f));
            CreateText(dialog.transform, "수집한 물품을 가지고 베이스로 돌아갑니다.", 19,
                new Vector2(0f, 12f), new Vector2(450f, 36f));

            Button confirmButton = CreateButton(dialog.transform, "종료", new Vector2(-105f, -68f),
                new Color(0.75f, 0.22f, 0.18f, 1f));
            confirmButton.onClick.AddListener(ConfirmReturnToBase);

            Button cancelButton = CreateButton(dialog.transform, "계속 탐험", new Vector2(105f, -68f),
                new Color(0.08f, 0.18f, 0.22f, 1f));
            cancelButton.onClick.AddListener(CancelReturnToBase);
            return overlay;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Color color)
        {
            GameObject buttonObject = CreateUiObject($"{label} Button", parent);
            SetCenterRect(buttonObject.GetComponent<RectTransform>(), new Vector2(170f, 52f), position);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText(buttonObject.transform, label, 20, Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform);
            return button;
        }

        private Text CreateText(Transform parent, string value, int fontSize, Vector2 position, Vector2 size)
        {
            GameObject textObject = CreateUiObject("Label", parent);
            Text text = textObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.92f, 0.97f, 0.96f, 1f);
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
