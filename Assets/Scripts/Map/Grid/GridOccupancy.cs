using UnityEngine;

namespace CleanPlanet.Map
{
    public class GridOccupancy
    {
        private readonly GridSystem _grid;
        private readonly GameObject[,] _cells; // [col, row]

        public GridOccupancy(GridSystem grid)
        {
            _grid = grid;
            _cells = new GameObject[grid.Columns, grid.Rows];
        }

        /// <summary>
        /// 빈 셀이면 occupant를 등록하고 true 반환. 이미 점유 중이면 false.
        /// </summary>
        public bool TryOccupy(Vector2Int index, GameObject occupant)
        {
            if (!_grid.IsInBounds(index)) return false;
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
            if (_cells[from.x, from.y] != occupant) return false;
            if (_cells[to.x, to.y] != null) return false;
            _cells[from.x, from.y] = null;
            _cells[to.x, to.y] = occupant;
            return true;
        }

        public bool IsOccupied(Vector2Int index)
        {
            if (!_grid.IsInBounds(index)) return false;
            return _cells[index.x, index.y] != null;
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
