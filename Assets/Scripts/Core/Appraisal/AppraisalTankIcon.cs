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
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
            }
        }
    }
}
