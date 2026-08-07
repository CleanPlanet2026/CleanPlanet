using System.Collections;
using UnityEngine;

namespace CleanPlanet.Trash
{
    [RequireComponent(typeof(QteController))]
    public class TrashCollectionSfx : MonoBehaviour
    {
        [SerializeField] private QteController _qte;
        [SerializeField] private AudioClip[] _collectionClips;
        [SerializeField] private float[] _clipStartTimes;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.2f;
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.25f;

        private AudioSource _audioSource;
        private Coroutine _fadeRoutine;
        private int _lastClipIndex = -1;

        private void Awake()
        {
            if (_qte == null)
            {
                _qte = GetComponent<QteController>();
            }

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            _qte.OnQTEStarted += PlayRandomClip;
            _qte.OnGreatSuccess += FadeOut;
            _qte.OnSuccess += FadeOut;
            _qte.OnFail += FadeOut;
        }

        private void OnDisable()
        {
            _qte.OnQTEStarted -= PlayRandomClip;
            _qte.OnGreatSuccess -= FadeOut;
            _qte.OnSuccess -= FadeOut;
            _qte.OnFail -= FadeOut;

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            _audioSource.Stop();
        }

        private void PlayRandomClip()
        {
            if (_collectionClips == null || _collectionClips.Length == 0)
            {
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            int clipIndex = Random.Range(0, _collectionClips.Length);
            if (_collectionClips.Length > 1 && clipIndex == _lastClipIndex)
            {
                clipIndex = (clipIndex + Random.Range(1, _collectionClips.Length))
                    % _collectionClips.Length;
            }

            _lastClipIndex = clipIndex;
            _audioSource.clip = _collectionClips[clipIndex];
            _audioSource.volume = _volume;
            _audioSource.time = GetClipStartTime(clipIndex);
            _audioSource.Play();
        }

        private float GetClipStartTime(int clipIndex)
        {
            if (_clipStartTimes == null || clipIndex >= _clipStartTimes.Length)
            {
                return 0f;
            }

            return Mathf.Clamp(_clipStartTimes[clipIndex], 0f, _audioSource.clip.length);
        }

        private void FadeOut()
        {
            if (!_audioSource.isPlaying)
            {
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            float startVolume = _audioSource.volume;
            float elapsed = 0f;

            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = _fadeOutDuration <= 0f ? 1f : elapsed / _fadeOutDuration;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }

            _audioSource.Stop();
            _audioSource.volume = _volume;
            _fadeRoutine = null;
        }
    }
}
