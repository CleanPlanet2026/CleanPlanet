using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class StageSelectButtonHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private static readonly Vector3 HoverScale = Vector3.one * 1.05f;
        private static readonly Vector3 PressedScale = Vector3.one * 0.98f;

        private Button _button;

        public void Initialize(Button button)
        {
            _button = button;
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (_button != null && _button.interactable)
            {
                transform.localScale = HoverScale;
            }
        }

        public void OnPointerExit(PointerEventData _)
        {
            transform.localScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData _)
        {
            if (_button != null && _button.interactable)
            {
                transform.localScale = PressedScale;
            }
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (_button != null && _button.interactable)
            {
                transform.localScale = HoverScale;
            }
        }

        private void OnDisable()
        {
            transform.localScale = Vector3.one;
        }
    }
}
