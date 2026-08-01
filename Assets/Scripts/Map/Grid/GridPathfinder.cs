using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CleanPlanet.Map
{
    public class GridPathfinder
    {
        // 탐색 우선순위 고정: 상 → 우 → 하 → 좌 (TargetCellSelector와 동일)
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        private readonly GridSystem _grid;
        private readonly GridOccupancy _occupancy;

        public GridPathfinder(GridSystem grid, GridOccupancy occupancy)
        {
            _grid = grid;
            _occupancy = occupancy;
        }

        /// <summary>
        /// start에서 goal까지 4방향 BFS로 최단 경로를 탐색한다. 점유된 셀은 장애물로 처리한다.
        /// 성공 시 path는 start와 goal을 포함한 이동 순서를 담는다. 실패 시 path는 null.
        /// </summary>
        public bool TryFindPath(Vector2Int start, Vector2Int goal, out List<Vector2Int> path)
        {
            path = null;

            if (!_grid.IsInBounds(start) || !_grid.IsInBounds(goal)) return false;

            if (start == goal)
            {
                path = new List<Vector2Int> { start };
                return true;
            }

            if (_occupancy.IsOccupied(goal)) return false;

            var visited = new bool[_grid.Columns, _grid.Rows];
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var queue = new Queue<Vector2Int>();

            visited[start.x, start.y] = true;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (var dir in Directions)
                {
                    Vector2Int next = current + dir;

                    if (!_grid.IsInBounds(next)) continue;
                    if (visited[next.x, next.y]) continue;
                    if (_occupancy.IsOccupied(next)) continue;

                    visited[next.x, next.y] = true;
                    cameFrom[next] = current;

                    if (next == goal)
                    {
                        path = ReconstructPath(cameFrom, start, goal);
                        return true;
                    }

                    queue.Enqueue(next);
                }
            }

            return false;
        }

        private static List<Vector2Int> ReconstructPath(
            Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int goal)
        {
            var path = new List<Vector2Int> { goal };
            Vector2Int current = goal;
            while (current != start)
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 단위 테스트. 에디터 ContextMenu 또는 코드에서 직접 호출한다.
        /// </summary>
        public static void RunTests()
        {
            var grid = new GridSystem(10, 8, 1f, new Vector2(-5f, -4f));
            int passed = 0;
            const int total = 5;

            // Test 1: 우회 경로 — (5,4)→(5,6) 사이 (5,5)를 막아 우회 확인
            {
                var occupancy = new GridOccupancy(grid);
                var blocker = new GameObject("__PathTestBlocker1");
                occupancy.TryOccupy(new Vector2Int(5, 5), blocker);
                var pathfinder = new GridPathfinder(grid, occupancy);
                bool ok = pathfinder.TryFindPath(new Vector2Int(5, 4), new Vector2Int(5, 6), out var path);
                bool pass = ok && path != null
                    && path[0] == new Vector2Int(5, 4)
                    && path[path.Count - 1] == new Vector2Int(5, 6)
                    && !path.Contains(new Vector2Int(5, 5));
                if (pass) passed++;
                Debug.Log($"[PathTest 1] 우회 경로: 길이={path?.Count ?? 0} | {(pass ? "PASS" : "FAIL")}");
                Object.DestroyImmediate(blocker);
            }

            // Test 2: 미로형 장애물 — 세로 벽 일부를 세워 우회 경로 존재 확인
            {
                var occupancy = new GridOccupancy(grid);
                var wall = new[]
                {
                    new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(2, 3),
                    new Vector2Int(2, 4), new Vector2Int(2, 5),
                };
                var blockers = new List<GameObject>();
                foreach (var w in wall)
                {
                    var b = new GameObject("__PathTestMaze");
                    occupancy.TryOccupy(w, b);
                    blockers.Add(b);
                }
                var pathfinder = new GridPathfinder(grid, occupancy);
                bool ok = pathfinder.TryFindPath(new Vector2Int(0, 3), new Vector2Int(4, 3), out var path);
                bool noWallCrossed = path != null;
                if (noWallCrossed)
                {
                    foreach (var w in wall)
                    {
                        if (path.Contains(w)) noWallCrossed = false;
                    }
                }
                bool pass = ok && noWallCrossed
                    && path[0] == new Vector2Int(0, 3)
                    && path[path.Count - 1] == new Vector2Int(4, 3);
                if (pass) passed++;
                Debug.Log($"[PathTest 2] 미로형 장애물: 길이={path?.Count ?? 0} | {(pass ? "PASS" : "FAIL")}");
                foreach (var b in blockers) Object.DestroyImmediate(b);
            }

            // Test 3: 완전 차단 — 목표 셀의 4면을 모두 점유해 도달 불가 확인
            {
                var occupancy = new GridOccupancy(grid);
                var goal = new Vector2Int(5, 5);
                var blockers = new List<GameObject>();
                foreach (var dir in Directions)
                {
                    var b = new GameObject("__PathTestBlockedGoal");
                    occupancy.TryOccupy(goal + dir, b);
                    blockers.Add(b);
                }
                var pathfinder = new GridPathfinder(grid, occupancy);
                bool ok = pathfinder.TryFindPath(new Vector2Int(0, 0), goal, out var path);
                bool pass = !ok && path == null;
                if (pass) passed++;
                Debug.Log($"[PathTest 3] 완전 차단: ok={ok} | {(pass ? "PASS" : "FAIL")} (예상: false)");
                foreach (var b in blockers) Object.DestroyImmediate(b);
            }

            // Test 4: 반복 실행 동일성 — 동일 조건에서 2회 실행 시 같은 경로 반환 확인
            {
                var occupancy = new GridOccupancy(grid);
                var blocker = new GameObject("__PathTestRepeat");
                occupancy.TryOccupy(new Vector2Int(5, 5), blocker);
                var pathfinder = new GridPathfinder(grid, occupancy);
                pathfinder.TryFindPath(new Vector2Int(5, 4), new Vector2Int(5, 6), out var pathA);
                pathfinder.TryFindPath(new Vector2Int(5, 4), new Vector2Int(5, 6), out var pathB);
                bool pass = pathA != null && pathB != null && pathA.Count == pathB.Count;
                if (pass)
                {
                    for (int i = 0; i < pathA.Count; i++)
                    {
                        if (pathA[i] != pathB[i]) { pass = false; break; }
                    }
                }
                if (pass) passed++;
                Debug.Log($"[PathTest 4] 반복 실행 동일성: {(pass ? "PASS" : "FAIL")}");
                Object.DestroyImmediate(blocker);
            }

            // Test 5: 성능 측정 — 50x50 빈 그리드에서 대각선 코너 간 탐색 시간 측정
            {
                var bigGrid = new GridSystem(50, 50, 1f, Vector2.zero);
                var occupancy = new GridOccupancy(bigGrid);
                var pathfinder = new GridPathfinder(bigGrid, occupancy);

                var sw = Stopwatch.StartNew();
                bool ok = pathfinder.TryFindPath(new Vector2Int(0, 0), new Vector2Int(49, 49), out var path);
                sw.Stop();

                bool pass = ok && sw.Elapsed.TotalMilliseconds < 50.0;
                if (pass) passed++;
                Debug.Log($"[PathTest 5] 성능 측정(50x50): 경로 길이={path?.Count ?? 0}, 소요시간={sw.Elapsed.TotalMilliseconds:F2}ms | {(pass ? "PASS" : "FAIL")}");
            }

            Debug.Log($"[PathTest] 결과: {passed}/{total} 통과");
        }
#endif
    }
}
