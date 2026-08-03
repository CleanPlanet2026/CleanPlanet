using UnityEngine;
using UnityEngine.EventSystems;

namespace CleanPlanet.UI
{
    [ExecuteAlways]
    public sealed class SkillTreeViewportController : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private Vector2 _designSize = new(1400f, 960f);
        [SerializeField, Min(0f)] private float _padding = 20f;
        [SerializeField, Range(0.1f, 1f)] private float _minimumFitScale = 0.3f;
        [SerializeField, Range(0.1f, 1f)] private float _minimumZoom = 0.8f;
        [SerializeField, Min(1f)] private float _maximumZoom = 2f;
        [SerializeField, Min(0.01f)] private float _zoomStep = 0.15f;

        private RectTransform _viewport;
        private float _fitScale = 1f;
        private float _userZoom = 1f;
        private Vector2 _lastPointerPosition;

        private void OnEnable()
        {
            _viewport = transform as RectTransform;
            RefreshFitScale(resetPosition: false);
        }

        private void OnRectTransformDimensionsChange()
        {
            RefreshFitScale(resetPosition: false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _viewport = transform as RectTransform;
            RefreshFitScale(resetPosition: false);
        }
#endif

        public void OnBeginDrag(PointerEventData eventData)
        {
            TryGetPointerPosition(eventData, out _lastPointerPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_content == null || !TryGetPointerPosition(eventData, out Vector2 pointerPosition))
            {
                return;
            }

            _content.anchoredPosition += pointerPosition - _lastPointerPosition;
            _lastPointerPosition = pointerPosition;
            ClampContentPosition();
        }

        public void OnScroll(PointerEventData eventData)
        {
            float direction = Mathf.Sign(eventData.scrollDelta.y);
            if (Mathf.Approximately(direction, 0f))
            {
                return;
            }

            ZoomBy(direction * _zoomStep, eventData.position, eventData.pressEventCamera);
        }

        public void ZoomIn()
        {
            ZoomBy(_zoomStep, GetViewportScreenCenter(), null);
        }

        public void ZoomOut()
        {
            ZoomBy(-_zoomStep, GetViewportScreenCenter(), null);
        }

        public void ResetView()
        {
            _userZoom = 1f;
            ApplyScale();
            _content.anchoredPosition = Vector2.zero;
        }

        private void RefreshFitScale(bool resetPosition)
        {
            if (_viewport == null || _content == null || _designSize.x <= 0f || _designSize.y <= 0f)
            {
                return;
            }

            _content.sizeDelta = _designSize;
            Rect viewportRect = _viewport.rect;
            float width = Mathf.Max(0f, viewportRect.width - (_padding * 2f));
            float height = Mathf.Max(0f, viewportRect.height - (_padding * 2f));
            _fitScale = Mathf.Clamp(Mathf.Min(width / _designSize.x, height / _designSize.y),
                _minimumFitScale, 1f);
            ApplyScale();

            if (resetPosition)
            {
                _content.anchoredPosition = Vector2.zero;
            }
            else
            {
                ClampContentPosition();
            }
        }

        private void ZoomBy(float amount, Vector2 screenPoint, Camera eventCamera)
        {
            if (_content == null || _viewport == null)
            {
                return;
            }

            float previousScale = GetFinalScale();
            float nextZoom = Mathf.Clamp(_userZoom + amount, _minimumZoom, _maximumZoom);
            if (Mathf.Approximately(nextZoom, _userZoom))
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _viewport, screenPoint, eventCamera, out Vector2 pointerPosition);
            _userZoom = nextZoom;
            ApplyScale();

            float ratio = GetFinalScale() / previousScale;
            _content.anchoredPosition = pointerPosition
                + ((_content.anchoredPosition - pointerPosition) * ratio);
            ClampContentPosition();
        }

        private void ApplyScale()
        {
            if (_content == null)
            {
                return;
            }

            float scale = GetFinalScale();
            _content.localScale = new Vector3(scale, scale, 1f);
        }

        private float GetFinalScale() => _fitScale * _userZoom;

        private void ClampContentPosition()
        {
            if (_content == null || _viewport == null)
            {
                return;
            }

            float scale = GetFinalScale();
            Vector2 scaledSize = _designSize * scale;
            Rect viewportRect = _viewport.rect;
            float limitX = Mathf.Max(0f, (scaledSize.x - viewportRect.width) * 0.5f + _padding);
            float limitY = Mathf.Max(0f, (scaledSize.y - viewportRect.height) * 0.5f + _padding);
            Vector2 position = _content.anchoredPosition;
            position.x = Mathf.Clamp(position.x, -limitX, limitX);
            position.y = Mathf.Clamp(position.y, -limitY, limitY);
            _content.anchoredPosition = position;
        }

        private bool TryGetPointerPosition(PointerEventData eventData, out Vector2 position)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _viewport, eventData.position, eventData.pressEventCamera, out position);
        }

        private Vector2 GetViewportScreenCenter()
        {
            Vector3 worldCenter = _viewport.TransformPoint(_viewport.rect.center);
            return RectTransformUtility.WorldToScreenPoint(null, worldCenter);
        }
    }
}
