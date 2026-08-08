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
        [SerializeField] private AppraisalConfig _config;
        [SerializeField] private CollectibleData[] _pendingItems;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _iconParent;
        [SerializeField] private AppraisalFloorSensor _floorSensor;
        [SerializeField, Min(0f)] private float _spawnWidth = 40f;
        [SerializeField, Min(0.01f)] private float _spawnInterval = 0.08f;
        [SerializeField, Min(0f)] private float _consumeSlideDistance = 20f;
        [SerializeField, Min(0.01f)] private float _consumeSlideDuration = 0.35f;

        private readonly Queue<AppraisalTankIcon> _pool = new();
        private readonly List<AppraisalTankIcon> _activeIcons = new();
        private readonly List<AppraisalTankIcon> _consuming = new();
        private readonly Queue<CollectibleData> _spawnQueue = new();
        private Coroutine _spawnRoutine;

        /// <summary>
        /// 레인별로, 진행 중인 감정 건을 Started에서 이미 소비했는지 여부. Completed의 안전망이
        /// 같은 건을 중복 소비하지 않도록 HandleAppraisalStarted/Completed가 레인 인덱스로 관리한다.
        /// </summary>
        private readonly bool[] _consumedInStarted = new bool[2];

        private void Awake()
        {
            if (_iconPrefab == null || _config == null || _floorSensor == null || _spawnPoint == null)
            {
                Debug.LogError($"{nameof(AppraisalTank)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _pendingItems = MergeWithInbox(_pendingItems, _config.Catalog);
            PrewarmPool();
        }

        private void OnEnable()
        {
            AppraisalService.OnAppraisalStarted += HandleAppraisalStarted;
            AppraisalService.OnAppraisalCompleted += HandleAppraisalCompleted;
            CollectionInbox.ItemsAdded += HandleItemsAdded;
            AppraisalService.NextItemProvider = GetNextItem;
            // 패널을 껐다 켜는 사이 진행 중이던 감정의 Started/Completed 짝이 끊길 수 있으니,
            // 재활성화 시 플래그를 항상 false로 되돌려 다음 Completed가 안전망을 정상 수행하게 한다.
            Array.Clear(_consumedInStarted, 0, _consumedInStarted.Length);
            EnsureSpawnRoutine();
        }

        private void OnDisable()
        {
            AppraisalService.OnAppraisalStarted -= HandleAppraisalStarted;
            AppraisalService.OnAppraisalCompleted -= HandleAppraisalCompleted;
            CollectionInbox.ItemsAdded -= HandleItemsAdded;
            // 다른 탱크 인스턴스가 나중에 등록했다면 그걸 지우지 않도록, 지금도 자신이
            // 등록한 provider일 때만 해제한다.
            if (AppraisalService.NextItemProvider == (Func<CollectibleData>)GetNextItem)
            {
                AppraisalService.NextItemProvider = null;
            }
            // Unity가 비활성화 시 코루틴을 멈추므로 핸들을 비워 재활성화 때 다시 시작하게 한다.
            _spawnRoutine = null;

            // 소비 슬라이드 코루틴도 비활성화 때 함께 멈춰, 반투명 상태로 방치된 아이콘이
            // 다시 켰을 때 투명하게 남는다. 소비 중이던 아이콘을 즉시 풀로 회수해 정리한다.
            for (int i = _consuming.Count - 1; i >= 0; i--)
            {
                ReturnToPool(_consuming[i]);
            }

            _consuming.Clear();
        }

        /// <summary>
        /// 플레이 도중 인박스에 수집물이 새로 들어오면(수집·디버그 추가 등) 관 위 스폰
        /// 지점에서 떨어뜨려 쌓는다. 씬 진입 시 이미 있던 항목은 Start에서 처리되므로
        /// 여기서 중복 처리되지 않는다(ItemsAdded는 새 추가에만 발생).
        /// </summary>
        private void HandleItemsAdded(CollectibleData item, int count)
        {
            if (item == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                _spawnQueue.Enqueue(item);
            }

            EnsureSpawnRoutine();
        }

        /// <summary>
        /// 인스펙터에 미리 채워둔 표본 목록에 GameScene에서 수집해온 항목을 이어붙인다.
        /// 이후 스폰/풀링 로직은 출처를 구분하지 않고 동일하게 처리한다. id→CollectibleData
        /// 해석에는 AppraisalConfig의 카탈로그를 쓴다(씬에 별도 카탈로그를 두지 않는다).
        /// </summary>
        private static CollectibleData[] MergeWithInbox(
            CollectibleData[] pendingItems,
            IReadOnlyList<CollectibleData> catalog)
        {
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
            foreach (CollectibleData item in _pendingItems)
            {
                if (item != null)
                {
                    _spawnQueue.Enqueue(item);
                }
            }

            EnsureSpawnRoutine();
        }

        private void EnsureSpawnRoutine()
        {
            if (_spawnRoutine == null && isActiveAndEnabled)
            {
                _spawnRoutine = StartCoroutine(DrainSpawnQueue());
            }
        }

        /// <summary>
        /// 대기열의 항목을 관 위 스폰 지점에서 일정 간격으로 하나씩 떨어뜨린다. 시간차를 둬서
        /// 여러 개가 한 점에 겹쳐 생성돼 물리로 튕겨나가는 것을 막고, 중력으로 자연스럽게 쌓이게 한다.
        /// </summary>
        private IEnumerator DrainSpawnQueue()
        {
            var wait = new WaitForSeconds(_spawnInterval);

            while (_spawnQueue.Count > 0)
            {
                CollectibleData data = _spawnQueue.Dequeue();
                float offsetX = UnityEngine.Random.Range(-_spawnWidth * 0.5f, _spawnWidth * 0.5f);
                Vector3 position = _spawnPoint.position + new Vector3(offsetX, 0f, 0f);
                SpawnAt(data, position);
                yield return wait;
            }

            _spawnRoutine = null;
        }

        /// <summary>
        /// 서비스가 한 건의 감정을 "시작"할 때, 릴이 보여주기 시작하는 그 항목의 아이콘을
        /// 관에서 빼내 아래로 내려보낸다. 릴 표시와 파이프에서 빠지는 아이템이 같은 시점에
        /// 같은 것이 되도록 완료가 아닌 시작에 맞춘다. 같은 종류 아이콘이 없으면(아직 안 떨어졌거나
        /// 매칭 실패) 맨 아래 아이콘으로 대체한다. 여기서 실제로 소비했는지를 레인별 플래그에 남겨,
        /// 그 레인의 완료 시점 안전망이 같은 건을 중복 소비하지 않게 한다. 레인0→레인1이 같은 틱
        /// 안에서 순차 호출되므로, 레인0이 여기서 빼간 아이콘은 곧바로 이어지는 레인1 배정에는
        /// 더 이상 보이지 않는다(_activeIcons에서 즉시 제거).
        /// </summary>
        private void HandleAppraisalStarted(int lane, CollectibleData item, AppraisalResult result, float duration)
        {
            AppraisalTankIcon match = FindMatchingIcon(item) ?? FindLowestGroundedIcon();
            if (match == null)
            {
                _consumedInStarted[lane] = false;
                return;
            }

            ConsumeMatch(match);
            _consumedInStarted[lane] = true;
        }

        /// <summary>
        /// 안전망: 감정이 진행 중일 때 씬에 들어와 시작 이벤트를 놓친 경우에만 동작해야 한다.
        /// 레인별로 Started에서 이미 소비했다면(_consumedInStarted[lane]) 여기선 스킵한다. 같은
        /// 종류가 여럿 있을 때 Completed가 또 다른 아이콘을 집어 이중 소비하는 걸 막기 위함이다.
        /// </summary>
        private void HandleAppraisalCompleted(int lane, AppraisalResult result)
        {
            if (_consumedInStarted[lane])
            {
                _consumedInStarted[lane] = false;
                return;
            }

            AppraisalTankIcon match = FindMatchingIcon(result.Item);
            if (match != null)
            {
                ConsumeMatch(match);
            }

            _consumedInStarted[lane] = false;
        }

        private void ConsumeMatch(AppraisalTankIcon match)
        {
            _activeIcons.Remove(match);
            _floorSensor.Release(match);
            _consuming.Add(match);
            StartCoroutine(ConsumeIcon(match));
        }

        /// <summary>
        /// 감정에 소비된 아이콘을 즉시 없애지 않고 관 아래로 스르륵 내려가며 옅어지게 한 뒤
        /// 풀로 되돌린다. 내려가는 동안 물리는 꺼 둔다(FreezeForConsume).
        /// </summary>
        private IEnumerator ConsumeIcon(AppraisalTankIcon icon)
        {
            icon.FreezeForConsume();

            Vector3 start = icon.transform.position;
            Vector3 end = start + Vector3.down * _consumeSlideDistance;

            float elapsed = 0f;
            while (elapsed < _consumeSlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _consumeSlideDuration);
                icon.transform.position = Vector3.Lerp(start, end, t);
                icon.SetAlpha(1f - t);
                yield return null;
            }

            _consuming.Remove(icon);
            ReturnToPool(icon);
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
            _spawnQueue.Clear();
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

        private void SpawnAt(CollectibleData data, Vector3 position)
        {
            if (_pool.Count == 0)
            {
                AppraisalTankIcon extra = Instantiate(_iconPrefab, _iconParent != null ? _iconParent : transform);
                extra.gameObject.SetActive(false);
                _pool.Enqueue(extra);
            }

            AppraisalTankIcon icon = _pool.Dequeue();
            _floorSensor.Release(icon);

            icon.Setup(data, position);
            icon.gameObject.SetActive(true);
            _activeIcons.Add(icon);
        }

        /// <summary>
        /// 감정 대상과 같은 종류의 아이콘 중 y 최소를 찾는다. 바닥 안착 여부와 무관하게
        /// 찾아, 아직 낙하 중이더라도 "지금 감정 중인 바로 그 종류"를 정확히 빼낼 수 있게 한다.
        /// </summary>
        private AppraisalTankIcon FindMatchingIcon(CollectibleData item)
        {
            AppraisalTankIcon lowest = null;
            float lowestY = float.PositiveInfinity;

            foreach (AppraisalTankIcon icon in _activeIcons)
            {
                if (icon.Data != item)
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

        /// <summary>
        /// AppraisalService가 다음 감정 대상을 물을 때 호출하는 provider(NextItemProvider).
        /// 바닥에 안착한 아이콘 중 y 최솟값의 데이터를 돌려주고, 안착한 아이콘이 없으면
        /// null을 돌려줘 서비스가 기존 수집순(FIFO)으로 폴백하게 한다.
        /// </summary>
        private CollectibleData GetNextItem()
        {
            AppraisalTankIcon lowest = FindLowestGroundedIcon();
            return lowest != null ? lowest.Data : null;
        }

        /// <summary>
        /// 바닥 트리거 안에 안착한 아이콘 중 y 최소를 찾는다(종류 매칭 실패 시 대체용).
        /// 아직 낙하 중인 아이콘은 제외해 공중에서 사라지지 않게 한다.
        /// </summary>
        private AppraisalTankIcon FindLowestGroundedIcon()
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
            // 소비 슬라이드로 옅어진 상태가 남지 않도록 알파를 복구해 두고 회수한다.
            icon.SetAlpha(1f);
            icon.gameObject.SetActive(false);
            _pool.Enqueue(icon);
        }
    }
}
