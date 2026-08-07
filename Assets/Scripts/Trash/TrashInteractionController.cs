using System;
using CleanPlanet.Upgrade;
using UnityEngine;
using CleanPlanet.Player;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// 더미 선택 → 로봇 도착 감지 → QTE 시작 → 보상 지급 및 더미 제거로 이어지는
    /// 한 번의 수집 상호작용을 이벤트로 조율한다. QTE 자체는 더미를 모르고,
    /// 더미는 QTE를 모르도록 이 컨트롤러가 둘을 연결하는 역할만 한다.
    /// </summary>
    public class TrashInteractionController : MonoBehaviour
    {
        [SerializeField] private PlayerClickToMove _clickToMove;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private QteController _qte;

        public event Action<TrashPile> OnRobotArrivedToTrash;
        public event Action<TrashPile, int> OnCollectibleRewardGranted;

        private TrashPile _pendingTrash;
        private RobotSpriteAnimator _robotAnimator;

        private void Awake()
        {
            _robotAnimator = _movement.GetComponent<RobotSpriteAnimator>();
        }

        private void OnEnable()
        {
            _clickToMove.OnTrashSelected += HandleTrashSelected;
            _movement.OnRobotArrived += HandleRobotArrived;
            _qte.OnQTEStarted += HandleQteStarted;
            _qte.OnGreatSuccess += HandleGreatSuccess;
            _qte.OnSuccess += HandleSuccess;
            _qte.OnFail += HandleFail;
        }

        private void OnDisable()
        {
            _clickToMove.OnTrashSelected -= HandleTrashSelected;
            _movement.OnRobotArrived -= HandleRobotArrived;
            _qte.OnQTEStarted -= HandleQteStarted;
            _qte.OnGreatSuccess -= HandleGreatSuccess;
            _qte.OnSuccess -= HandleSuccess;
            _qte.OnFail -= HandleFail;
        }

        private void HandleQteStarted()
        {
            _clickToMove.enabled = false;
        }

        private void HandleTrashSelected(TrashPile trash)
        {
            Debug.Log($"[TrashInteractionController] 더미 선택됨: {trash.name} ({trash.GridIndex.x},{trash.GridIndex.y})");
            _pendingTrash = trash;
        }

        private void HandleRobotArrived(Vector2Int arrivedIndex)
        {
            if (_pendingTrash == null)
            {
                Debug.Log($"[TrashInteractionController] 셀 ({arrivedIndex.x},{arrivedIndex.y}) 도착 — 선택된 더미가 없어 무시합니다.");
                return;
            }

            if (_pendingTrash.IsCollected)
            {
                Debug.Log($"[TrashInteractionController] 셀 ({arrivedIndex.x},{arrivedIndex.y}) 도착 — 선택된 더미가 이미 제거되어 무시합니다.");
                return;
            }

            _robotAnimator?.FaceTowards(_pendingTrash.transform.position);
            Debug.Log($"[TrashInteractionController] 셀 ({arrivedIndex.x},{arrivedIndex.y}) 도착 — QTE 시작.");
            OnRobotArrivedToTrash?.Invoke(_pendingTrash);
            _qte.StartQte();
        }

        private void HandleGreatSuccess()
        {
            HandleQteResult(QteResult.GreatSuccess);
        }

        private void HandleSuccess()
        {
            HandleQteResult(QteResult.Success);
        }

        private void HandleFail()
        {
            HandleQteResult(QteResult.Fail);
        }

        private void HandleQteResult(QteResult result)
        {
            _clickToMove.enabled = true;

            if (_pendingTrash == null)
            {
                Debug.LogWarning($"[TrashInteractionController] QTE 결과({result})를 받았지만 선택된 더미가 없습니다.");
                return;
            }

            TrashPile trash = _pendingTrash;
            _pendingTrash = null;

            int count = ApplyRewardMultiplier(trash.GetRewardCount(result));
            trash.Remove();
            OnCollectibleRewardGranted?.Invoke(trash, count);
        }

        private static int ApplyRewardMultiplier(int baseCount)
        {
            float adjustedCount = baseCount * UpgradeEffects.CollectionRewardMultiplier;
            int guaranteedCount = Mathf.FloorToInt(adjustedCount);
            float additionalChance = adjustedCount - guaranteedCount;
            return guaranteedCount + (UnityEngine.Random.value < additionalChance ? 1 : 0);
        }
    }
}
