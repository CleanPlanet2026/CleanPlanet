using System;
using System.Collections.Generic;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Core.Persistence;
using UnityEngine;

namespace CleanPlanet.Core.Collection
{
    /// <summary>
    /// GameScene에서 수집한 CollectibleData를 씬 전환 이후에도 유지해 BaseScene의
    /// AppraisalTank로 넘기는 정적 보관소. 인스턴스를 만들지 않는 순수 메모리 저장소이며
    /// Play Mode가 끝나거나 게임이 종료되면 함께 초기화된다.
    /// </summary>
    public static class CollectionInbox
    {
        private static readonly List<string> _pendingIds = new();

        public static event Action<int> CountChanged;
        public static event Action<CollectibleData, int> ItemsAdded;

        public static int Count => _pendingIds.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            _pendingIds.Clear();
            _pendingIds.AddRange(GameSaveSystem.Data.PendingCollectibleIds);
            CountChanged = null;
            ItemsAdded = null;
        }

        public static void Add(CollectibleData item, int count)
        {
            if (item == null || count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                _pendingIds.Add(item.PersistenceId);
            }

            SavePendingItems();
            CountChanged?.Invoke(_pendingIds.Count);
            ItemsAdded?.Invoke(item, count);
        }

        /// <summary>
        /// 쌓인 항목을 모두 꺼내고 보관소를 비운다. BaseScene의 AppraisalTank가
        /// Awake에서 한 번 호출해 자신의 대기 목록에 합친다.
        /// </summary>
        public static List<CollectibleData> GetPendingItems(IEnumerable<CollectibleData> catalog)
        {
            var byId = new Dictionary<string, CollectibleData>();
            if (catalog != null)
            {
                foreach (CollectibleData item in catalog)
                {
                    if (item != null)
                    {
                        byId[item.PersistenceId] = item;
                    }
                }
            }

            var items = new List<CollectibleData>(_pendingIds.Count);
            foreach (string id in _pendingIds)
            {
                if (byId.TryGetValue(id, out CollectibleData item))
                {
                    items.Add(item);
                }
                else
                {
                    Debug.LogWarning($"Saved collectible '{id}' is missing from the catalog.");
                }
            }

            return items;
        }

        public static void Remove(CollectibleData item)
        {
            if (item != null)
            {
                _pendingIds.Remove(item.PersistenceId);
                SavePendingItems();
                CountChanged?.Invoke(_pendingIds.Count);
            }
        }

        private static void SavePendingItems()
        {
            GameSaveSystem.Data.PendingCollectibleIds.Clear();
            GameSaveSystem.Data.PendingCollectibleIds.AddRange(_pendingIds);
            GameSaveSystem.MarkDirty();
        }

        internal static void ResetProgress()
        {
            _pendingIds.Clear();
        }
    }
}
