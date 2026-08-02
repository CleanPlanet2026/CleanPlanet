using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 대기 중인 수집물을 유리관 안에 물리로 쌓아두는 공급원.
    /// 감정이 필요할 때마다 관 맨 아래(y 최소) 아이콘을 제거해 그 CollectibleData를 넘긴다.
    /// 아이콘 오브젝트는 시작 시 풀링해두고 런타임 Instantiate/Destroy 없이 재사용한다.
    /// </summary>
    public sealed class AppraisalTank : MonoBehaviour
    {
        [SerializeField] private AppraisalTankIcon _iconPrefab;
        [SerializeField] private CollectibleData[] _pendingItems;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _iconParent;
        [SerializeField, Min(0f)] private float _spawnWidth = 1f;
        [SerializeField, Min(0.01f)] private float _spawnInterval = 0.04f;

        private readonly Queue<AppraisalTankIcon> _pool = new();
        private readonly List<AppraisalTankIcon> _activeIcons = new();

        private void Awake()
        {
            if (_iconPrefab == null || _spawnPoint == null || _pendingItems == null || _pendingItems.Length == 0)
            {
                Debug.LogError($"{nameof(AppraisalTank)}에 필요한 참조 또는 대기 목록이 없습니다.", this);
                enabled = false;
                return;
            }

            PrewarmPool();
        }

        private void Start()
        {
            StartCoroutine(SpawnPendingItems());
        }

        /// <summary>
        /// 관 맨 아래(y 최소) 아이콘을 제거하고 그 데이터를 반환한다.
        /// 제거와 반환이 한 호출 안에서 함께 일어나므로 "빠지는 것=감정되는 것"이 항상 일치한다.
        /// </summary>
        public bool TryTakeBottomItem(out CollectibleData item)
        {
            AppraisalTankIcon lowest = FindLowestIcon();
            if (lowest == null)
            {
                item = null;
                return false;
            }

            item = lowest.Data;
            _activeIcons.Remove(lowest);
            ReturnToPool(lowest);
            return true;
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _pendingItems.Length; i++)
            {
                AppraisalTankIcon icon = Instantiate(_iconPrefab, _iconParent != null ? _iconParent : transform);
                icon.gameObject.SetActive(false);
                _pool.Enqueue(icon);
            }
        }

        private IEnumerator SpawnPendingItems()
        {
            var tick = new WaitForSeconds(_spawnInterval);

            foreach (CollectibleData data in _pendingItems)
            {
                SpawnOne(data);
                yield return tick;
            }
        }

        private void SpawnOne(CollectibleData data)
        {
            if (_pool.Count == 0)
            {
                Debug.LogWarning($"{nameof(AppraisalTank)}: 풀이 부족해 {data.Name}을(를) 스폰하지 못했습니다.", this);
                return;
            }

            AppraisalTankIcon icon = _pool.Dequeue();
            float offsetX = Random.Range(-_spawnWidth * 0.5f, _spawnWidth * 0.5f);
            Vector3 spawnPosition = _spawnPoint.position + new Vector3(offsetX, 0f, 0f);

            icon.Setup(data, spawnPosition);
            icon.gameObject.SetActive(true);
            _activeIcons.Add(icon);
        }

        private AppraisalTankIcon FindLowestIcon()
        {
            AppraisalTankIcon lowest = null;
            float lowestY = float.PositiveInfinity;

            foreach (AppraisalTankIcon icon in _activeIcons)
            {
                float y = icon.transform.position.y;
                if (y < lowestY)
                {
                    lowestY = y;
                    lowest = icon;
                }
            }

            return lowest;
        }

        private void ReturnToPool(AppraisalTankIcon icon)
        {
            icon.gameObject.SetActive(false);
            _pool.Enqueue(icon);
        }
    }
}
