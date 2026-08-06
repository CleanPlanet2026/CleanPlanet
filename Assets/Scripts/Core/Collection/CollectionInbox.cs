using System;
using System.Collections.Generic;
using CleanPlanet.Core.Appraisal;
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
        private static readonly List<CollectibleData> _pending = new();

        public static event Action<int> CountChanged;

        public static int Count => _pending.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            _pending.Clear();
            CountChanged = null;
        }

        public static void Add(CollectibleData item, int count)
        {
            if (item == null || count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                _pending.Add(item);
            }

            CountChanged?.Invoke(_pending.Count);
        }

        /// <summary>
        /// 쌓인 항목을 모두 꺼내고 보관소를 비운다. BaseScene의 AppraisalTank가
        /// Awake에서 한 번 호출해 자신의 대기 목록에 합친다.
        /// </summary>
        public static List<CollectibleData> GetPendingItems()
        {
            return new List<CollectibleData>(_pending);
        }

        public static void Remove(CollectibleData item)
        {
            if (item != null)
            {
                _pending.Remove(item);
                CountChanged?.Invoke(_pending.Count);
            }
        }
    }
}
