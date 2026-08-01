using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanPlanet.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("A scene name is required.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Scene '{sceneName}' is not included in the build settings.", this);
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
