using UnityEngine;

namespace CleanPlanet.Map
{
    public sealed class ExplorationEntryPoint : MonoBehaviour
    {
        [SerializeField] private string _entryId;
        [SerializeField] private Vector2Int _gridIndex;

        public string EntryId => _entryId;
        public Vector2Int GridIndex => _gridIndex;
    }
}
