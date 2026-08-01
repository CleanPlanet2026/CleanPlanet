using UnityEngine;

namespace CleanPlanet.Map
{
    public class GridSystem
    {
        public int Columns { get; }
        public int Rows { get; }
        public float CellSize { get; }
        public Vector2 Origin { get; }

        public GridSystem(int columns, int rows, float cellSize, Vector2 origin)
        {
            Columns = columns;
            Rows = rows;
            CellSize = cellSize;
            Origin = origin;
        }

        /// <summary>
        /// 월드 좌표 → 그리드 인덱스. 범위 밖 좌표는 경계 인덱스로 클램프된다.
        /// </summary>
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            int col = Mathf.FloorToInt((worldPos.x - Origin.x) / CellSize);
            int row = Mathf.FloorToInt((worldPos.y - Origin.y) / CellSize);
            return new Vector2Int(
                Mathf.Clamp(col, 0, Columns - 1),
                Mathf.Clamp(row, 0, Rows - 1)
            );
        }

        /// <summary>
        /// 그리드 인덱스 → 셀 중심의 월드 좌표.
        /// </summary>
        public Vector2 GridToWorldCenter(Vector2Int index)
        {
            return new Vector2(
                Origin.x + (index.x + 0.5f) * CellSize,
                Origin.y + (index.y + 0.5f) * CellSize
            );
        }

        public bool IsInBounds(Vector2Int index)
        {
            return index.x >= 0 && index.x < Columns
                && index.y >= 0 && index.y < Rows;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 왕복 변환 단위 테스트. 에디터 메뉴 또는 코드에서 직접 호출한다.
        /// </summary>
        public static void RunTests()
        {
            // columns=10, rows=8, cellSize=1, origin=(-5,-4)
            var grid = new GridSystem(10, 8, 1.0f, new Vector2(-5f, -4f));

            Vector2[] inputs =
            {
                new(-5.0f, -4.0f),
                new(-4.5f, -3.5f),
                new( 0.3f,  0.7f),
                new( 4.9f,  3.9f),
                new(-0.1f, -0.1f),
                new( 2.0f,  1.0f),
                new(-3.5f,  2.5f),
                new( 1.999f, 1.999f),
                new(-4.999f, -3.999f),
                new( 3.0f, -2.0f),
            };

            int passed = 0;
            for (int i = 0; i < inputs.Length; i++)
            {
                Vector2Int idx = grid.WorldToGrid(inputs[i]);
                Vector2 center = grid.GridToWorldCenter(idx);
                Vector2Int roundTrip = grid.WorldToGrid(center);
                bool ok = roundTrip == idx;
                if (ok) passed++;

                Debug.Log($"[GridTest {i + 1:D2}] " +
                          $"in={inputs[i]} → idx={idx} → center={center} → roundTrip={roundTrip} | {(ok ? "PASS" : "FAIL")}");
            }

            Debug.Log($"[GridTest] 결과: {passed}/{inputs.Length} 통과");
        }
#endif
    }
}
