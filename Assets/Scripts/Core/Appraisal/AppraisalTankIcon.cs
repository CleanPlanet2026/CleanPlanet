using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 유리관 안에서 중력에 의해 쌓이는 수집물 아이콘 1개. 풀링되어 재사용되며,
    /// 자신이 어떤 CollectibleData를 표시 중인지 들고 있는다.
    /// </summary>
    public sealed class AppraisalTankIcon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rigidbody;

        public CollectibleData Data { get; private set; }

        public void Setup(CollectibleData data, Vector3 spawnPosition)
        {
            Data = data;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = data != null ? data.Icon : null;
            }

            transform.position = spawnPosition;
            transform.rotation = Quaternion.identity;

            if (_rigidbody != null)
            {
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
            }

            SetAlpha(1f);
        }

        /// <summary>
        /// 감정에 소비되어 관 아래로 미끄러져 내려가는 연출 동안 물리를 끈다.
        /// 중력·충돌로 흔들리지 않게 Kinematic으로 두고, 위치는 탱크가 트윈으로 옮긴다.
        /// 풀에서 재사용될 때 Setup이 다시 Dynamic으로 되돌린다.
        /// </summary>
        public void FreezeForConsume()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (_spriteRenderer != null)
            {
                Color color = _spriteRenderer.color;
                color.a = alpha;
                _spriteRenderer.color = color;
            }
        }
    }
}
