using System;
using System.Collections;
using System.Collections.Generic;
using CleanPlanet.Core.Collection;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 대기 중인 수집물을 유리관 안에 물리로 쌓아두고 시각적으로만 보여주는 뷰.
    /// 실제로 어느 항목을 언제 감정할지는 AppraisalService가 CollectionInbox를 직접 보고
    /// 결정하며, 이 탱크는 그 결과(OnAppraisalCompleted)를 받아 바닥 아이콘을 치우는
    /// 표시 전담이다. 아이콘 오브젝트는 시작 시 풀링해두고 런타임 Instantiate/Destroy 없이
    /// 재사용한다.
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

        private void OnEnable()
        {
            AppraisalService.OnAppraisalCompleted += HandleAppraisalCompleted;
        }

        private void OnDisable()
        {
            AppraisalService.OnAppraisalCompleted -= HandleAppraisalCompleted;
        }

        /// <summary>
        /// 인스펙터에 미리 채워둔 표본 목록에 GameScene에서 수집해온 항목을 이어붙인다.
        /// 이후 스폰/풀링 로직은 출처를 구분하지 않고 동일하게 처리한다. id→CollectibleData
        /// 해석에는 AppraisalConfig의 카탈로그를 쓴다(씬에 별도 카탈로그를 두지 않는다).
        /// </summary>
        private static CollectibleData[] MergeWithInbox(CollectibleData[] pendingItems)
        {
            IReadOnlyList<CollectibleData> catalog = AppraisalConfig.Instance != null
                ? AppraisalConfig.Instance.Catalog
                : null;

            List<CollectibleData> collected = CollectionInbox.GetPendingItems(catalog);
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
        /// 서비스가 감정을 완료한 시점에 그 항목과 같은 아이콘을(없으면 맨 아래 아이콘을)
        /// 관에서 치운다. 관은 표시 전담이라 매칭이 어긋나도 게임 진행에는 영향이 없다.
        /// </summary>
        private void HandleAppraisalCompleted(AppraisalResult result)
        {
            AppraisalTankIcon match = FindLowestIcon(icon => icon.Data == result.Item) ?? FindLowestIcon();
            if (match == null)
            {
                return;
            }

            _activeIcons.Remove(match);
            ReturnToPool(match);
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
        /// 바닥 트리거(_floorSensor) 안에 들어와 안정된 아이콘 중에서(선택적으로 predicate를
        /// 만족하는 것 중에서) y 최소를 찾는다. 아직 낙하 중인 아이콘은 후보에서 제외되어
        /// 공중에서 사라지지 않는다.
        /// </summary>
        private AppraisalTankIcon FindLowestIcon(Func<AppraisalTankIcon, bool> predicate = null)
        {
            AppraisalTankIcon lowest = null;
            float lowestY = float.PositiveInfinity;

            foreach (AppraisalTankIcon icon in _activeIcons)
            {
                if (!_floorSensor.Contains(icon))
                {
                    continue;
                }

                if (predicate != null && !predicate(icon))
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
