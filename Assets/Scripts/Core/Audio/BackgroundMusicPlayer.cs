using UnityEngine;

namespace CleanPlanet.Core.Audio
{
    /// <summary>
    /// 시작 씬에서 배경음을 재생하고 씬 전환 후에도 같은 재생 상태를 유지한다.
    /// </summary>
    public sealed class BackgroundMusicPlayer : MonoBehaviour
    {
        private static BackgroundMusicPlayer _instance;

        [SerializeField] private AudioClip _music;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.28f;

        private AudioSource _audioSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = _volume * AudioSettings.MusicVolume;
            _audioSource.clip = _music;
            AudioSettings.MusicVolumeChanged += HandleMusicVolumeChanged;

            if (_music != null)
            {
                _audioSource.Play();
            }
        }

        private void OnDestroy()
        {
            AudioSettings.MusicVolumeChanged -= HandleMusicVolumeChanged;

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void HandleMusicVolumeChanged(float volume)
        {
            _audioSource.volume = _volume * volume;
        }
    }
}
