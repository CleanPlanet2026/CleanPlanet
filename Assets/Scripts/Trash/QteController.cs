using System;
using CleanPlanet.Upgrade;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// TrashPile 보상 계산 등 결과를 하나의 값으로 다뤄야 하는 곳에서 사용하는 판정 결과.
    /// </summary>
    public enum QteResult
    {
        Fail,
        Success,
        GreatSuccess
    }

    /// <summary>
    /// Dead by Daylight의 Skill Check와 동일하게 동작하는 원형 QTE.
    /// Needle이 0도(12시 방향)에서 시작해 시계 방향으로 한 바퀴 도는 동안 Space를 한 번 눌러 판정한다.
    /// 입력 즉시 종료되며, 한 바퀴를 다 돌 때까지 입력이 없으면 실패로 종료된다.
    /// 매 StartQte() 호출마다 Success/Great Zone을 새로 랜덤 배치한다.
    /// </summary>
    public class QteController : MonoBehaviour
    {
        [Header("속도/폭")]
        [SerializeField, Min(0f)] private float _needleSpeed = 250f; // deg/sec
        [SerializeField, Range(1f, 360f)] private float _successWidth = 35f; // deg
        [SerializeField, Range(1f, 360f)] private float _greatWidth = 10f; // deg

        [Header("시작 각도")]
        [SerializeField] private bool _randomizeStartAngle = true;
        [SerializeField, Range(0f, 360f)] private float _fixedStartAngle; // _randomizeStartAngle == false일 때만 사용

        [Header("테스트")]
        [SerializeField] private bool _autoStart; // 켜두면 활성화 즉시 QTE를 시작해 단독 테스트 가능

        // 0~360, 시계 방향 증가. 0도는 Needle 시작 위치(12시 방향)를 의미하며 한 바퀴(360도)를 넘지 않는다.
        public bool IsActive { get; private set; }
        public float NeedleAngle { get; private set; }
        public float SuccessStartAngle { get; private set; }
        public float SuccessEndAngle { get; private set; }
        public float GreatStartAngle { get; private set; }
        public float GreatEndAngle { get; private set; }

        public event Action OnQTEStarted;
        public event Action OnGreatSuccess;
        public event Action OnSuccess;
        public event Action OnFail;

        private InputAction _pressAction;
        private bool _consumedThisRun;

        private void Awake()
        {
            _pressAction = new InputAction(binding: "<Keyboard>/space");
            _pressAction.performed += OnPressPerformed;
        }

        private void OnEnable()
        {
            _pressAction.Enable();

            if (_autoStart)
            {
                StartQte();
            }
        }

        private void OnDisable()
        {
            _pressAction.Disable();
        }

        private void OnDestroy()
        {
            _pressAction.performed -= OnPressPerformed;
            _pressAction.Dispose();
        }

        /// <summary>
        /// QTE를 시작한다. Success/Great Zone을 새로 랜덤 배치하고 Needle을 0도로 되돌린다.
        /// 이미 진행 중이면 무시한다.
        /// </summary>
        public void StartQte()
        {
            if (IsActive)
            {
                return;
            }

            PlaceZones();
            NeedleAngle = 0f;
            _consumedThisRun = false;
            IsActive = true;
            OnQTEStarted?.Invoke();
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            NeedleAngle += _needleSpeed * Time.deltaTime;

            if (NeedleAngle >= 360f)
            {
                // 한 바퀴를 다 돌 때까지 입력이 없었다는 뜻 — 실패로 즉시 종료한다.
                NeedleAngle = 360f;
                Resolve(QteResult.Fail);
            }
        }

        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            if (!IsActive || _consumedThisRun)
            {
                return;
            }

            _consumedThisRun = true;

            if (NeedleAngle >= GreatStartAngle && NeedleAngle <= GreatEndAngle)
            {
                Resolve(QteResult.GreatSuccess);
            }
            else if (NeedleAngle >= SuccessStartAngle && NeedleAngle <= SuccessEndAngle)
            {
                Resolve(QteResult.Success);
            }
            else
            {
                Resolve(QteResult.Fail);
            }
        }

        private void Resolve(QteResult result)
        {
            IsActive = false;

            switch (result)
            {
                case QteResult.GreatSuccess:
                    OnGreatSuccess?.Invoke();
                    break;
                case QteResult.Success:
                    OnSuccess?.Invoke();
                    break;
                default:
                    OnFail?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Success Zone 시작 각도를 정하고(랜덤 또는 고정), 그 안에 Great Zone을 랜덤 배치한다.
        /// Success Zone은 0~360 범위를 넘어가지 않도록(래핑 없이) 배치해 각도 비교를 단순하게 유지한다.
        /// </summary>
        private void PlaceZones()
        {
            float successWidth = Mathf.Clamp(
                _successWidth * UpgradeEffects.CollectionQteWidthMultiplier,
                1f,
                360f);
            SuccessStartAngle = _randomizeStartAngle
                ? UnityEngine.Random.Range(0f, 360f - successWidth)
                : Mathf.Min(_fixedStartAngle, 360f - successWidth);
            SuccessEndAngle = SuccessStartAngle + successWidth;

            float greatWidth = Mathf.Min(_greatWidth, successWidth);
            float greatOffset = UnityEngine.Random.Range(0f, successWidth - greatWidth);
            GreatStartAngle = SuccessStartAngle + greatOffset;
            GreatEndAngle = GreatStartAngle + greatWidth;
        }
    }
}
