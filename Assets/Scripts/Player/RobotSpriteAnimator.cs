using UnityEngine;

namespace CleanPlanet.Player
{
    [RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement))]
    public class RobotSpriteAnimator : MonoBehaviour
    {
        private enum FacingDirection
        {
            Front,
            Back,
            Right,
            Left
        }

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Sprite[] _frontIdleFrames;
        [SerializeField] private Sprite[] _backIdleFrames;
        [SerializeField] private Sprite[] _rightIdleFrames;
        [SerializeField] private Sprite[] _frontWalkFrames;
        [SerializeField] private Sprite[] _backWalkFrames;
        [SerializeField] private Sprite[] _rightWalkFrames;
        [SerializeField, Min(1f)] private float _framesPerSecond = 10f;

        private FacingDirection _facingDirection = FacingDirection.Front;
        private FacingDirection _playingDirection;
        private Vector3 _previousPosition;
        private float _frameTimer;
        private int _frameIndex;
        private bool _wasMoving;

        public void FaceTowards(Vector3 worldPosition)
        {
            Vector3 direction = worldPosition - transform.position;
            UpdateFacingDirection(direction);
            ResetAnimation(false);
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_movement == null)
            {
                _movement = GetComponent<PlayerMovement>();
            }

            _previousPosition = transform.position;
            PlayCurrentFrame();
        }

        private void Update()
        {
            Vector3 movementDelta = transform.position - _previousPosition;
            _previousPosition = transform.position;

            UpdateFacingDirection(movementDelta);

            bool isMoving = _movement.IsMoving;
            if (isMoving != _wasMoving || _facingDirection != _playingDirection)
            {
                ResetAnimation(isMoving);
            }

            AdvanceAnimation(isMoving);
        }

        private void UpdateFacingDirection(Vector3 movementDelta)
        {
            if (movementDelta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            if (Mathf.Abs(movementDelta.x) > Mathf.Abs(movementDelta.y))
            {
                _facingDirection = movementDelta.x >= 0f
                    ? FacingDirection.Right
                    : FacingDirection.Left;
                return;
            }

            _facingDirection = movementDelta.y >= 0f
                ? FacingDirection.Back
                : FacingDirection.Front;
        }

        private void ResetAnimation(bool isMoving)
        {
            _wasMoving = isMoving;
            _playingDirection = _facingDirection;
            _frameTimer = 0f;
            _frameIndex = 0;
            PlayCurrentFrame();
        }

        private void AdvanceAnimation(bool isMoving)
        {
            Sprite[] frames = GetFrames(isMoving);
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            _frameTimer += Time.deltaTime;
            float frameDuration = 1f / _framesPerSecond;
            if (_frameTimer < frameDuration)
            {
                return;
            }

            _frameTimer %= frameDuration;
            _frameIndex = (_frameIndex + 1) % frames.Length;
            PlayCurrentFrame();
        }

        private void PlayCurrentFrame()
        {
            Sprite[] frames = GetFrames(_movement != null && _movement.IsMoving);
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            _frameIndex %= frames.Length;
            _spriteRenderer.sprite = frames[_frameIndex];
            _spriteRenderer.flipX = _facingDirection == FacingDirection.Left;
        }

        private Sprite[] GetFrames(bool isMoving)
        {
            switch (_facingDirection)
            {
                case FacingDirection.Back:
                    return isMoving ? _backWalkFrames : _backIdleFrames;
                case FacingDirection.Right:
                case FacingDirection.Left:
                    return isMoving ? _rightWalkFrames : _rightIdleFrames;
                default:
                    return isMoving ? _frontWalkFrames : _frontIdleFrames;
            }
        }
    }
}
