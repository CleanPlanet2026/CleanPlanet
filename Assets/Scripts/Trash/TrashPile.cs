using System;
using UnityEngine;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Map;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// 그리드 위 한 Cell을 점유하는 쓰레기 더미. QTE 결과별 보상 개수를 들고 있으며,
    /// 결과와 무관하게 Remove() 호출 시 점유를 해제하고 제거되어 재도전할 수 없다.
    /// </summary>
    public class TrashPile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _visualVariants;
        [SerializeField] private CollectibleData _reward;
        [SerializeField, Min(0)] private int _failMinCount = 0;
        [SerializeField, Min(0)] private int _failMaxCount = 1;
        [SerializeField, Min(0)] private int _successCount = 2;
        [SerializeField, Min(0)] private int _greatSuccessCount = 4;

        public GridSystem Grid { get; set; }
        public GridOccupancy Occupancy { get; set; }
        public Vector2Int GridIndex { get; private set; }
        public bool IsCollected { get; private set; }
        public CollectibleData Reward => _reward;

        public event Action<TrashPile> OnTrashCollected;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void SetReward(CollectibleData reward)
        {
            _reward = reward;
        }

        /// <summary>
        /// Grid/Occupancy와 Reward를 먼저 설정한 뒤 호출한다. 지정한 Cell 중심으로 위치를 옮기고 점유한다.
        /// </summary>
        public void Spawn(Vector2Int index)
        {
            ApplyRandomVisual();
            GridIndex = index;
            transform.position = Grid.GridToWorldCenter(index);

            if (!Occupancy.TryOccupy(index, gameObject))
            {
                Debug.LogWarning($"[TrashPile] 셀 ({index.x},{index.y}) 점유에 실패했습니다.");
            }
        }

        private void ApplyRandomVisual()
        {
            if (_spriteRenderer == null || _visualVariants == null || _visualVariants.Length == 0)
            {
                return;
            }

            int variantIndex = UnityEngine.Random.Range(0, _visualVariants.Length);
            _spriteRenderer.sprite = _visualVariants[variantIndex];
        }

        public int GetRewardCount(QteResult result)
        {
            return result switch
            {
                QteResult.Fail => UnityEngine.Random.Range(_failMinCount, _failMaxCount + 1),
                QteResult.Success => _successCount,
                QteResult.GreatSuccess => _greatSuccessCount,
                _ => 0
            };
        }

        /// <summary>
        /// QTE 결과와 무관하게 호출된다. Cell 점유를 해제하고 더미를 제거해 재도전을 막는다.
        /// </summary>
        public void Remove()
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            Occupancy.Release(GridIndex, gameObject);
            OnTrashCollected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
