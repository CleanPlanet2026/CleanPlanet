using UnityEngine;
using UnityEngine.InputSystem;
using CleanPlanet.Map;

namespace CleanPlanet.Player
{
    public class PlayerClickToMove : MonoBehaviour
    {
        [field: SerializeField] public Camera Camera { get; set; }
        [field: SerializeField] public PlayerMovement Movement { get; set; }

        public GridSystem Grid { get; set; }
        public GridOccupancy Occupancy { get; set; }

        private TargetCellSelector _targetSelector;
        private InputAction _clickAction;

        private void Awake()
        {
            if (Camera == null) Camera = Camera.main;

            _clickAction = new InputAction(binding: "<Mouse>/leftButton");
            _clickAction.performed += OnClickPerformed;
        }

        private void OnEnable()
        {
            _clickAction.Enable();
        }

        private void OnDisable()
        {
            _clickAction.Disable();
        }

        private void OnDestroy()
        {
            _clickAction.performed -= OnClickPerformed;
            _clickAction.Dispose();
        }

        /// <summary>
        /// Grid/Occupancy를 할당한 뒤 호출해 내부 TargetCellSelector를 준비한다.
        /// </summary>
        public void Initialize()
        {
            _targetSelector = new TargetCellSelector(Grid, Occupancy);
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -Camera.transform.position.z));
            Vector2Int clickedIndex = Grid.WorldToGrid(worldPos);

            if (!TryResolveDestination(clickedIndex, out Vector2Int destination))
            {
                Debug.Log("[PlayerClickToMove] 이동 가능한 목적지가 없습니다.");
                return;
            }

            Movement.TryMoveTo(destination);
        }

        /// <summary>
        /// 클릭한 셀이 비어 있으면 그대로, 점유돼 있으면 인접 4방향 중 로봇과 가장 가까운
        /// 빈 셀을 TargetCellSelector로 선정해 목적지를 보정한다.
        /// </summary>
        private bool TryResolveDestination(Vector2Int clickedIndex, out Vector2Int destination)
        {
            if (!Occupancy.IsOccupied(clickedIndex))
            {
                destination = clickedIndex;
                return true;
            }

            return _targetSelector.TrySelectTarget(clickedIndex, Movement.CurrentIndex, out destination);
        }
    }
}
