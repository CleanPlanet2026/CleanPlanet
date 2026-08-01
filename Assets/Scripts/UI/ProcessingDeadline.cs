using UnityEngine;

namespace CleanPlanet.UI
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ProcessingDeadline : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ProcessingCollectible collectible))
            {
                collectible.ReachDeadline();
            }
        }
    }
}
