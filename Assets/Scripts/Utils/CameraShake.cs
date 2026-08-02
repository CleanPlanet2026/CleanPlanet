using System.Collections;
using UnityEngine;

namespace CleanPlanet.Utils
{
    /// <summary>
    /// 자신이 붙은 오브젝트를 원위치 기준으로 흔든다(카메라에 붙이면 화면 흔들림 연출).
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        private Vector3 _originalLocalPosition;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            _originalLocalPosition = transform.localPosition;
        }

        /// <summary>
        /// intensity 반경의 노이즈로 duration 동안 흔들고 원위치로 복원한다.
        /// 이미 흔드는 중이면 이전 연출을 정리하고 새로 시작한다.
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                transform.localPosition = _originalLocalPosition;
            }

            _shakeCoroutine = StartCoroutine(RunShake(intensity, duration));
        }

        private IEnumerator RunShake(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector2 offset = Random.insideUnitCircle * intensity;
                transform.localPosition = _originalLocalPosition + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            transform.localPosition = _originalLocalPosition;
            _shakeCoroutine = null;
        }
    }
}
