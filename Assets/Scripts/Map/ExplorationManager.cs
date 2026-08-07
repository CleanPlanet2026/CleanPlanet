using CleanPlanet.Player;
using UnityEngine;

namespace CleanPlanet.Map
{
    public sealed class ExplorationManager : MonoBehaviour
    {
        [SerializeField] private ExplorationMap _startingMapPrefab;
        [SerializeField] private Transform _mapRoot;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerClickToMove _playerClickToMove;

        public ExplorationMap CurrentMap { get; private set; }

        private void Awake()
        {
            LoadStartingMap();
        }

        private void LoadStartingMap()
        {
            if (_startingMapPrefab == null || _playerMovement == null || _playerClickToMove == null)
            {
                Debug.LogError($"{nameof(ExplorationManager)}에 필요한 참조가 없습니다.", this);
                return;
            }

            Transform parent = _mapRoot != null ? _mapRoot : transform;
            CurrentMap = Instantiate(_startingMapPrefab, parent);
            CurrentMap.Initialize(_playerMovement, _playerClickToMove);
        }
    }
}
