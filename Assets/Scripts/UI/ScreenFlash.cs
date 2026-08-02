using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 전체화면을 덮는 이미지를 잠깐 밝혔다 페이드아웃하는 연출(잭팟 등 최상위 티어 전용).
    /// </summary>
    public sealed class ScreenFlash : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField, Min(0.01f)] private float _flashDuration = 0.4f;

        private Coroutine _flashCoroutine;

        /// <summary>
        /// 알파를 곧바로 최대치로 올린 뒤 _flashDuration 동안 0으로 페이드아웃한다.
        /// </summary>
        public void Play()
        {
            if (_image == null)
            {
                return;
            }

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(RunFlash());
        }

        private IEnumerator RunFlash()
        {
            Color color = _flashColor;
            color.a = 1f;
            _image.color = color;

            float elapsed = 0f;
            while (elapsed < _flashDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / _flashDuration);
                _image.color = color;
                yield return null;
            }

            color.a = 0f;
            _image.color = color;
            _flashCoroutine = null;
        }
    }
}
