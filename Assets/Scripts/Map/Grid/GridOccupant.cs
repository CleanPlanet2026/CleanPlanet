using UnityEngine;

namespace CleanPlanet.Map
{
    public class GridOccupant : MonoBehaviour
    {
        public GridOccupancy Occupancy { get; set; }
        public GridSystem Grid { get; set; }

        public Vector2Int CurrentIndex => _currentIndex;

        private Vector2Int _currentIndex;
        private bool _registered;

        /// <summary>
        /// Tester가 스폰 직후 명시적으로 호출. Start 대신 사용해 즉시 등록을 보장한다.
        /// </summary>
        public void Register()
        {
            if (_registered) return;
            _currentIndex = Grid.WorldToGrid(transform.position);
            _registered = Occupancy.TryOccupy(_currentIndex, gameObject);
        }

        /// <summary>
        /// 점유를 즉시 해제한다. OnDestroy에서도 호출되므로 중복 호출은 안전하다.
        /// </summary>
        public void Unregister()
        {
            if (!_registered) return;
            Occupancy.Release(_currentIndex, gameObject);
            _registered = false;
        }

        private void Update()
        {
            if (!_registered) return;

            Vector2Int newIndex = Grid.WorldToGrid(transform.position);
            if (newIndex == _currentIndex) return;

            if (Occupancy.Move(gameObject, _currentIndex, newIndex))
            {
                Debug.Log($"[GridOccupant] {name}: ({_currentIndex.x},{_currentIndex.y}) → ({newIndex.x},{newIndex.y}) 이동");
                _currentIndex = newIndex;
            }
            else
            {
                Debug.LogWarning($"[GridOccupant] {name}: 셀 ({newIndex.x},{newIndex.y}) 이동 실패 — 이미 점유 중");
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }
    }
}
