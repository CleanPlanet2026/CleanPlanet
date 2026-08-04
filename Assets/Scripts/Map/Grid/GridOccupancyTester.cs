#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using CleanPlanet.Utils;

namespace CleanPlanet.Map
{
    public class GridOccupancyTester : MonoBehaviour
    {
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private Vector2Int _spawnIndexA = new(2, 2);

        private GridSystem _grid;
        private GridOccupancy _occupancy;
        private TargetCellSelector _selector;
        private GameObject _objectA;

        private void Awake()
        {
            if (_gridManager == null)
            {
                Debug.LogError("[Tester] GridManager가 할당되지 않았습니다.");
                return;
            }

            _grid = _gridManager.Grid;
            _occupancy = _gridManager.Occupancy;
            _selector = new TargetCellSelector(_grid, _occupancy);
        }

        [ContextMenu("Test: Spawn Object A")]
        private void SpawnObjectA()
        {
            if (_objectA != null)
            {
                Debug.LogWarning("[Tester] Object A가 이미 존재합니다.");
                return;
            }
            _objectA = CreateSquareObject("Object A", Color.black, _spawnIndexA);
            LogCell(_spawnIndexA, "A 스폰 직후");
        }

        [ContextMenu("Test: Destroy Object A")]
        private void DestroyObjectA()
        {
            if (_objectA == null)
            {
                Debug.LogWarning("[Tester] Object A가 없습니다.");
                return;
            }
            _objectA.GetComponent<GridOccupant>().Unregister();
            Destroy(_objectA);
            _objectA = null;
            LogCell(_spawnIndexA, "A 파괴 직후");
        }

        [ContextMenu("Test: Query All Cells")]
        private void QueryAllCells()
        {
            Debug.Log("[Tester] === 전체 점유 셀 조회 ===");
            bool anyOccupied = false;
            for (int col = 0; col < _grid.Columns; col++)
            {
                for (int row = 0; row < _grid.Rows; row++)
                {
                    var index = new Vector2Int(col, row);
                    var occupant = _occupancy.GetOccupant(index);
                    if (occupant == null) continue;
                    Debug.Log($"  셀 ({col},{row}): {occupant.name}");
                    anyOccupied = true;
                }
            }
            if (!anyOccupied)
                Debug.Log("  점유된 셀 없음");
        }

        [ContextMenu("Test: Select Target Cell")]
        private void SelectTargetCell()
        {
            if (_objectA == null || _gridManager.Player == null)
            {
                Debug.LogWarning("[Tester] Object A와 GridManager의 Player가 모두 존재해야 합니다.");
                return;
            }

            Vector2Int dummyIndex = _objectA.GetComponent<GridOccupant>().CurrentIndex;
            Vector2Int robotIndex = _gridManager.Player.CurrentIndex;

            bool found = _selector.TrySelectTarget(dummyIndex, robotIndex, out Vector2Int target);

            if (found)
                Debug.Log($"[Tester] Target Cell 선정 완료: 더미={dummyIndex}, 로봇={robotIndex} → Target={target}");
            else
                Debug.LogWarning($"[Tester] Target Cell 선정 실패: 더미({dummyIndex.x},{dummyIndex.y}) 주변 이동 가능한 셀 없음");
        }

        private void LogCell(Vector2Int index, string context)
        {
            var occupant = _occupancy.GetOccupant(index);
            string state = occupant != null ? $"점유 중 — {occupant.name}" : "비어있음";
            Debug.Log($"[Tester] [{context}] 셀 ({index.x},{index.y}): {state}");
        }

        private GameObject CreateSquareObject(string objName, Color color, Vector2Int gridIndex)
        {
            var go = new GameObject(objName);
            var sr = go.AddComponent<SpriteRenderer>();
            PlaceholderSprite.AssignIfMissing(sr);
            sr.color = color;

            go.transform.position = _grid.GridToWorldCenter(gridIndex);
            go.transform.localScale = new Vector3(_grid.CellSize, _grid.CellSize, 1f);

            var occupant = go.AddComponent<GridOccupant>();
            occupant.Occupancy = _occupancy;
            occupant.Grid = _grid;
            occupant.Register();

            return go;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_gridManager == null) return;
            var grid = _gridManager.Grid;
            var occupancy = _gridManager.Occupancy;

            // 셀 경계선 (내부 포함)
            Gizmos.color = new Color(0.4f, 0.6f, 1f, 0.4f);
            for (int row = 0; row <= grid.Rows; row++)
            {
                float y = grid.Origin.y + row * grid.CellSize;
                Gizmos.DrawLine(
                    new Vector3(grid.Origin.x, y, 0f),
                    new Vector3(grid.Origin.x + grid.Columns * grid.CellSize, y, 0f));
            }
            for (int col = 0; col <= grid.Columns; col++)
            {
                float x = grid.Origin.x + col * grid.CellSize;
                Gizmos.DrawLine(
                    new Vector3(x, grid.Origin.y, 0f),
                    new Vector3(x, grid.Origin.y + grid.Rows * grid.CellSize, 0f));
            }

            // 그리드 외곽선 — 내부 경계선 위에 덮어 그려 노란색으로 강조
            Gizmos.color = Color.yellow;
            var gridCenter = new Vector3(
                grid.Origin.x + grid.Columns * grid.CellSize * 0.5f,
                grid.Origin.y + grid.Rows * grid.CellSize * 0.5f,
                0f);
            Gizmos.DrawWireCube(gridCenter, new Vector3(grid.Columns * grid.CellSize, grid.Rows * grid.CellSize, 0f));

            // 점유 셀 강조
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            for (int col = 0; col < grid.Columns; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    var idx = new Vector2Int(col, row);
                    if (!occupancy.IsOccupied(idx)) continue;
                    Vector2 c = grid.GridToWorldCenter(idx);
                    Gizmos.DrawCube(
                        new Vector3(c.x, c.y, 0f),
                        new Vector3(grid.CellSize * 0.9f, grid.CellSize * 0.9f, 0.01f));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_gridManager == null) return;
            var grid = _gridManager.Grid;
            var style = new GUIStyle { fontSize = 9 };
            style.normal.textColor = Color.white;

            Gizmos.color = Color.red;
            for (int col = 0; col < grid.Columns; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    Vector2 c = grid.GridToWorldCenter(new Vector2Int(col, row));
                    var pos = new Vector3(c.x, c.y, 0f);
                    Gizmos.DrawSphere(pos, grid.CellSize * 0.05f);
                    Handles.Label(pos, $"({col},{row})", style);
                }
            }
        }
#endif
    }
}
