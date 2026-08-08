using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CleanPlanet.Map;
using CleanPlanet.Upgrade;

namespace CleanPlanet.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _cellsPerSecond = 4f;

        public GridSystem Grid { get; set; }
        public GridOccupancy Occupancy { get; set; }

        public Vector2Int CurrentIndex { get; private set; }
        public bool IsMoving { get; private set; }

        /// <summary>
        /// 목표 Cell에 정상적으로 도착했을 때 발행된다. 도착한 Cell 인덱스를 전달한다.
        /// 이동 중단(재탐색 실패 등)이나 재점유를 위한 중간 셀 이동에서는 발행하지 않는다.
        /// </summary>
        public event Action<Vector2Int> OnRobotArrived;

        private GridPathfinder _pathfinder;
        private Coroutine _moveRoutine;
        private bool _registered;
        private Vector2Int _targetGoal;
        private bool _hasPendingRedirect;

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
        /// targetIndex까지 경로를 계산해 이동을 시작한다. 이미 이동 중이면 현재 진행 중인
        /// 한 칸 이동이 끝나는 대로 새 목적지로 다시 경로를 계산해 이어서 이동한다(방향 전환).
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
                _targetGoal = targetIndex;
                _hasPendingRedirect = true;
                return true;
            }

            if (!_pathfinder.TryFindPath(CurrentIndex, targetIndex, out var path))
            {
                Debug.Log($"[PlayerMovement] 경로를 찾을 수 없습니다: ({targetIndex.x},{targetIndex.y})");
                return false;
            }

            _targetGoal = targetIndex;
            _hasPendingRedirect = false;
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

                    if (_hasPendingRedirect && !TryApplyRedirect(ref path, ref finalGoal, ref i))
                    {
                        _moveRoutine = null;
                        yield break;
                    }

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
            OnRobotArrived?.Invoke(finalGoal);
        }

        /// <summary>
        /// 한 칸 이동을 마치고 셀을 재점유한 직후(안전한 시점)에 호출된다. 대기 중인 새 목적지로
        /// 현재 위치에서 다시 경로를 계산해 path/finalGoal/i를 교체한다. 이미 새 목적지에
        /// 도착했으면 그대로 루프가 끝나며 도착 처리되고, 경로를 찾지 못하면 이동을 중단한다.
        /// </summary>
        private bool TryApplyRedirect(ref List<Vector2Int> path, ref Vector2Int finalGoal, ref int i)
        {
            _hasPendingRedirect = false;

            if (CurrentIndex == _targetGoal)
            {
                finalGoal = _targetGoal;
                path = new List<Vector2Int> { CurrentIndex };
                i = 1;
                return true;
            }

            if (!_pathfinder.TryFindPath(CurrentIndex, _targetGoal, out List<Vector2Int> newPath))
            {
                Debug.Log($"[PlayerMovement] 재탐색 실패로 이동을 중단합니다: ({_targetGoal.x},{_targetGoal.y})");
                IsMoving = false;
                return false;
            }

            path = newPath;
            finalGoal = _targetGoal;
            i = 1;
            return true;
        }

        private IEnumerator MoveTransformTo(Vector2Int index)
        {
            Vector2 targetWorld = Grid.GridToWorldCenter(index);

            while (Vector2.Distance(transform.position, targetWorld) > 0.0001f)
            {
                Vector2 next = Vector2.MoveTowards(
                    transform.position,
                    targetWorld,
                    _cellsPerSecond * UpgradeEffects.MovementSpeedMultiplier * Grid.CellSize * Time.deltaTime);
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
