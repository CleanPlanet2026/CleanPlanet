using UnityEngine;

namespace CleanPlanet.Map
{
    public sealed class ExplorationPortal : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridIndex;
        [SerializeField] private string _destinationMapId;
        [SerializeField] private string _destinationEntryId;

        public string DestinationMapId => _destinationMapId;
        public string DestinationEntryId => _destinationEntryId;

        private GridOccupancy _occupancy;

        public void Register(GridManager gridManager)
        {
            _occupancy = gridManager.Occupancy;
            Vector2 position = gridManager.Grid.GridToWorldCenter(_gridIndex);
            transform.position = new Vector3(position.x, position.y, transform.position.z);

            if (!_occupancy.TryOccupy(_gridIndex, gameObject))
            {
                Debug.LogWarning($"포탈이 셀 ({_gridIndex.x},{_gridIndex.y})을 점유하지 못했습니다.", this);
            }
        }

        private void OnDestroy()
        {
            _occupancy?.Release(_gridIndex, gameObject);
        }
    }
}
