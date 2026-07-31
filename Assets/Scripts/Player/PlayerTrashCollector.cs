using System.Collections;
using CleanPlanet.Trash;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CleanPlanet.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(TrashInventory))]
    public sealed class PlayerTrashCollector : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField, Min(0f)] private float _interactionDistance = 0.25f;

        private PlayerMovement _movement;
        private TrashInventory _inventory;
        private TrashItem _target;
        private Coroutine _processingCoroutine;
        private InputAction _clickAction;

        public bool IsProcessing => _processingCoroutine != null;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _inventory = GetComponent<TrashInventory>();
        }

        private void OnEnable()
        {
            if (_camera == null)
            {
                Debug.LogError($"{nameof(PlayerTrashCollector)} requires a camera.", this);
                enabled = false;
                return;
            }

            _clickAction = new InputAction(
                "Select Trash",
                InputActionType.Button,
                "<Pointer>/press");
            _clickAction.performed += OnClick;
            _clickAction.Enable();
            _movement.DestinationReached += OnDestinationReached;
        }

        private void OnDisable()
        {
            if (_clickAction != null)
            {
                _clickAction.performed -= OnClick;
                _clickAction.Disable();
                _clickAction.Dispose();
                _clickAction = null;
            }

            if (_movement != null)
            {
                _movement.DestinationReached -= OnDestinationReached;
                _movement.Stop();
            }

            CancelTarget();
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            Pointer pointer = context.control.device as Pointer;

            if (pointer == null)
            {
                return;
            }

            Vector3 screenPosition = pointer.position.ReadValue();
            Vector2 worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out TrashItem trash))
                {
                    OnTrashClicked(trash);
                    return;
                }
            }
        }

        private void OnTrashClicked(TrashItem trash)
        {
            if (IsProcessing || trash == _target)
            {
                return;
            }

            CancelTarget();

            if (!trash.TryReserve(this))
            {
                return;
            }

            _target = trash;
            _movement.SetDestination(GetInteractionPosition(trash));
        }

        private void OnDestinationReached()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                CancelTarget();
                return;
            }

            _processingCoroutine = StartCoroutine(ProcessTrash());
        }

        private IEnumerator ProcessTrash()
        {
            yield return new WaitForSeconds(_target.ProcessingDuration);

            TrashItem processedTrash = _target;
            _target = null;
            _processingCoroutine = null;
            _inventory.AddTrash(processedTrash.RewardAmount);
            processedTrash.CompleteProcessing(this);
        }

        private Vector2 GetInteractionPosition(TrashItem trash)
        {
            Vector2 playerPosition = transform.position;
            Vector2 trashPosition = trash.transform.position;
            Vector2 direction = playerPosition - trashPosition;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return trashPosition;
            }

            return trashPosition + direction.normalized * _interactionDistance;
        }

        private void CancelTarget()
        {
            if (_processingCoroutine != null)
            {
                StopCoroutine(_processingCoroutine);
                _processingCoroutine = null;
            }

            if (_target != null)
            {
                _target.Release(this);
                _target = null;
            }
        }
    }
}
