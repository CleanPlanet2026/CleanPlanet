using System;
using UnityEngine;

namespace CleanPlanet.Core.Audio
{
    public static class AudioSettings
    {
        private const string MusicVolumeKey = "Audio.MusicVolume";
        private const string ButtonSfxVolumeKey = "Audio.ButtonSfxVolume";

        public static event Action<float> MusicVolumeChanged;
        public static event Action<float> ButtonSfxVolumeChanged;

        public static float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        public static float ButtonSfxVolume => PlayerPrefs.GetFloat(ButtonSfxVolumeKey, 1f);

        public static void SetMusicVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, clampedVolume);
            PlayerPrefs.Save();
            MusicVolumeChanged?.Invoke(clampedVolume);
        }

        public static void SetButtonSfxVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(ButtonSfxVolumeKey, clampedVolume);
            PlayerPrefs.Save();
            ButtonSfxVolumeChanged?.Invoke(clampedVolume);
        }
    }
}
