using UnityEngine;

namespace CleanPlanet.Trash
{
    [RequireComponent(typeof(QteController))]
    public class QteResultSfx : MonoBehaviour
    {
        [SerializeField] private QteController _qte;
        [SerializeField] private AudioClip _failClip;
        [SerializeField] private AudioClip _successClip;
        [SerializeField] private AudioClip _greatSuccessClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.45f;

        private AudioSource _audioSource;

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
            _qte.OnFail += PlayFail;
            _qte.OnSuccess += PlaySuccess;
            _qte.OnGreatSuccess += PlayGreatSuccess;
        }

        private void OnDisable()
        {
            _qte.OnFail -= PlayFail;
            _qte.OnSuccess -= PlaySuccess;
            _qte.OnGreatSuccess -= PlayGreatSuccess;
        }

        private void PlayFail()
        {
            Play(_failClip);
        }

        private void PlaySuccess()
        {
            Play(_successClip);
        }

        private void PlayGreatSuccess()
        {
            Play(_greatSuccessClip);
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, _volume);
            }
        }
    }
}
