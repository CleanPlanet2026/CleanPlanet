using CleanPlanet.Player;
using CleanPlanet.Utils;
using UnityEngine;

namespace CleanPlanet.Trash
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class TrashItem : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _processingDuration = 3f;
        [SerializeField, Min(1)] private int _rewardAmount = 1;

        private TrashSpawner _owner;
        private PlayerTrashCollector _reservedBy;

        public float ProcessingDuration => _processingDuration;
        public int RewardAmount => _rewardAmount;

        private void Awake()
        {
            PlaceholderSprite.AssignIfMissing(GetComponent<SpriteRenderer>());
        }

        public void Initialize(TrashSpawner owner)
        {
            _owner = owner;
        }

        public bool TryReserve(PlayerTrashCollector collector)
        {
            if (_reservedBy != null)
            {
                return false;
            }

            _reservedBy = collector;
            return true;
        }

        public void Release(PlayerTrashCollector collector)
        {
            if (_reservedBy == collector)
            {
                _reservedBy = null;
            }
        }

        public void CompleteProcessing(PlayerTrashCollector collector)
        {
            if (_reservedBy != collector)
            {
                return;
            }

            _reservedBy = null;
            _owner.Recycle(this);
        }

        private void OnDisable()
        {
            _reservedBy = null;
        }
    }
}
