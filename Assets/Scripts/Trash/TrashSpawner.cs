using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.Trash
{
    public sealed class TrashSpawner : MonoBehaviour
    {
        [SerializeField] private TrashItem _trashPrefab;
        [SerializeField] private BoxCollider2D _spawnArea;
        [SerializeField, Min(1)] private int _initialTrashCount = 10;
        [SerializeField, Min(1)] private int _maximumTrashCount = 20;
        [SerializeField, Min(0.1f)] private float _spawnInterval = 2f;

        private readonly Queue<TrashItem> _pool = new();
        private int _activeTrashCount;
        private Bounds _spawnBounds;

        private void Awake()
        {
            if (_trashPrefab == null || _spawnArea == null)
            {
                Debug.LogError($"{nameof(TrashSpawner)} requires a trash prefab and spawn area.", this);
                enabled = false;
                return;
            }

            _initialTrashCount = Mathf.Min(_initialTrashCount, _maximumTrashCount);
            _spawnBounds = _spawnArea.bounds;
            _spawnArea.enabled = false;
        }

        private void Start()
        {
            for (int i = 0; i < _initialTrashCount; i++)
            {
                Spawn();
            }

            StartCoroutine(SpawnContinuously());
        }

        public void Recycle(TrashItem trash)
        {
            trash.gameObject.SetActive(false);
            _pool.Enqueue(trash);
            _activeTrashCount--;
        }

        private IEnumerator SpawnContinuously()
        {
            var wait = new WaitForSeconds(_spawnInterval);

            while (true)
            {
                yield return wait;

                if (_activeTrashCount < _maximumTrashCount)
                {
                    Spawn();
                }
            }
        }

        private void Spawn()
        {
            TrashItem trash = _pool.Count > 0
                ? _pool.Dequeue()
                : CreateTrash();

            trash.transform.position = GetRandomPosition();
            trash.gameObject.SetActive(true);
            _activeTrashCount++;
        }

        private TrashItem CreateTrash()
        {
            TrashItem trash = Instantiate(_trashPrefab, transform);
            trash.Initialize(this);
            return trash;
        }

        private Vector2 GetRandomPosition()
        {
            return new Vector2(
                UnityEngine.Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
                UnityEngine.Random.Range(_spawnBounds.min.y, _spawnBounds.max.y));
        }
    }
}
