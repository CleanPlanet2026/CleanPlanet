using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 씬에 존재하는 일반 버튼과 홀드 확정 버튼에 공통 클릭음을 연결한다.
    /// 실제 UI 효과음이 준비되기 전까지 런타임에 생성한 짧은 전자음을 사용한다.
    /// </summary>
    public sealed class GlobalButtonSfx : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float ClipDuration = 0.06f;

        private static GlobalButtonSfx _instance;

        private AudioSource _audioSource;
        private AudioClip _temporaryClickClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject serviceObject = new(nameof(GlobalButtonSfx));
            _instance = serviceObject.AddComponent<GlobalButtonSfx>();
            DontDestroyOnLoad(serviceObject);
        }

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 0.35f;
            _temporaryClickClip = CreateTemporaryClickClip();

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterButtons();
        }

        private void RegisterButtons()
        {
            Button[] buttons = FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (Button button in buttons)
            {
                button.onClick.RemoveListener(PlayClick);
                button.onClick.AddListener(PlayClick);
            }

            HoldToConfirmButton[] holdButtons = FindObjectsByType<HoldToConfirmButton>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (HoldToConfirmButton holdButton in holdButtons)
            {
                holdButton.Confirmed -= PlayClick;
                holdButton.Confirmed += PlayClick;
            }
        }

        private void PlayClick()
        {
            _audioSource.PlayOneShot(_temporaryClickClip);
        }

        private static AudioClip CreateTemporaryClickClip()
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * ClipDuration);
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float progress = time / ClipDuration;
                float frequency = Mathf.Lerp(1350f, 720f, progress);
                float attack = Mathf.Clamp01(time / 0.003f);
                float envelope = attack * Mathf.Exp(-42f * time);

                phase += 2f * Mathf.PI * frequency / SampleRate;
                float tone = Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * 0.2f;
                samples[i] = tone * envelope * 0.24f;
            }

            AudioClip clip = AudioClip.Create(
                "Temporary UI Click",
                sampleCount,
                1,
                SampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
