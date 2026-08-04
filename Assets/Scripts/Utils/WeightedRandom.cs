using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.Utils
{
    public static class WeightedRandom
    {
        public delegate float WeightSelector<T>(T item);

        /// <summary>
        /// 가중치 합 기준으로 무작위 항목 하나를 뽑는다. 전체 가중치 합이 0 이하이면 default를 반환한다.
        /// </summary>
        public static T Pick<T>(IReadOnlyList<T> items, WeightSelector<T> weightOf)
        {
            float total = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                total += weightOf(items[i]);
            }

            if (total <= 0f)
            {
                return default;
            }

            float roll = Random.Range(0f, total);
            float cumulative = 0f;

            for (int i = 0; i < items.Count; i++)
            {
                cumulative += weightOf(items[i]);
                if (roll <= cumulative)
                {
                    return items[i];
                }
            }

            return items[items.Count - 1];
        }
    }
}
