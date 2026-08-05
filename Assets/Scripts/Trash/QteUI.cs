using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// QteController를 원형으로 시각화하는 임시 UI. Needle/Success Zone/Great Zone은 각각
    /// 자신만의 빈 Pivot 부모(Ring 중심에 위치)를 돌려서 회전한다 — 이미지 자신의 Pivot이나
    /// 스프라이트 모양과 무관하게 항상 Ring 중심을 축으로 정확히 돈다.
    /// Success/Great Zone은 QTE가 시작될 때마다 QteController가 새로 배치한 각도를 그대로
    /// 읽어 그리므로 실제 판정 구간과 항상 일치한다.
    /// </summary>
    public class QteUI : MonoBehaviour
    {
        [SerializeField] private QteController _qte;
        [SerializeField] private GameObject _root;

        [Header("회전축(Pivot) — Ring 중심에 위치한 빈 RectTransform")]
        [SerializeField] private RectTransform _needlePivot;
        [SerializeField] private RectTransform _successZonePivot;
        [SerializeField] private RectTransform _greatSuccessZonePivot;

        [Header("실제 그림")]
        [SerializeField] private Image _needleImage;
        [SerializeField] private Image _successZoneImage;
        [SerializeField] private Image _greatSuccessZoneImage;

        [Header("회전 보정 — 스프라이트가 0도(위쪽)를 기본으로 그려지지 않았을 때 사용")]
        [SerializeField] private float _needleRotationOffset;
        [SerializeField] private float _successZoneRotationOffset;
        [SerializeField] private float _greatSuccessZoneRotationOffset;

        [SerializeField] private Color _progressColor = Color.white;
        [SerializeField] private Color _failColor = Color.red;
        [SerializeField] private Color _successColor = Color.green;
        [SerializeField] private Color _greatSuccessColor = new(1f, 0.84f, 0f);
        [SerializeField, Min(0f)] private float _resultDisplayDuration = 0.5f;

        private void OnEnable()
        {
            if (_qte == null || _root == null || _needlePivot == null)
            {
                Debug.LogError($"{nameof(QteUI)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _qte.OnQTEStarted += HandleStarted;
            _qte.OnGreatSuccess += HandleGreatSuccess;
            _qte.OnSuccess += HandleSuccess;
            _qte.OnFail += HandleFail;
            _root.SetActive(false);
        }

        private void OnDisable()
        {
            _qte.OnQTEStarted -= HandleStarted;
            _qte.OnGreatSuccess -= HandleGreatSuccess;
            _qte.OnSuccess -= HandleSuccess;
            _qte.OnFail -= HandleFail;
        }

        private void Update()
        {
            if (!_qte.IsActive)
            {
                return;
            }

            _needlePivot.localRotation = Quaternion.Euler(0f, 0f, -_qte.NeedleAngle + _needleRotationOffset);
        }

        private void HandleStarted()
        {
            _root.SetActive(true);
            DrawZones();

            if (_needleImage != null)
            {
                _needleImage.color = _progressColor;
            }
        }

        private void HandleGreatSuccess()
        {
            ShowResult(_greatSuccessColor);
        }

        private void HandleSuccess()
        {
            ShowResult(_successColor);
        }

        private void HandleFail()
        {
            ShowResult(_failColor);
        }

        private void ShowResult(Color color)
        {
            if (_needleImage != null)
            {
                _needleImage.color = color;
            }

            Invoke(nameof(Hide), _resultDisplayDuration);
        }

        private void Hide()
        {
            _root.SetActive(false);
        }

        /// <summary>
        /// Success/Great Zone Pivot을 QteController가 이번 판에 배치한 시작 각도로 회전시키고,
        /// Radial360 Image를 쓰는 경우를 위해 fillAmount(구간 폭)도 함께 설정한다.
        /// </summary>
        private void DrawZones()
        {
            ConfigureZone(_successZonePivot, _successZoneImage, _qte.SuccessStartAngle, _qte.SuccessEndAngle, _successZoneRotationOffset);
            ConfigureZone(_greatSuccessZonePivot, _greatSuccessZoneImage, _qte.GreatStartAngle, _qte.GreatEndAngle, _greatSuccessZoneRotationOffset);
        }

        private static void ConfigureZone(RectTransform pivot, Image zoneImage, float startAngle, float endAngle, float rotationOffset)
        {
            if (pivot == null)
            {
                return;
            }

            pivot.localRotation = Quaternion.Euler(0f, 0f, -startAngle + rotationOffset);

            if (zoneImage != null)
            {
                zoneImage.fillAmount = Mathf.Clamp01((endAngle - startAngle) / 360f);
            }
        }
    }
}
