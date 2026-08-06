using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanPlanet.Core.Persistence
{
    public sealed class GameSaveRunner : MonoBehaviour
    {
        private const float AutoSaveInterval = 5f;

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            StartCoroutine(AutoSave());
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        }

        private IEnumerator AutoSave()
        {
            var wait = new WaitForSecondsRealtime(AutoSaveInterval);
            while (true)
            {
                yield return wait;
                GameSaveSystem.SaveNow();
            }
        }

        private static void HandleSceneUnloaded(Scene _)
        {
            GameSaveSystem.SaveNow();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                GameSaveSystem.SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            GameSaveSystem.SaveNow();
        }
    }
}
