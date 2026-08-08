using CleanPlanet.Map;
using CleanPlanet.Map.Procedural;
using Unity.Cinemachine;
using UnityEngine;

namespace CleanPlanet.Utils
{
    /// <summary>
    /// 절차적 맵이 (재)생성될 때마다 카메라 컨파이너의 경계 박스를 맵 크기에 맞춰 갱신해,
    /// Cinemachine 카메라가 비추는 영역이 맵 밖으로 나가지 않게 한다.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MapCameraBounds : MonoBehaviour
    {
        [SerializeField] private ProceduralMapGenerator _mapGenerator;
        [SerializeField] private CinemachineConfiner2D _confiner;

        private BoxCollider2D _bounds;

        private void Awake()
        {
            _bounds = GetComponent<BoxCollider2D>();
            _bounds.isTrigger = true;
        }

        private void OnEnable()
        {
            if (_mapGenerator == null)
            {
                Debug.LogError($"{nameof(MapCameraBounds)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _mapGenerator.OnMapGenerated += ApplyBounds;
        }

        private void OnDisable()
        {
            if (_mapGenerator != null)
            {
                _mapGenerator.OnMapGenerated -= ApplyBounds;
            }
        }

        private void ApplyBounds()
        {
            GridManager gridManager = _mapGenerator.GetComponent<GridManager>();
            if (gridManager == null) return;

            GridSystem grid = gridManager.Grid;
            float width = grid.Columns * grid.CellSize;
            float height = grid.Rows * grid.CellSize;

            // BoxCollider2D의 offset은 이 오브젝트의 로컬 좌표 기준이다. 이 오브젝트가
            // 월드 원점에 무회전·1배율로 있다는 전제로 월드 좌표를 그대로 offset에 사용한다.
            _bounds.offset = grid.Origin + new Vector2(width, height) * 0.5f;
            _bounds.size = new Vector2(width, height);

            if (_confiner != null)
            {
                _confiner.InvalidateBoundingShapeCache();
            }
        }
    }
}
