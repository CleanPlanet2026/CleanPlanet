using UnityEngine;
using CleanPlanet.Core.Collection;

namespace CleanPlanet.Trash
{
    /// <summary>
    /// TrashInteractionController가 발행하는 수집 보상을 CollectionInbox에 적립한다.
    /// QTE/더미 시스템과 감정 시스템 사이의 유일한 연결점으로, 이 릴레이를 제외하면
    /// 둘 중 어느 쪽도 서로를 모른다.
    /// </summary>
    public sealed class TrashCollectionRelay : MonoBehaviour
    {
        [SerializeField] private TrashInteractionController _interaction;

        private void OnEnable()
        {
            if (_interaction == null)
            {
                Debug.LogError($"{nameof(TrashCollectionRelay)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _interaction.OnCollectibleRewardGranted += HandleRewardGranted;
            _interaction.OnRadiationPenaltyRequested += HandleRadiationPenalty;
        }

        private void OnDisable()
        {
            if (_interaction != null)
            {
                _interaction.OnCollectibleRewardGranted -= HandleRewardGranted;
                _interaction.OnRadiationPenaltyRequested -= HandleRadiationPenalty;
            }
        }

        private void HandleRewardGranted(TrashPile trash, int count)
        {
            CollectionInbox.Add(trash.Reward, count);
        }

        private static void HandleRadiationPenalty()
        {
            if (CollectionInbox.Count == 0)
            {
                return;
            }

            int lossCount = Mathf.Clamp(Mathf.CeilToInt(CollectionInbox.Count * 0.1f), 1, 3);
            int removed = CollectionInbox.RemoveRandom(lossCount);
            Debug.Log($"[RadiationHazard] 수집물 {removed}개를 잃었습니다.");
        }
    }
}
