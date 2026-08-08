using System;
using System.Collections;
using System.Collections.Generic;
using CleanPlanet.Core.Collection;
using CleanPlanet.Core.Currency;
using CleanPlanet.Upgrade;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// CollectionInbox에 쌓인 수집물을 씬과 무관하게 백그라운드로 감정해 골드를 지급하는
    /// 영속 서비스. DontDestroyOnLoad 오브젝트로 부팅되어 어느 씬(GameScene 포함)에 있든
    /// 동일한 속도로 진행된다. BaseScene의 릴·탱크·이펙트는 이 서비스가 발행하는 이벤트를
    /// 구독해 화면에 보여주는 뷰일 뿐, 감정 계산이나 지급 타이밍을 직접 소유하지 않는다.
    /// 레인 0은 항상 동작하고, 레인 1은 appraisal_second_reel 업그레이드가 있을 때만
    /// 배정되어 두 건을 동시에 진행한다(처리량 2배).
    /// </summary>
    public sealed class AppraisalService : MonoBehaviour
    {
        private const string AppraisalSceneName = "BaseScene";
        private const int LaneCount = 2;

        /// <summary>레인 하나의 진행 상태. Busy는 배정~hold 종료까지, HasActive는 배정~지급 직전까지만 true다(기존 단일 레인의 페이싱을 그대로 보존).</summary>
        private struct LaneState
        {
            public bool Busy;
            public bool HasActive;
            public AppraisalResult ActiveResult;
            public float ActiveStartTime;
            public float ActiveDuration;
        }

        [SerializeField, Min(0.05f)] private float _idlePollInterval = 0.2f;

        /// <summary>감정 한 건이 시작될 때 발행된다(레인 인덱스 포함). BaseScene의 릴이 이 결과로 스핀 연출을 시작한다.</summary>
        public static event Action<int, CollectibleData, AppraisalResult, float> OnAppraisalStarted;

        /// <summary>감정이 끝나 골드가 지급된 직후 발행된다(레인 인덱스 포함). 탱크가 이 시점에 아이콘을 정리한다.</summary>
        public static event Action<int, AppraisalResult> OnAppraisalCompleted;

        /// <summary>
        /// 다음으로 감정할 대상을 뷰(AppraisalTank)에게 물어보는 훅. 관에 바닥 안착한 아이콘
        /// 중 y 최솟값의 데이터를 돌려주도록 AppraisalTank가 등록한다. 등록된 게 없거나
        /// null을 반환하면 서비스는 기존 수집순(FIFO)으로 폴백한다. 레인0→레인1을 같은 틱에서
        /// 순차로 배정하고 그때마다 탱크가 동기적으로 아이콘을 빼가므로, 이 훅에 레인 구분은
        /// 필요 없다(같은 물리 아이콘이 두 번 반환되지 않는다).
        /// </summary>
        public static Func<CollectibleData> NextItemProvider;

        private static LaneState[] _lanes = new LaneState[LaneCount];

        /// <summary>지금 어느 레인이든 이미 물고 있는 항목들. 탱크 없이(FIFO 폴백) 두 레인이 동시에
        /// 같은 인박스 항목을 집지 않도록 pending 목록에서 빼는 데 쓴다(멀티셋 차감).</summary>
        private static readonly List<CollectibleData> _inFlightItems = new();

        private bool _warnedMissingConfig;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            OnAppraisalStarted = null;
            OnAppraisalCompleted = null;
            NextItemProvider = null;
            _lanes = new LaneState[LaneCount];
            _inFlightItems.Clear();

            var runner = new GameObject("Appraisal Service");
            DontDestroyOnLoad(runner);
            runner.AddComponent<AppraisalService>();
        }

        /// <summary>
        /// 지정한 레인에서 지금 감정이 진행 중이면 그 결과와 남은 시간을 돌려준다. BaseScene 뷰가
        /// 씬 로드나 탭 복귀(또는 레인1의 늦은 언락) 시점에 이미 진행 중이던 감정을 놓치지 않고
        /// 릴 연출을 복원하는 데 쓴다.
        /// </summary>
        public static bool TryGetActiveAppraisal(int lane, out AppraisalResult result, out float remainingDuration)
        {
            if (!_lanes[lane].HasActive)
            {
                result = default;
                remainingDuration = 0f;
                return false;
            }

            result = _lanes[lane].ActiveResult;
            remainingDuration = Mathf.Max(0f, _lanes[lane].ActiveDuration - (Time.time - _lanes[lane].ActiveStartTime));
            return true;
        }

        private void Start()
        {
            StartCoroutine(RunLoop());
        }

        private IEnumerator RunLoop()
        {
            var idleWait = new WaitForSeconds(_idlePollInterval);

            while (true)
            {
                if (SceneManager.GetActiveScene().name != AppraisalSceneName)
                {
                    yield return idleWait;
                    continue;
                }

                AppraisalConfig config = AppraisalConfig.Instance;
                if (config == null)
                {
                    if (!_warnedMissingConfig)
                    {
                        Debug.LogWarning($"{nameof(AppraisalService)}: {nameof(AppraisalConfig)}가 " +
                            "Preloaded Assets에 등록되어 있지 않아 감정을 대기합니다.", this);
                        _warnedMissingConfig = true;
                    }

                    yield return idleWait;
                    continue;
                }

                // 레인0→레인1 순서로, 같은 틱 안에서(yield 없이) 순차 배정한다. 레인0이 배정되면
                // OnAppraisalStarted 구독자(탱크)가 동기적으로 아이콘/인박스 상태를 갱신하므로,
                // 바로 이어지는 레인1 배정은 자연히 다른 대상을 보게 된다.
                for (int lane = 0; lane < LaneCount; lane++)
                {
                    if (_lanes[lane].Busy)
                    {
                        continue;
                    }

                    if (lane == 1 && !UpgradeEffects.SecondAppraisalReelUnlocked)
                    {
                        continue;
                    }

                    CollectibleData next = PeekNextItem(config.Catalog);
                    if (next == null)
                    {
                        continue;
                    }

                    AppraisalResult result = config.Appraise(next);
                    float duration = config.AppraisalDuration;

                    _lanes[lane].Busy = true;
                    _lanes[lane].HasActive = true;
                    _lanes[lane].ActiveResult = result;
                    _lanes[lane].ActiveStartTime = Time.time;
                    _lanes[lane].ActiveDuration = duration;
                    _inFlightItems.Add(next);

                    OnAppraisalStarted?.Invoke(lane, next, result, duration);
                    StartCoroutine(RunLaneAppraisal(lane, next, result, duration, config.ResultHoldDuration));
                }

                yield return idleWait;
            }
        }

        /// <summary>
        /// 배정된 감정 한 건의 나머지 진행(대기→지급→완료 알림→결과 홀드)을 레인별로 독립적으로
        /// 처리한다. Busy는 홀드가 끝날 때까지 유지해, 결과를 보여주는 동안 같은 레인이 곧바로
        /// 다음 건에 재배정되지 않게 한다(기존 단일 레인 페이싱과 동일).
        /// </summary>
        private IEnumerator RunLaneAppraisal(
            int lane, CollectibleData item, AppraisalResult result, float duration, float resultHoldDuration)
        {
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            CollectionInbox.Remove(item);
            CurrencyWallet.AddStatic(result.Payout);
            _inFlightItems.Remove(item);
            _lanes[lane].HasActive = false;

            OnAppraisalCompleted?.Invoke(lane, result);

            if (resultHoldDuration > 0f)
            {
                yield return new WaitForSeconds(resultHoldDuration);
            }

            _lanes[lane].Busy = false;
        }

        /// <summary>
        /// 관 바닥에 안착한 아이콘 순서(NextItemProvider)를 우선 따르고, 관이 없거나
        /// 안착한 아이콘이 없거나 인박스와 어긋나면 기존 수집순(FIFO)으로 폴백한다.
        /// 이미 다른 레인이 물고 있는 항목(_inFlightItems)은 후보에서 제외한다.
        /// </summary>
        private static CollectibleData PeekNextItem(IReadOnlyList<CollectibleData> catalog)
        {
            if (CollectionInbox.Count == 0)
            {
                return null;
            }

            List<CollectibleData> pending = CollectionInbox.GetPendingItems(catalog);
            foreach (CollectibleData claimed in _inFlightItems)
            {
                pending.Remove(claimed);
            }

            if (pending.Count == 0)
            {
                return null;
            }

            CollectibleData candidate = NextItemProvider?.Invoke();
            if (candidate != null && pending.Contains(candidate))
            {
                return candidate;
            }

            return pending[0];
        }
    }
}
