using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Map;

namespace CleanPlanet.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _cellsPerSecond = 4f;

        public GridSystem Grid { get; set; }
        public GridOccupancy Occupancy { get; set; }

        public Vector2Int CurrentIndex { get; private set; }
        public bool IsMoving { get; private set; }

        private GridPathfinder _pathfinder;
        private Coroutine _moveRoutine;
        private bool _registered;

        /// <summary>
        /// 현재 world position을 기준으로 Grid Index를 등록하고 셀 중심으로 위치를 보정한다.
        /// Grid/Occupancy를 할당한 뒤 명시적으로 호출해야 한다.
        /// </summary>
        public void Register()
        {
            if (_registered) return;

            _pathfinder = new GridPathfinder(Grid, Occupancy);
            CurrentIndex = Grid.WorldToGrid(transform.position);
            SnapTo(CurrentIndex);
            _registered = Occupancy.TryOccupy(CurrentIndex, gameObject);

            if (!_registered)
            {
                Debug.LogWarning($"[PlayerMovement] 셀 ({CurrentIndex.x},{CurrentIndex.y}) 등록 실패 — 이미 다른 오브젝트가 점유 중입니다.");
            }
        }

        public void Unregister()
        {
            if (!_registered) return;
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
                IsMoving = false;
            }
            Occupancy.Release(CurrentIndex, gameObject);
            _registered = false;
        }

        /// <summary>
        /// targetIndex까지 경로를 계산해 이동을 시작한다. 이미 이동 중이면 새 명령은 무시한다.
        /// </summary>
        public bool TryMoveTo(Vector2Int targetIndex)
        {
            if (!_registered)
            {
                Debug.LogWarning("[PlayerMovement] 등록되지 않은 상태에서 이동을 시도했습니다.");
                return false;
            }

            if (IsMoving)
            {
                Debug.Log("[PlayerMovement] 이동 중에는 새로운 이동 명령을 무시합니다.");
                return false;
            }

            if (!_pathfinder.TryFindPath(CurrentIndex, targetIndex, out var path))
            {
                Debug.Log($"[PlayerMovement] 경로를 찾을 수 없습니다: ({targetIndex.x},{targetIndex.y})");
                return false;
            }

            _moveRoutine = StartCoroutine(MoveAlongPath(path, targetIndex));
            return true;
        }

        private IEnumerator MoveAlongPath(List<Vector2Int> path, Vector2Int finalGoal)
        {
            IsMoving = true;

            int i = 1;
            while (i < path.Count)
            {
                Vector2Int from = CurrentIndex;
                Vector2Int to = path[i];

                Occupancy.Release(from, gameObject);
                yield return MoveTransformTo(to);

                if (Occupancy.TryOccupy(to, gameObject))
                {
                    CurrentIndex = to;
                    i++;
                    continue;
                }

                // 전환 구간 중 다른 오브젝트가 목표 셀을 선점한 경우: 직전 셀로 안전 복귀 후 재탐색한다.
                Debug.LogWarning($"[PlayerMovement] 셀 ({to.x},{to.y})이(가) 이동 중 선점되어 재탐색합니다.");

                if (!Occupancy.TryOccupy(from, gameObject))
                {
                    Debug.LogError($"[PlayerMovement] 직전 셀 ({from.x},{from.y}) 재점유에 실패해 이동을 중단합니다.");
                    IsMoving = false;
                    _moveRoutine = null;
                    yield break;
                }

                yield return MoveTransformTo(from);
                CurrentIndex = from;

                if (!_pathfinder.TryFindPath(from, finalGoal, out path))
                {
                    Debug.Log($"[PlayerMovement] 재탐색 실패로 이동을 중단합니다: ({finalGoal.x},{finalGoal.y})");
                    IsMoving = false;
                    _moveRoutine = null;
                    yield break;
                }

                i = 1;
            }

            IsMoving = false;
            _moveRoutine = null;
        }

        private IEnumerator MoveTransformTo(Vector2Int index)
        {
            Vector2 targetWorld = Grid.GridToWorldCenter(index);

            while (Vector2.Distance(transform.position, targetWorld) > 0.0001f)
            {
                Vector2 next = Vector2.MoveTowards(
                    transform.position, targetWorld, _cellsPerSecond * Grid.CellSize * Time.deltaTime);
                transform.position = new Vector3(next.x, next.y, transform.position.z);
                yield return null;
            }

            SnapTo(index);
        }

        private void SnapTo(Vector2Int index)
        {
            Vector2 center = Grid.GridToWorldCenter(index);
            transform.position = new Vector3(center.x, center.y, transform.position.z);
        }

        private void OnDestroy()
        {
            Unregister();
        }
    }
}
