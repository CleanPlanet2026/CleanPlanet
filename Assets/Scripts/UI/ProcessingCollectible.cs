using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class ProcessingCollectible : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Text _label;

        private RobotProcessingPreview _owner;
        private Rigidbody2D _rigidbody;
        private bool _converted;

        public RectTransform RectTransform { get; private set; }
        public int BaseGold { get; private set; }

        private void Awake()
        {
            RectTransform = (RectTransform)transform;
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Initialize(
            RobotProcessingPreview owner,
            string itemName,
            Color color,
            int baseGold,
            Vector2 velocity,
            float angularVelocity)
        {
            _owner = owner;
            BaseGold = baseGold;
            _converted = false;
            _image.color = color;
            _label.text = itemName;
            gameObject.SetActive(true);
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = angularVelocity;
            _rigidbody.WakeUp();
        }

        public void ReachDeadline()
        {
            if (_converted || _owner == null)
            {
                return;
            }

            _converted = true;
            _owner.Convert(this);
        }

        public void Deactivate()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
            }

            gameObject.SetActive(false);
        }
    }
}
