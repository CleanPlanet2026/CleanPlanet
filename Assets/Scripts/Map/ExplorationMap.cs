using CleanPlanet.Player;
using UnityEngine;

namespace CleanPlanet.Map
{
    public sealed class ExplorationMap : MonoBehaviour
    {
        [SerializeField] private GridManager _gridManager;

        public GridManager GridManager => _gridManager;

        public void Initialize(PlayerMovement playerMovement, PlayerClickToMove playerClickToMove)
        {
            _gridManager.Initialize(playerMovement, playerClickToMove);
        }
    }
}
