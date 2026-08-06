using CleanPlanet.Core.Persistence;
using UnityEngine;

namespace CleanPlanet.Core
{
    public static class GameSessionState
    {
        public static bool HasReachedBase { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            HasReachedBase = GameSaveSystem.Data.HasReachedBase;
        }

        public static void MarkBaseReached()
        {
            if (HasReachedBase)
            {
                return;
            }

            HasReachedBase = true;
            GameSaveSystem.Data.HasReachedBase = true;
            GameSaveSystem.MarkDirty();
        }

        internal static void ResetProgress()
        {
            HasReachedBase = false;
        }
    }
}
