using UnityEngine;

namespace CleanPlanet.Map
{
    public class TargetCellSelector
    {
        // 탐색 우선순위 고정: 상 → 우 → 하 → 좌
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        private readonly GridSystem _grid;
        private readonly GridOccupancy _occupancy;

        public TargetCellSelector(GridSystem grid, GridOccupancy occupancy)
        {
            _grid = grid;
            _occupancy = occupancy;
        }

        /// <summary>
        /// dummyIndex 인접 4셀 중 robotIndex와 가장 가까운 빈 셀을 반환한다.
        /// 동일 거리 후보가 여러 개면 탐색 우선순위(상→우→하→좌)에서 먼저 등록된 셀이 선택된다.
        /// 이동 가능한 셀이 없으면 false를 반환하고 targetIndex는 (-1,-1)로 설정된다.
        /// </summary>
        public bool TrySelectTarget(
            Vector2Int dummyIndex,
            Vector2Int robotIndex,
            out Vector2Int targetIndex)
        {
            targetIndex = new Vector2Int(-1, -1);
            int bestDist = int.MaxValue;
            bool found = false;

            foreach (var dir in Directions)
            {
                Vector2Int candidate = dummyIndex + dir;

                if (!_grid.IsInBounds(candidate)) continue;
                if (_occupancy.IsOccupied(candidate)) continue;

                int dist = ManhattanDistance(candidate, robotIndex);

                // strict less than: 동일 거리는 먼저 등록된 후보 유지 → 우선순위 보장
                if (dist < bestDist)
                {
                    bestDist = dist;
                    targetIndex = candidate;
                    found = true;
                }
            }

            if (!found)
                Debug.Log($"[TargetCellSelector] 더미 ({dummyIndex.x},{dummyIndex.y}) 주변에 이동 가능한 셀이 없습니다.");

            return found;
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

            // Test 1: 맵 경계 — 범위 밖 셀(Down, Left) 제외, 동일 거리 시 상 우선
            {
                var occupancy = new GridOccupancy(grid);
                var sel = new TargetCellSelector(grid, occupancy);
                bool ok = sel.TrySelectTarget(new Vector2Int(0, 0), new Vector2Int(5, 4), out var t);
                bool pass = ok && t == new Vector2Int(0, 1);
                if (pass) passed++;
                Debug.Log($"[TargetTest 1] 맵 경계: target={t} | {(pass ? "PASS" : "FAIL")} (예상: (0,1))");
            }

            // Test 2: 점유 셀 제외 — Up 방향 셀이 점유됨, 나머지 중 Left가 최단
            {
                var occupancy = new GridOccupancy(grid);
                var blocker = new GameObject("__TestBlocker2");
                occupancy.TryOccupy(new Vector2Int(5, 5), blocker);
                var sel = new TargetCellSelector(grid, occupancy);
                bool ok = sel.TrySelectTarget(new Vector2Int(5, 4), new Vector2Int(3, 4), out var t);
                bool pass = ok && t == new Vector2Int(4, 4);
                if (pass) passed++;
                Debug.Log($"[TargetTest 2] 점유 셀 제외: target={t} | {(pass ? "PASS" : "FAIL")} (예상: (4,4))");
                Object.DestroyImmediate(blocker);
            }

            // Test 3: Manhattan Distance 최솟값 — Down이 dist=1로 단독 최단
            {
                var occupancy = new GridOccupancy(grid);
                var sel = new TargetCellSelector(grid, occupancy);
                bool ok = sel.TrySelectTarget(new Vector2Int(5, 4), new Vector2Int(5, 2), out var t);
                bool pass = ok && t == new Vector2Int(5, 3);
                if (pass) passed++;
                Debug.Log($"[TargetTest 3] 최단 거리: target={t} | {(pass ? "PASS" : "FAIL")} (예상: (5,3))");
            }

            // Test 4: 동일 거리 우선순위 — Up(5,5)과 Right(6,4) 모두 dist=1, 상 우선
            {
                var occupancy = new GridOccupancy(grid);
                var sel = new TargetCellSelector(grid, occupancy);
                bool ok = sel.TrySelectTarget(new Vector2Int(5, 4), new Vector2Int(6, 5), out var t);
                bool pass = ok && t == new Vector2Int(5, 5);
                if (pass) passed++;
                Debug.Log($"[TargetTest 4] 동일 거리 우선순위: target={t} | {(pass ? "PASS" : "FAIL")} (예상: (5,5))");
            }

            // Test 5: 전방향 막힘 — 범위 밖 2개 + 점유 2개 → 후보 없음
            {
                var occupancy = new GridOccupancy(grid);
                var b1 = new GameObject("__TestBlocker5a");
                var b2 = new GameObject("__TestBlocker5b");
                occupancy.TryOccupy(new Vector2Int(0, 1), b1);
                occupancy.TryOccupy(new Vector2Int(1, 0), b2);
                var sel = new TargetCellSelector(grid, occupancy);
                bool ok = sel.TrySelectTarget(new Vector2Int(0, 0), new Vector2Int(5, 4), out var t);
                bool pass = !ok && t == new Vector2Int(-1, -1);
                if (pass) passed++;
                Debug.Log($"[TargetTest 5] 전방향 막힘: ok={ok}, target={t} | {(pass ? "PASS" : "FAIL")} (예상: false, (-1,-1))");
                Object.DestroyImmediate(b1);
                Object.DestroyImmediate(b2);
            }

            Debug.Log($"[TargetTest] 결과: {passed}/{total} 통과");
        }
#endif
    }
}
