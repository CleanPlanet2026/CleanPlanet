using UnityEngine;

namespace CleanPlanet.Core
{
    public static class GameSessionState
    {
        public static bool HasReachedBase { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            HasReachedBase = false;
        }

        public static void MarkBaseReached()
        {
            HasReachedBase = true;
        }
    }
}
