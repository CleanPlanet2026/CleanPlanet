using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameAudioSettings = CleanPlanet.Core.Audio.AudioSettings;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 씬에 존재하는 일반 버튼과 홀드 확정 버튼에 공통 클릭음을 연결한다.
    /// 실제 UI 효과음이 준비되기 전까지 런타임에 생성한 짧은 전자음을 사용한다.
    /// </summary>
    public sealed class GlobalButtonSfx : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private enum ButtonSound
        {
            Select,
            Navigate,
            Adjust,
            Confirm,
            HoldConfirm
        }

        private static GlobalButtonSfx _instance;

        private readonly HashSet<Button> _registeredButtons = new();
        private readonly HashSet<HoldToConfirmButton> _registeredHoldButtons = new();
        private readonly Dictionary<ButtonSound, AudioClip> _clips = new();

        private AudioSource _audioSource;

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
            _audioSource.volume = GameAudioSettings.ButtonSfxVolume;
            CreateTemporaryClips();

            SceneManager.sceneLoaded += HandleSceneLoaded;
            GameAudioSettings.ButtonSfxVolumeChanged += HandleButtonSfxVolumeChanged;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            GameAudioSettings.ButtonSfxVolumeChanged -= HandleButtonSfxVolumeChanged;

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
            _registeredButtons.RemoveWhere(button => button == null);
            _registeredHoldButtons.RemoveWhere(button => button == null);

            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);

            foreach (Button button in buttons)
            {
                if (!_registeredButtons.Add(button))
                {
                    continue;
                }

                ButtonSound sound = Classify(button);
                button.onClick.AddListener(() => Play(sound));
            }

            HoldToConfirmButton[] holdButtons =
                FindObjectsByType<HoldToConfirmButton>(FindObjectsInactive.Include);

            foreach (HoldToConfirmButton holdButton in holdButtons)
            {
                if (_registeredHoldButtons.Add(holdButton))
                {
                    holdButton.Confirmed += () => Play(ButtonSound.HoldConfirm);
                }
            }
        }

        private void Play(ButtonSound sound)
        {
            _audioSource.PlayOneShot(_clips[sound]);
        }

        private void HandleButtonSfxVolumeChanged(float volume)
        {
            _audioSource.volume = volume;
        }

        private void CreateTemporaryClips()
        {
            _clips[ButtonSound.Select] = CreateTemporaryClip("UI Select", 1050f, 720f, 0.05f, 0.18f);
            _clips[ButtonSound.Navigate] = CreateTemporaryClip("UI Navigate", 760f, 1180f, 0.07f, 0.22f);
            _clips[ButtonSound.Adjust] = CreateTemporaryClip("UI Adjust", 1450f, 1080f, 0.035f, 0.14f);
            _clips[ButtonSound.Confirm] = CreateTemporaryClip("UI Confirm", 620f, 980f, 0.09f, 0.28f);
            _clips[ButtonSound.HoldConfirm] = CreateTemporaryClip("UI Hold Confirm", 460f, 760f, 0.13f, 0.34f);
        }

        private static ButtonSound Classify(Button button)
        {
            string buttonName = button.name;

            if (buttonName.Contains("Start") || buttonName.Contains("Upgrade Button"))
            {
                return ButtonSound.Confirm;
            }

            if (buttonName.Contains("Tab") ||
                buttonName.Contains("Settings") ||
                buttonName.Contains("Close") ||
                buttonName.Contains("Explore Button"))
            {
                return ButtonSound.Navigate;
            }

            if (buttonName.Contains("Zoom") || buttonName.Contains("Reset View"))
            {
                return ButtonSound.Adjust;
            }

            return ButtonSound.Select;
        }

        private static AudioClip CreateTemporaryClip(
            string clipName,
            float startFrequency,
            float endFrequency,
            float duration,
            float harmonicAmount)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float progress = time / duration;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                float attack = Mathf.Clamp01(time / 0.003f);
                float envelope = attack * Mathf.Exp(-3.5f * time / duration);

                phase += 2f * Mathf.PI * frequency / SampleRate;
                float tone = Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * harmonicAmount;
                samples[i] = tone * envelope * 0.22f;
            }

            AudioClip clip = AudioClip.Create(
                clipName,
                sampleCount,
                1,
                SampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
