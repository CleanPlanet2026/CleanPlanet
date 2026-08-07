using UnityEngine;

namespace CleanPlanet.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(AudioSource))]
    public class RobotMovementSfx : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _rollingClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.2f;
        [SerializeField, Min(0.1f)] private float _fadeSpeed = 2.5f;
        [SerializeField, Min(0.05f)] private float _startCueDuration = 0.3f;
        [SerializeField, Min(0.05f)] private float _endingDuration = 0.5f;

        private bool _wasMoving;

        private void Awake()
        {
            if (_movement == null)
            {
                _movement = GetComponent<PlayerMovement>();
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 0f;

            _audioSource.clip = _rollingClip;

            if (_rollingClip != null && _rollingClip.loadState == AudioDataLoadState.Unloaded)
            {
                _rollingClip.LoadAudioData();
            }
        }

        private void Update()
        {
            if (_rollingClip == null || _rollingClip.loadState == AudioDataLoadState.Failed)
            {
                return;
            }

            if (_rollingClip.loadState == AudioDataLoadState.Unloaded)
            {
                _rollingClip.LoadAudioData();
                return;
            }

            if (_rollingClip.loadState == AudioDataLoadState.Loading)
            {
                return;
            }

            bool isMoving = _movement.IsMoving;
            if (isMoving && !_wasMoving)
            {
                _audioSource.time = 0f;
                _audioSource.Play();
            }

            if (isMoving && _audioSource.isPlaying && _audioSource.time >= _startCueDuration)
            {
                _audioSource.Stop();
            }

            if (!isMoving && _wasMoving)
            {
                float endingStartTime = Mathf.Max(0f, _rollingClip.length - _endingDuration);
                _audioSource.time = endingStartTime;
                _audioSource.Play();
            }

            float targetVolume = _audioSource.isPlaying ? _volume : 0f;
            _audioSource.volume = Mathf.MoveTowards(
                _audioSource.volume,
                targetVolume,
                _fadeSpeed * Time.deltaTime);

            _wasMoving = isMoving;
        }
    }
}
