using System;
using CleanPlanet.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 일정 시간 이상 눌러야 확정되는 버튼. 짧게 누르거나 중간에 포인터가 벗어나면 취소된다.
    /// 진행 상태를 보여주는 채움 이미지는 선택 사항이다.
    /// </summary>
    public sealed class HoldToConfirmButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Min(0f)] private float _holdDuration = 3f;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Text _label;
        [SerializeField] private string _hoverLabel = "1초간 길게 클릭";

        public event Action Confirmed;

        private float _heldTime;
        private bool _isHeld;
        private bool _isPointerOver;
        private string _defaultLabel;

        private void Awake()
        {
            if (_label != null)
            {
                _defaultLabel = _label.text;
            }
        }

        private void OnDisable()
        {
            _isPointerOver = false;
            CancelHold();
            UpdateLabel(showHoverLabel: false);
        }

        private void Update()
        {
            PointerGate.ReleaseIfButtonUp();

            if (!_isHeld)
            {
                return;
            }

            _heldTime += Time.deltaTime;
            UpdateProgress(_heldTime / _holdDuration);

            if (_heldTime >= _holdDuration)
            {
                _isHeld = false;
                UpdateProgress(0f);
                PointerGate.Lock();
                Confirmed?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (PointerGate.IsLocked)
            {
                return;
            }

            _isHeld = true;
            _heldTime = 0f;
            UpdateLabel(showHoverLabel: true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelHold();
            UpdateLabel(_isPointerOver);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOver = true;
            UpdateLabel(showHoverLabel: true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
            CancelHold();
            UpdateLabel(showHoverLabel: false);
        }

        private void CancelHold()
        {
            _isHeld = false;
            _heldTime = 0f;
            UpdateProgress(0f);
        }

        private void UpdateProgress(float ratio)
        {
            if (_progressFill != null)
            {
                _progressFill.fillAmount = Mathf.Clamp01(ratio);
            }
        }

        private void UpdateLabel(bool showHoverLabel)
        {
            if (_label != null)
            {
                _label.text = showHoverLabel ? _hoverLabel : _defaultLabel;
            }
        }
    }
}
