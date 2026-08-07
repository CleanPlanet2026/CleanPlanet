using CleanPlanet.Trash;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class RadiationQteView : MonoBehaviour
    {
        private static readonly Color OverlayColor = new(0.02f, 0.12f, 0.03f, 0.78f);
        private static readonly Color PanelColor = new(0.04f, 0.14f, 0.05f, 0.98f);
        private static readonly Color TrackColor = new(0.05f, 0.08f, 0.05f, 1f);
        private static readonly Color ProgressColor = new(0.35f, 1f, 0.22f, 1f);

        [SerializeField] private RadiationQteController _qte;
        [SerializeField] private Font _font;
        [SerializeField, Min(0f)] private float _resultDisplayDuration = 0.8f;

        private GameObject _root;
        private Image _progressFill;
        private Text _statusLabel;
        private Text _counterLabel;

        private void Awake()
        {
            CreateView();
            _root.SetActive(false);
        }

        private void OnEnable()
        {
            if (_qte == null)
            {
                Debug.LogError($"{nameof(RadiationQteView)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _qte.Started += HandleStarted;
            _qte.Succeeded += HandleSucceeded;
            _qte.Failed += HandleFailed;
        }

        private void OnDisable()
        {
            if (_qte != null)
            {
                _qte.Started -= HandleStarted;
                _qte.Succeeded -= HandleSucceeded;
                _qte.Failed -= HandleFailed;
            }

            CancelInvoke();
        }

        private void Update()
        {
            if (!_qte.IsActive)
            {
                return;
            }

            _progressFill.fillAmount = _qte.Progress;
            _counterLabel.text = $"SPACE  {_qte.PressCount} / {_qte.RequiredPressCount}   ·   {_qte.RemainingTime:0.0}초";
        }

        private void HandleStarted()
        {
            CancelInvoke();
            _statusLabel.text = "방사능 감지! 스페이스바를 연타하세요!";
            _statusLabel.color = Color.white;
            _progressFill.fillAmount = 0f;
            _counterLabel.text = $"SPACE  0 / {_qte.RequiredPressCount}";
            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
        }

        private void HandleSucceeded()
        {
            ShowResult("위험 회피! 보너스 수집 성공", ProgressColor);
        }

        private void HandleFailed()
        {
            ShowResult("피폭 발생! 수집물 일부를 잃었습니다", new Color(1f, 0.25f, 0.12f));
        }

        private void ShowResult(string message, Color color)
        {
            _statusLabel.text = message;
            _statusLabel.color = color;
            Invoke(nameof(Hide), _resultDisplayDuration);
        }

        private void Hide()
        {
            _root.SetActive(false);
        }

        private void CreateView()
        {
            _root = CreateUiObject("Radiation QTE Overlay", transform);
            Stretch(_root.GetComponent<RectTransform>());
            _root.AddComponent<Image>().color = OverlayColor;

            GameObject panel = CreateUiObject("Radiation QTE Panel", _root.transform);
            SetCenterRect(panel.GetComponent<RectTransform>(), new Vector2(720f, 260f), Vector2.zero);
            panel.AddComponent<Image>().color = PanelColor;

            _statusLabel = CreateText("Status", panel.transform, 30, FontStyle.Bold,
                new Vector2(0f, 72f), new Vector2(650f, 58f));

            GameObject track = CreateUiObject("Mash Progress Track", panel.transform);
            SetCenterRect(track.GetComponent<RectTransform>(), new Vector2(600f, 48f), new Vector2(0f, 2f));
            track.AddComponent<Image>().color = TrackColor;

            GameObject fill = CreateUiObject("Mash Progress Fill", track.transform);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            Stretch(fillRect);
            fillRect.offsetMin = new Vector2(6f, 6f);
            fillRect.offsetMax = new Vector2(-6f, -6f);
            _progressFill = fill.AddComponent<Image>();
            _progressFill.color = ProgressColor;
            _progressFill.type = Image.Type.Filled;
            _progressFill.fillMethod = Image.FillMethod.Horizontal;
            _progressFill.fillOrigin = 0;

            _counterLabel = CreateText("Counter", panel.transform, 22, FontStyle.Bold,
                new Vector2(0f, -67f), new Vector2(650f, 48f));
        }

        private Text CreateText(string name, Transform parent, int fontSize, FontStyle style,
            Vector2 position, Vector2 size)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
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
