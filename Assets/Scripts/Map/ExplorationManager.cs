using CleanPlanet.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.Map
{
    public sealed class ExplorationManager : MonoBehaviour
    {
        [SerializeField] private ExplorationMap _startingMapPrefab;
        [SerializeField] private ExplorationMap[] _mapPrefabs;
        [SerializeField] private Transform _mapRoot;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerClickToMove _playerClickToMove;

        public ExplorationMap CurrentMap { get; private set; }

        private readonly Dictionary<string, ExplorationMap> _loadedMaps = new();
        private ExplorationPortal _pendingPortal;

        private void Awake()
        {
            LoadStartingMap();
        }

        private void OnEnable()
        {
            _playerClickToMove.OnPortalSelected += HandlePortalSelected;
            _playerMovement.OnRobotArrived += HandleRobotArrived;
        }

        private void OnDisable()
        {
            _playerClickToMove.OnPortalSelected -= HandlePortalSelected;
            _playerMovement.OnRobotArrived -= HandleRobotArrived;
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
            _loadedMaps[CurrentMap.MapId] = CurrentMap;
        }

        private void HandlePortalSelected(ExplorationPortal portal)
        {
            _pendingPortal = portal;
        }

        private void HandleRobotArrived(Vector2Int _)
        {
            if (_pendingPortal == null)
            {
                return;
            }

            string destinationMapId = _pendingPortal.DestinationMapId;
            string destinationEntryId = _pendingPortal.DestinationEntryId;
            _pendingPortal = null;
            LoadMap(destinationMapId, destinationEntryId);
        }

        private void LoadMap(string mapId, string entryId)
        {
            ExplorationMap mapPrefab = Array.Find(
                _mapPrefabs,
                candidate => candidate != null && candidate.MapId == mapId);
            if (mapPrefab == null)
            {
                Debug.LogError($"목적지 맵 '{mapId}'을 찾을 수 없습니다.", this);
                return;
            }

            _playerMovement.Unregister();
            if (CurrentMap != null)
            {
                CurrentMap.gameObject.SetActive(false);
            }

            if (!_loadedMaps.TryGetValue(mapId, out ExplorationMap targetMap))
            {
                Transform parent = _mapRoot != null ? _mapRoot : transform;
                targetMap = Instantiate(mapPrefab, parent);
                _loadedMaps[mapId] = targetMap;
            }

            CurrentMap = targetMap;
            CurrentMap.gameObject.SetActive(true);
            CurrentMap.Initialize(_playerMovement, _playerClickToMove, entryId);
        }
    }
}
