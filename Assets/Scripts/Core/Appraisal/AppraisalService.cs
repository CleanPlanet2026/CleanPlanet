using System;
using System.Collections;
using System.Collections.Generic;
using CleanPlanet.Core.Collection;
using CleanPlanet.Core.Currency;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// CollectionInbox에 쌓인 수집물을 씬과 무관하게 백그라운드로 감정해 골드를 지급하는
    /// 영속 서비스. DontDestroyOnLoad 오브젝트로 부팅되어 어느 씬(GameScene 포함)에 있든
    /// 동일한 속도로 진행된다. BaseScene의 릴·탱크·이펙트는 이 서비스가 발행하는 이벤트를
    /// 구독해 화면에 보여주는 뷰일 뿐, 감정 계산이나 지급 타이밍을 직접 소유하지 않는다.
    /// </summary>
    public sealed class AppraisalService : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _idlePollInterval = 0.2f;

        /// <summary>감정 한 건이 시작될 때 발행된다. BaseScene의 릴이 이 결과로 스핀 연출을 시작한다.</summary>
        public static event Action<CollectibleData, AppraisalResult, float> OnAppraisalStarted;

        /// <summary>감정이 끝나 골드가 지급된 직후 발행된다. 탱크가 이 시점에 아이콘을 정리한다.</summary>
        public static event Action<AppraisalResult> OnAppraisalCompleted;

        private static bool _hasActiveAppraisal;
        private static AppraisalResult _activeResult;
        private static float _activeStartTime;
        private static float _activeDuration;

        private bool _warnedMissingConfig;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            OnAppraisalStarted = null;
            OnAppraisalCompleted = null;
            _hasActiveAppraisal = false;

            var runner = new GameObject("Appraisal Service");
            DontDestroyOnLoad(runner);
            runner.AddComponent<AppraisalService>();
        }

        /// <summary>
        /// 지금 감정이 진행 중이면 그 결과와 남은 시간을 돌려준다. BaseScene 뷰가 씬 로드나
        /// 탭 복귀 시점에 이미 진행 중이던 감정을 놓치지 않고 릴 연출을 복원하는 데 쓴다.
        /// </summary>
        public static bool TryGetActiveAppraisal(out AppraisalResult result, out float remainingDuration)
        {
            if (!_hasActiveAppraisal)
            {
                result = default;
                remainingDuration = 0f;
                return false;
            }

            result = _activeResult;
            remainingDuration = Mathf.Max(0f, _activeDuration - (Time.time - _activeStartTime));
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

                CollectibleData next = PeekNextItem(config.Catalog);
                if (next == null)
                {
                    yield return idleWait;
                    continue;
                }

                AppraisalResult result = config.Appraise(next);
                float duration = config.AppraisalDuration;

                _hasActiveAppraisal = true;
                _activeResult = result;
                _activeStartTime = Time.time;
                _activeDuration = duration;

                OnAppraisalStarted?.Invoke(next, result, duration);

                if (duration > 0f)
                {
                    yield return new WaitForSeconds(duration);
                }

                CollectionInbox.Remove(next);
                CurrencyWallet.AddStatic(result.Payout);
                _hasActiveAppraisal = false;

                OnAppraisalCompleted?.Invoke(result);

                if (config.ResultHoldDuration > 0f)
                {
                    yield return new WaitForSeconds(config.ResultHoldDuration);
                }
            }
        }

        private static CollectibleData PeekNextItem(IReadOnlyList<CollectibleData> catalog)
        {
            if (CollectionInbox.Count == 0)
            {
                return null;
            }

            List<CollectibleData> pending = CollectionInbox.GetPendingItems(catalog);
            return pending.Count > 0 ? pending[0] : null;
        }
    }
}
