using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanPlanet.UI
{
    public sealed class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "GameScene";

        public void StartGame()
        {
            SceneManager.LoadScene(_gameSceneName);
        }
    }
}
