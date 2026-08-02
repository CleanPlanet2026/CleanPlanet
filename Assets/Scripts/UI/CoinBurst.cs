using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 코인 한 개가 시작점에서 도착점(골드 HUD)까지 포물선을 그리며 날아가는 연출.
    /// 풀에서 재사용되며, 연출이 끝나면 완료 콜백을 알린 뒤 스스로 비활성화된다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class CoinBurst : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Min(0.01f)] private float _moveDuration = 0.5f;
        [SerializeField] private float _arcHeight = 80f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.6f);
        [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("폭발 연출(2단계: 흩어짐 → 상승)")]
        [SerializeField] private float _scatterRadiusMin = 60f;
        [SerializeField] private float _scatterRadiusMax = 180f;
        [SerializeField, Min(0.01f)] private float _scatterDuration = 0.3f;
        [SerializeField, Min(0f)] private float _scatterHold = 0.15f;
        [SerializeField, Min(0.01f)] private float _riseDuration = 0.7f;

        private RectTransform _rectTransform;
        private Coroutine _activeCoroutine;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        /// <summary>
        /// from → to 구간(부모 RectTransform 기준 anchoredPosition)을 포물선으로 이동한다.
        /// coinScale은 기존 스케일 커브 값 위에 곱해져 티어별로 코인 크기를 키운다(잭팟 등).
        /// onComplete는 도착 직후, 비활성화 직전에 호출된다.
        /// </summary>
        public void Play(Vector2 from, Vector2 to, float coinScale = 1f, Action onComplete = null)
        {
            StopActiveCoroutine();
            _activeCoroutine = StartCoroutine(RunPlay(from, to, coinScale, onComplete));
        }

        /// <summary>
        /// 폭발 모드: (a) from에서 랜덤 각도·반경으로 흩어짐(감속) → (b) 잠시 정지 →
        /// (c) 흩어진 점에서 to까지 상승(가감속). 코인마다 상승 출발을 spreadDuration 안에서
        /// 랜덤하게 늦춰, 다 같이 터진 뒤 그 시간(오디오 길이) 동안 분수처럼 주르륵 올라간다.
        /// </summary>
        public void PlayExplode(Vector2 from, Vector2 to, float coinScale, float spreadDuration, float radiusScale, Action onComplete = null)
        {
            StopActiveCoroutine();
            _activeCoroutine = StartCoroutine(RunExplode(from, to, coinScale, spreadDuration, radiusScale, onComplete));
        }

        /// <summary>흩어짐+정지+상승만 합한 폭발 코인 한 개의 최소 연출 길이(상승 딜레이 제외).</summary>
        public float ExplodeDuration => _scatterDuration + _scatterHold + _riseDuration;

        private void StopActiveCoroutine()
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
                _activeCoroutine = null;
            }
        }

        private IEnumerator RunPlay(Vector2 from, Vector2 to, float coinScale, Action onComplete)
        {
            ApplyFrame(from, 0f, coinScale);

            float elapsed = 0f;
            while (elapsed < _moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _moveDuration);

                Vector2 linear = Vector2.Lerp(from, to, t);
                float arcOffset = 4f * _arcHeight * t * (1f - t);
                _rectTransform.anchoredPosition = new Vector2(linear.x, linear.y + arcOffset);
                ApplyScaleAndAlpha(t, coinScale);

                yield return null;
            }

            ApplyFrame(to, 1f, coinScale);

            _activeCoroutine = null;
            onComplete?.Invoke();
            gameObject.SetActive(false);
        }

        private IEnumerator RunExplode(Vector2 from, Vector2 to, float coinScale, float spreadDuration, float radiusScale, Action onComplete)
        {
            float totalDuration = Mathf.Max(0.0001f, ExplodeDuration);
            float overallElapsed = 0f;

            // 코인이 많을수록 radiusScale이 커져 더 넓게 흩어진다(스포너가 개수로 산정).
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(_scatterRadiusMin, _scatterRadiusMax) * radiusScale;
            Vector2 scattered = from + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            // 코인마다 상승 출발을 spreadDuration(오디오 길이)에서 남는 시간만큼 랜덤하게 늦춰
            // "다 같이 터진 뒤 그 시간 동안 분수처럼 주르륵" 올라가게 한다.
            float riseWindow = Mathf.Max(0f, spreadDuration - ExplodeDuration);
            float riseDelay = UnityEngine.Random.Range(0f, riseWindow);

            _rectTransform.anchoredPosition = from;
            ApplyScale(0f, coinScale);
            SetAlpha(1f);

            // (a) 흩어짐: ease-out(빠르게 튕겨 서서히 멈춤)
            float phaseElapsed = 0f;
            while (phaseElapsed < _scatterDuration)
            {
                phaseElapsed += Time.deltaTime;
                overallElapsed += Time.deltaTime;

                float localT = Mathf.Clamp01(phaseElapsed / _scatterDuration);
                float eased = 1f - (1f - localT) * (1f - localT) * (1f - localT);
                _rectTransform.anchoredPosition = Vector2.Lerp(from, scattered, eased);
                ApplyScale(Mathf.Clamp01(overallElapsed / totalDuration), coinScale);
                SetAlpha(1f);

                yield return null;
            }
            _rectTransform.anchoredPosition = scattered;

            // (b) 정지 + 코인별 랜덤 상승 딜레이
            float holdElapsed = 0f;
            while (holdElapsed < _scatterHold + riseDelay)
            {
                holdElapsed += Time.deltaTime;
                overallElapsed += Time.deltaTime;
                yield return null;
            }

            // (c) 상승: ease-in-out, 살짝 아크. 모든 코인이 같은 타이밍이라 여기서 함께 오른다.
            phaseElapsed = 0f;
            while (phaseElapsed < _riseDuration)
            {
                phaseElapsed += Time.deltaTime;
                overallElapsed += Time.deltaTime;

                float localT = Mathf.Clamp01(phaseElapsed / _riseDuration);
                float eased = localT * localT * (3f - 2f * localT);
                Vector2 linear = Vector2.Lerp(scattered, to, eased);
                float arcOffset = 4f * _arcHeight * eased * (1f - eased);
                _rectTransform.anchoredPosition = new Vector2(linear.x, linear.y + arcOffset);
                ApplyScale(Mathf.Clamp01(overallElapsed / totalDuration), coinScale);
                SetAlpha(_alphaCurve.Evaluate(localT));

                yield return null;
            }

            _rectTransform.anchoredPosition = to;
            ApplyScale(1f, coinScale);
            SetAlpha(_alphaCurve.Evaluate(1f));

            _activeCoroutine = null;
            onComplete?.Invoke();
            gameObject.SetActive(false);
        }

        private void ApplyFrame(Vector2 position, float t, float coinScale)
        {
            _rectTransform.anchoredPosition = position;
            ApplyScaleAndAlpha(t, coinScale);
        }

        private void ApplyScaleAndAlpha(float t, float coinScale)
        {
            ApplyScale(t, coinScale);
            SetAlpha(_alphaCurve.Evaluate(t));
        }

        private void ApplyScale(float t, float coinScale)
        {
            _rectTransform.localScale = Vector3.one * (_scaleCurve.Evaluate(t) * coinScale);
        }

        private void SetAlpha(float alpha)
        {
            if (_image == null)
            {
                return;
            }

            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
        }
    }
}
