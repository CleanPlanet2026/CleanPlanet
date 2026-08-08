using System;
using CleanPlanet.Core.Persistence;
using UnityEngine;

namespace CleanPlanet.Core.Progress
{
    /// <summary>
    /// 지구 정화 누적치를 소유하는 정적 상태. CurrencyWallet과 동일한 패턴으로,
    /// AppraisalService처럼 씬 인스턴스 없이도 적립할 수 있도록 정적 지급 경로를 제공한다.
    /// 맵 생성/스테이지 잠금해제 로직은 이 값을 참조만 하며 직접 갱신하지 않는다.
    /// </summary>
    public static class EarthCleanMeter
    {
        private static float _earthClean;

        public static event Action<float> EarthCleanChanged;

        public static float EarthClean => _earthClean;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            _earthClean = Mathf.Max(0f, GameSaveSystem.Data.EarthClean);
            EarthCleanChanged = null;
        }

        public static void Add(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _earthClean += amount;
            GameSaveSystem.Data.EarthClean = _earthClean;
            GameSaveSystem.MarkDirty();
            Debug.Log($"[EarthCleanMeter] +{amount} → {_earthClean}");
            EarthCleanChanged?.Invoke(_earthClean);
        }

        internal static void ResetProgress()
        {
            _earthClean = 0f;
        }
    }
}
