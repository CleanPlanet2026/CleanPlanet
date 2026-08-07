using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CleanPlanet.Trash
{
    public sealed class RadiationQteController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _duration = 3f;
        [SerializeField, Min(1)] private int _requiredPressCount = 12;

        public bool IsActive { get; private set; }
        public float RemainingTime { get; private set; }
        public int PressCount { get; private set; }
        public int RequiredPressCount => _requiredPressCount;
        public float Progress => Mathf.Clamp01((float)PressCount / _requiredPressCount);

        public event Action Started;
        public event Action Succeeded;
        public event Action Failed;

        private InputAction _pressAction;

        private void Awake()
        {
            _pressAction = new InputAction(binding: "<Keyboard>/space");
            _pressAction.performed += HandlePress;
        }

        private void OnEnable()
        {
            _pressAction.Enable();
        }

        private void OnDisable()
        {
            IsActive = false;
            _pressAction.Disable();
        }

        private void OnDestroy()
        {
            _pressAction.performed -= HandlePress;
            _pressAction.Dispose();
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                Resolve(false);
            }
        }

        public void StartQte()
        {
            if (IsActive)
            {
                return;
            }

            PressCount = 0;
            RemainingTime = _duration;
            IsActive = true;
            Started?.Invoke();
        }

        private void HandlePress(InputAction.CallbackContext _)
        {
            if (!IsActive)
            {
                return;
            }

            PressCount++;
            if (PressCount >= _requiredPressCount)
            {
                Resolve(true);
            }
        }

        private void Resolve(bool succeeded)
        {
            IsActive = false;
            Debug.Log(
                $"[RadiationQTE] success={succeeded}, presses={PressCount}/{_requiredPressCount}",
                this);

            if (succeeded)
            {
                Succeeded?.Invoke();
            }
            else
            {
                Failed?.Invoke();
            }
        }
    }
}
