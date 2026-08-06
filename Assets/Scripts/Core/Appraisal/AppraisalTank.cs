using System;
using System.Collections;
using System.Collections.Generic;
using CleanPlanet.Core.Collection;
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
        [SerializeField] private AppraisalFloorSensor _floorSensor;
        [SerializeField, Min(0f)] private float _spawnWidth = 1f;
        [SerializeField, Min(0.01f)] private float _spawnInterval = 0.04f;

        private readonly Queue<AppraisalTankIcon> _pool = new();
        private readonly List<AppraisalTankIcon> _activeIcons = new();
        private int _unspawnedCount;

        /// <summary>
        /// 아직 스폰되지 않았거나(대기 중) 관 안에 남아있는(공중이든 바닥이든) 항목이 있으면 true.
        /// Sequencer가 "집을 게 잠깐 없을 뿐"과 "완전히 소진됨"을 구분하는 데 사용한다.
        /// </summary>
        public bool HasRemaining => _activeIcons.Count > 0 || _unspawnedCount > 0;

        private void Awake()
        {
            if (_iconPrefab == null || _spawnPoint == null || _floorSensor == null)
            {
                Debug.LogError($"{nameof(AppraisalTank)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _pendingItems = MergeWithInbox(_pendingItems);
            _unspawnedCount = _pendingItems.Length;
            PrewarmPool();
        }

        /// <summary>
        /// 인스펙터에 미리 채워둔 표본 목록에 GameScene에서 수집해온 항목을 이어붙인다.
        /// 이후 스폰/풀링 로직은 출처를 구분하지 않고 동일하게 처리한다.
        /// </summary>
        private static CollectibleData[] MergeWithInbox(CollectibleData[] pendingItems)
        {
            List<CollectibleData> collected = CollectionInbox.GetPendingItems();
            if (collected.Count == 0)
            {
                return pendingItems ?? Array.Empty<CollectibleData>();
            }

            var merged = new List<CollectibleData>(pendingItems ?? Array.Empty<CollectibleData>());
            merged.AddRange(collected);
            return merged.ToArray();
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
            CollectionInbox.Remove(item);
            _activeIcons.Remove(lowest);
            ReturnToPool(lowest);
            return true;
        }

        /// <summary>
        /// 관 안의 아이콘을 전부 즉시 회수한다(디버그 패널의 "관 비우기" 등).
        /// 감정 스핀·배수·payout 계산에는 관여하지 않는다.
        /// </summary>
        public void Clear()
        {
            for (int i = _activeIcons.Count - 1; i >= 0; i--)
            {
                ReturnToPool(_activeIcons[i]);
            }

            _activeIcons.Clear();
            _unspawnedCount = 0;
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
            _unspawnedCount--;

            if (_pool.Count == 0)
            {
                Debug.LogWarning($"{nameof(AppraisalTank)}: 풀이 부족해 {data.Name}을(를) 스폰하지 못했습니다.", this);
                return;
            }

            AppraisalTankIcon icon = _pool.Dequeue();
            _floorSensor.Release(icon);

            float offsetX = UnityEngine.Random.Range(-_spawnWidth * 0.5f, _spawnWidth * 0.5f);
            Vector3 spawnPosition = _spawnPoint.position + new Vector3(offsetX, 0f, 0f);

            icon.Setup(data, spawnPosition);
            icon.gameObject.SetActive(true);
            _activeIcons.Add(icon);
        }

        /// <summary>
        /// 바닥 트리거(_floorSensor) 안에 들어와 안정된 아이콘 중에서만 y 최소를 찾는다.
        /// 아직 낙하 중인 아이콘은 후보에서 제외되어 공중에서 사라지지 않는다.
        /// </summary>
        private AppraisalTankIcon FindLowestIcon()
        {
            AppraisalTankIcon lowest = null;
            float lowestY = float.PositiveInfinity;

            foreach (AppraisalTankIcon icon in _activeIcons)
            {
                if (!_floorSensor.Contains(icon))
                {
                    continue;
                }

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
