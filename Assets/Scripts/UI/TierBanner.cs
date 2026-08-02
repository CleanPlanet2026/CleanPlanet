using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// "대박!", "잭팟!!!" 같은 티어 문구를 스케일 팝 + 페이드로 잠깐 띄우는 배너.
    /// </summary>
    public sealed class TierBanner : MonoBehaviour
    {
        [SerializeField] private Text _label;
        [SerializeField, Min(0.01f)] private float _popDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float _holdDuration = 0.6f;
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.3f;
        [SerializeField] private float _popScale = 1.3f;

        private Coroutine _showCoroutine;

        private void Awake()
        {
            // 첫 Show 전까지는 보이지 않아야 한다. 알파를 0으로 초기화한다.
            if (_label != null)
            {
                Color color = _label.color;
                color.a = 0f;
                _label.color = color;
            }
        }

        /// <summary>
        /// text가 비어있으면 아무 연출도 하지 않는다. 그 외에는 텍스트를 세팅하고
        /// 스케일 팝 후 유지, 페이드아웃하는 순서로 연출한다.
        /// </summary>
        public void Show(string text)
        {
            if (_label == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }

            _showCoroutine = StartCoroutine(RunShow(text));
        }

        private IEnumerator RunShow(string text)
        {
            _label.text = text;

            Transform label = _label.transform;
            Color color = _label.color;
            color.a = 1f;
            _label.color = color;

            float elapsed = 0f;
            while (elapsed < _popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _popDuration);
                float scale = Mathf.Lerp(1f, _popScale, t);
                label.localScale = Vector3.one * scale;
                yield return null;
            }

            label.localScale = Vector3.one * _popScale;
            yield return new WaitForSeconds(_holdDuration);

            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeDuration);
                float scale = Mathf.Lerp(_popScale, 1f, t);
                label.localScale = Vector3.one * scale;
                color.a = Mathf.Lerp(1f, 0f, t);
                _label.color = color;
                yield return null;
            }

            label.localScale = Vector3.one;
            color.a = 0f;
            _label.color = color;
            _showCoroutine = null;
        }
    }
}
