using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class RobotProcessingPreview : MonoBehaviour
    {
        private static readonly string[] ItemNames = { "고철", "유리", "전자품", "보석" };
        private static readonly Color[] ItemColors =
        {
            new(0.55f, 0.60f, 0.66f),
            new(0.36f, 0.78f, 0.90f),
            new(0.96f, 0.62f, 0.20f),
            new(0.78f, 0.36f, 0.92f)
        };
        private static readonly int[] Multipliers = { 1, 2, 4, 8, 16 };

        [SerializeField] private GameObject _collectiblePrefab;
        [SerializeField] private RectTransform _collectibleContainer;
        [SerializeField] private RectTransform _spawnPoint;
        [SerializeField] private Text _rouletteText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _goldGainText;
        [SerializeField, Min(1)] private int _initialPoolSize = 20;
        [SerializeField, Min(1)] private int _previewItemCount = 20;
        [SerializeField, Min(0.02f)] private float _spawnInterval = 0.08f;
        [SerializeField, Min(0.02f)] private float _rouletteStep = 0.05f;
        [SerializeField, Min(0f)] private float _spawnWidth = 36f;
        [SerializeField] private Vector2 _initialVelocity = new(-35f, -80f);
        [SerializeField, Min(0f)] private float _horizontalVelocityVariation = 70f;

        private readonly Queue<ProcessingCollectible> _pool = new();
        private Coroutine _spawnCoroutine;
        private int _nextItemIndex;
        private int _rouletteIndex;
        private int _previewGold;
        private float _rouletteElapsed;

        public int CurrentMultiplier => Multipliers[_rouletteIndex];

        private void OnEnable()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError($"{nameof(RobotProcessingPreview)} requires all UI references.", this);
                enabled = false;
                return;
            }

            EnsurePool();
            UpdateDisplays();
            _spawnCoroutine = StartCoroutine(SpawnContinuously());
        }

        private void Update()
        {
            _rouletteElapsed += Time.deltaTime;

            if (_rouletteElapsed < _rouletteStep)
            {
                return;
            }

            _rouletteElapsed %= _rouletteStep;
            _rouletteIndex = (_rouletteIndex + 1) % Multipliers.Length;
            _rouletteText.text = $"x{CurrentMultiplier}";
        }

        private void OnDisable()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        public void Convert(ProcessingCollectible collectible)
        {
            int gainedGold = collectible.BaseGold * CurrentMultiplier;
            _previewGold += gainedGold;
            _goldText.text = $"골드 {_previewGold:N0}";
            _goldGainText.text = $"+{gainedGold} 골드 (x{CurrentMultiplier})";
            ReturnToPool(collectible);
        }

        private IEnumerator SpawnContinuously()
        {
            WaitForSeconds interval = new(_spawnInterval);

            for (int spawnedItemCount = 0; spawnedItemCount < _previewItemCount; spawnedItemCount++)
            {
                SpawnCollectible();
                yield return interval;
            }

            _spawnCoroutine = null;
        }

        private void SpawnCollectible()
        {
            ProcessingCollectible collectible = GetFromPool();
            int itemIndex = _nextItemIndex % ItemNames.Length;
            float spawnOffset = Random.Range(-_spawnWidth, _spawnWidth);
            float horizontalVelocity = _initialVelocity.x +
                Random.Range(-_horizontalVelocityVariation, _horizontalVelocityVariation);

            collectible.transform.SetParent(_collectibleContainer, false);
            collectible.RectTransform.anchoredPosition =
                _spawnPoint.anchoredPosition + Vector2.right * spawnOffset;
            collectible.Initialize(
                this,
                ItemNames[itemIndex],
                ItemColors[itemIndex],
                (itemIndex + 1) * 10,
                new Vector2(horizontalVelocity, _initialVelocity.y),
                Random.Range(-180f, 180f));
            _nextItemIndex++;
        }

        private ProcessingCollectible GetFromPool()
        {
            if (_pool.Count == 0)
            {
                return CreateCollectible();
            }

            return _pool.Dequeue();
        }

        private void ReturnToPool(ProcessingCollectible collectible)
        {
            collectible.Deactivate();
            _pool.Enqueue(collectible);
        }

        private void EnsurePool()
        {
            while (_pool.Count < _initialPoolSize)
            {
                ProcessingCollectible collectible = CreateCollectible();
                collectible.Deactivate();
                _pool.Enqueue(collectible);
            }
        }

        private ProcessingCollectible CreateCollectible()
        {
            GameObject instance = Instantiate(_collectiblePrefab, _collectibleContainer);
            return instance.GetComponent<ProcessingCollectible>();
        }

        private void UpdateDisplays()
        {
            _rouletteText.text = $"x{CurrentMultiplier}";
            _goldText.text = $"골드 {_previewGold:N0}";
            _goldGainText.text = string.Empty;
        }

        private bool HasRequiredReferences()
        {
            return _collectiblePrefab != null &&
                _collectibleContainer != null &&
                _spawnPoint != null &&
                _rouletteText != null &&
                _goldText != null &&
                _goldGainText != null;
        }
    }
}
