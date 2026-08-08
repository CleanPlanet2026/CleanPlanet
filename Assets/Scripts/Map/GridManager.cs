using UnityEngine;
using CleanPlanet.Player;

namespace CleanPlanet.Map
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int _columns = 10;
        [SerializeField] private int _rows = 8;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Vector2 _origin = new(-5f, -4f);

        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerClickToMove _playerClickToMove;

        private GridSystem _grid;
        private GridOccupancy _occupancy;

        /// <summary>
        /// 지연 생성 프로퍼티. GridOccupancyTester 등 다른 컴포넌트가 Awake 순서와
        /// 무관하게 동일한 Grid/Occupancy 인스턴스를 참조할 수 있도록 한다.
        /// </summary>
        public GridSystem Grid => _grid ??= new GridSystem(_columns, _rows, _cellSize, _origin);
        public GridOccupancy Occupancy => _occupancy ??= new GridOccupancy(Grid);
        public PlayerMovement Player => _playerMovement;
        public PlayerClickToMove PlayerClickToMove => _playerClickToMove;

        /// <summary>
        /// 그리드 크기를 런타임에 재설정하고 Grid/Occupancy를 새로 생성한다.
        /// 절차적 맵 생성기가 맵 생성 직전에 호출한다. 기존 손맵 프리팹은 이 메서드를 호출하지 않는다.
        /// </summary>
        public void Configure(int columns, int rows, float cellSize, Vector2 origin)
        {
            _columns = columns;
            _rows = rows;
            _cellSize = cellSize;
            _origin = origin;

            _grid = new GridSystem(_columns, _rows, _cellSize, _origin);
            _occupancy = new GridOccupancy(_grid);
        }

        private void Awake()
        {
            if (_playerMovement != null && _playerClickToMove != null)
            {
                Initialize(_playerMovement, _playerClickToMove);
            }
        }

        public void Initialize(PlayerMovement playerMovement, PlayerClickToMove playerClickToMove)
        {
            _playerMovement = playerMovement;
            _playerClickToMove = playerClickToMove;

            _playerMovement.Unregister();
            _playerMovement.Grid = Grid;
            _playerMovement.Occupancy = Occupancy;
            _playerMovement.Register();

            _playerClickToMove.Grid = Grid;
            _playerClickToMove.Occupancy = Occupancy;
            _playerClickToMove.Movement = _playerMovement;
            _playerClickToMove.Initialize();
        }
    }
}
