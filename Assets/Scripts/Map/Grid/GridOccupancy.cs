using UnityEngine;

namespace CleanPlanet.Map
{
    public class GridOccupancy
    {
        private readonly GridSystem _grid;
        private readonly GameObject[,] _cells; // [col, row]
        private readonly bool[,] _blocked; // [col, row], 영구 통행 불가(벽/장애물)

        public GridOccupancy(GridSystem grid)
        {
            _grid = grid;
            _cells = new GameObject[grid.Columns, grid.Rows];
            _blocked = new bool[grid.Columns, grid.Rows];
        }

        /// <summary>
        /// 셀을 영구적으로 통행 불가로 표시/해제한다. 절차적 맵 생성기가 벽/장애물 셀에 대해 호출한다.
        /// </summary>
        public void SetBlocked(Vector2Int index, bool blocked)
        {
            if (!_grid.IsInBounds(index)) return;
            _blocked[index.x, index.y] = blocked;
        }

        public bool IsBlocked(Vector2Int index)
        {
            if (!_grid.IsInBounds(index)) return false;
            return _blocked[index.x, index.y];
        }

        /// <summary>
        /// 빈 셀이면 occupant를 등록하고 true 반환. 이미 점유 중이거나 벽이면 false.
        /// </summary>
        public bool TryOccupy(Vector2Int index, GameObject occupant)
        {
            if (!_grid.IsInBounds(index)) return false;
            if (_blocked[index.x, index.y]) return false;
            if (_cells[index.x, index.y] != null) return false;
            _cells[index.x, index.y] = occupant;
            return true;
        }

        /// <summary>
        /// 해당 셀의 소유자가 occupant일 때만 해제. 다른 오브젝트의 잘못된 해제 방지.
        /// </summary>
        public bool Release(Vector2Int index, GameObject occupant)
        {
            if (!_grid.IsInBounds(index)) return false;
            if (_cells[index.x, index.y] != occupant) return false;
            _cells[index.x, index.y] = null;
            return true;
        }

        /// <summary>
        /// from 해제 + to 점유를 원자적으로 처리. to가 점유 중이면 이동 실패.
        /// </summary>
        public bool Move(GameObject occupant, Vector2Int from, Vector2Int to)
        {
            if (!_grid.IsInBounds(to)) return false;
            if (_blocked[to.x, to.y]) return false;
            if (_cells[from.x, from.y] != occupant) return false;
            if (_cells[to.x, to.y] != null) return false;
            _cells[from.x, from.y] = null;
            _cells[to.x, to.y] = occupant;
            return true;
        }

        /// <summary>
        /// 벽(영구 차단) 또는 다른 오브젝트가 점유 중이면 true.
        /// </summary>
        public bool IsOccupied(Vector2Int index)
        {
            if (!_grid.IsInBounds(index)) return false;
            return _blocked[index.x, index.y] || _cells[index.x, index.y] != null;
        }

        /// <summary>
        /// 점유 주체를 반환. 비어있거나 범위 밖이면 null.
        /// </summary>
        public GameObject GetOccupant(Vector2Int index)
        {
            if (!_grid.IsInBounds(index)) return null;
            return _cells[index.x, index.y];
        }
    }
}
