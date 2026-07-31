using UnityEngine;
using UnityEngine.InputSystem;
using System;
using CleanPlanet.Utils;

namespace CleanPlanet.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField, Min(0f)] private float _moveSpeed = 5f;
        [SerializeField, Min(0.01f)] private float _arrivalDistance = 0.1f;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private Vector2 _destination;
        private bool _hasDestination;

        public event Action DestinationReached;

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0f, value);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            PlaceholderSprite.AssignIfMissing(GetComponent<SpriteRenderer>());
        }

        private void OnEnable()
        {
            if (_moveAction != null)
            {
                _moveAction.action.performed += OnMove;
                _moveAction.action.canceled += OnMove;
                _moveAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (_moveAction != null)
            {
                _moveAction.action.performed -= OnMove;
                _moveAction.action.canceled -= OnMove;
                _moveAction.action.Disable();
            }

            _moveInput = Vector2.zero;
            _hasDestination = false;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (_hasDestination)
            {
                MoveToDestination();
                return;
            }

            _rigidbody.linearVelocity = _moveInput * _moveSpeed;
        }

        public void SetDestination(Vector2 destination)
        {
            _destination = destination;
            _hasDestination = true;
        }

        public void Stop()
        {
            _hasDestination = false;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void MoveToDestination()
        {
            Vector2 position = _rigidbody.position;
            Vector2 offset = _destination - position;

            if (offset.sqrMagnitude <= _arrivalDistance * _arrivalDistance)
            {
                _rigidbody.position = _destination;
                Stop();
                DestinationReached?.Invoke();
                return;
            }

            Vector2 nextPosition = Vector2.MoveTowards(
                position,
                _destination,
                _moveSpeed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(nextPosition);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }
}
