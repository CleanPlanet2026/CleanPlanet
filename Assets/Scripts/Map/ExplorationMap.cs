using CleanPlanet.Player;
using UnityEngine;

namespace CleanPlanet.Map
{
    public sealed class ExplorationMap : MonoBehaviour
    {
        [SerializeField] private string _mapId;
        [SerializeField] private GridManager _gridManager;

        public string MapId => _mapId;
        public GridManager GridManager => _gridManager;

        public void Initialize(
            PlayerMovement playerMovement,
            PlayerClickToMove playerClickToMove,
            string entryId = null)
        {
            ExplorationEntryPoint entryPoint = FindEntryPoint(entryId);
            if (entryPoint != null)
            {
                playerMovement.Unregister();
                Vector2 spawnPosition = _gridManager.Grid.GridToWorldCenter(entryPoint.GridIndex);
                playerMovement.transform.position = new Vector3(
                    spawnPosition.x,
                    spawnPosition.y,
                    playerMovement.transform.position.z);
            }

            _gridManager.Initialize(playerMovement, playerClickToMove);

            foreach (ExplorationPortal portal in GetComponentsInChildren<ExplorationPortal>(true))
            {
                portal.Register(_gridManager);
            }
        }

        private ExplorationEntryPoint FindEntryPoint(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return null;
            }

            foreach (ExplorationEntryPoint entryPoint in GetComponentsInChildren<ExplorationEntryPoint>(true))
            {
                if (entryPoint.EntryId == entryId)
                {
                    return entryPoint;
                }
            }

            Debug.LogWarning($"{name}에서 입장 지점 '{entryId}'을 찾을 수 없습니다.", this);
            return null;
        }
    }
}
