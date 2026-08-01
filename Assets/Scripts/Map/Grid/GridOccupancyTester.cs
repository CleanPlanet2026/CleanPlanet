using UnityEngine;
using CleanPlanet.Utils;

namespace CleanPlanet.Map
{
    public class GridOccupancyTester : MonoBehaviour
    {
        [SerializeField] private int _columns = 10;
        [SerializeField] private int _rows = 8;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Vector2 _origin = new(-5f, -4f);

        [SerializeField] private Vector2Int _spawnIndexA = new(2, 2);
        [SerializeField] private Vector2Int _spawnIndexB = new(5, 4);

        private GridSystem _grid;
        private GridOccupancy _occupancy;
        private TargetCellSelector _selector;
        private GameObject _objectA;
        private GameObject _objectB;

        private void Awake()
        {
            _grid = new GridSystem(_columns, _rows, _cellSize, _origin);
            _occupancy = new GridOccupancy(_grid);
            _selector = new TargetCellSelector(_grid, _occupancy);

            _objectB = CreateSquareObject("Object B", Color.white, _spawnIndexB);
            LogCell(_spawnIndexB, "B 스폰");
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

        [ContextMenu("Test: Destroy Object B")]
        private void DestroyObjectB()
        {
            if (_objectB == null)
            {
                Debug.LogWarning("[Tester] Object B가 없습니다.");
                return;
            }
            var occupant = _objectB.GetComponent<GridOccupant>();
            Vector2Int lastIndex = occupant.CurrentIndex;
            occupant.Unregister();
            Destroy(_objectB);
            _objectB = null;
            LogCell(lastIndex, "B 파괴 직후");
        }

        [ContextMenu("Test: Query All Cells")]
        private void QueryAllCells()
        {
            Debug.Log("[Tester] === 전체 점유 셀 조회 ===");
            bool anyOccupied = false;
            for (int col = 0; col < _columns; col++)
            {
                for (int row = 0; row < _rows; row++)
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
            if (_objectA == null || _objectB == null)
            {
                Debug.LogWarning("[Tester] Object A와 Object B가 모두 존재해야 합니다.");
                return;
            }

            Vector2Int dummyIndex = _objectA.GetComponent<GridOccupant>().CurrentIndex;
            Vector2Int robotIndex = _objectB.GetComponent<GridOccupant>().CurrentIndex;

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
            go.transform.localScale = new Vector3(_cellSize, _cellSize, 1f);

            var occupant = go.AddComponent<GridOccupant>();
            occupant.Occupancy = _occupancy;
            occupant.Grid = _grid;
            occupant.Register();

            return go;
        }
    }
}
