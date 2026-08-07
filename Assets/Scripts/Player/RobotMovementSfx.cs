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
            _audioSource.loop = true;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = _volume;

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

            if (!isMoving && _wasMoving)
            {
                _audioSource.Stop();
            }

            _wasMoving = isMoving;
        }
    }
}
