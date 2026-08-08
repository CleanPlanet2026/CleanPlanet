using System;
using System.Collections.Generic;
using System.Diagnostics;
using CleanPlanet.Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace CleanPlanet.Map.Procedural
{
    /// <summary>
    /// 셀룰러 오토마타로 Floor/Wall 그리드를 만들어 Tilemap에 칠하고,
    /// 벽/장애물 셀을 GridOccupancy에 영구 차단으로 등록해 기존 이동/경로탐색 시스템이
    /// 그대로 벽을 인식하게 한다. 플레이어 시작 위치에서 BFS로 연결성을 검사해 기준 미달 시
    /// 파생 시드로 재생성한다. WalkableCells/SpawnableCells와 OnMapGenerated 이벤트를 통해
    /// 오브젝트 스폰 위치 정보를 외부(예: TrashSpawner)에 제공한다. 실제 오브젝트 종류/개수
    /// 결정은 이 클래스가 관여하지 않는다.
    /// </summary>
    [RequireComponent(typeof(GridManager))]
    public sealed class ProceduralMapGenerator : MonoBehaviour
    {
        [SerializeField] private MapGenerationSettings _settings = new();
        [SerializeField] private TileSet _tileSet;
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;

        [Tooltip("스테이지 선택 화면과 같은 순서로 채워두면, StageSessionState.SelectedStageIndex에 " +
            "해당하는 항목의 MapSettings/TileSet을 위 기본값 대신 사용한다.")]
        [SerializeField] private StageConfig[] _stageConfigs;

        [Header("Debug")]
        [Tooltip("컨텍스트 메뉴 'Generate With Debug Seed' 실행 시 사용할 시드.")]
        [SerializeField] private int _debugSeed;
        [SerializeField] private bool _showWalkableGizmos;
        [SerializeField] private bool _showBlockedGizmos;
        [SerializeField] private bool _showSpawnableGizmos;
        [SerializeField] private bool _showPlayerStartGizmo = true;

        private GridManager _gridManager;
        private MapCellType[,] _cells;
        private int _lastSeed;
        private Vector2Int _playerStartIndex;
        private List<Vector2Int> _walkableCells = new();
        private List<Vector2Int> _spawnableCells = new();

        public MapCellType[,] Cells => _cells;
        public Vector2Int PlayerStartIndex => _playerStartIndex;

        /// <summary>플레이어 시작 위치에서 도달 가능한 Floor 셀 전체.</summary>
        public IReadOnlyList<Vector2Int> WalkableCells => _walkableCells;

        /// <summary>WalkableCells 중 플레이어 시작 위치 주변 제외 반경 밖의 오브젝트 스폰 후보 셀.</summary>
        public IReadOnlyList<Vector2Int> SpawnableCells => _spawnableCells;

        /// <summary>맵 생성(및 연결성 검사, 플레이어 배치)이 끝날 때마다 발생한다.</summary>
        public event Action OnMapGenerated;

        /// <summary>이번 생성에 실제로 적용된 스테이지 설정. 매칭되는 항목이 없으면 null(기본값 사용).</summary>
        public StageConfig ActiveStageConfig { get; private set; }

        private void Awake()
        {
            _gridManager = GetComponent<GridManager>();
        }

        private void Start()
        {
            GenerateMap();
        }

        [ContextMenu("Generate Map")]
        public void GenerateMap()
        {
            ApplyStageConfigIfAny();
            int seed = _settings.UseFixedSeed ? _settings.Seed : Environment.TickCount;
            Generate(seed);
        }

        [ContextMenu("Regenerate Map")]
        public void RegenerateMap()
        {
            GenerateMap();
        }

        public void GenerateWithSeed(int seed)
        {
            ApplyStageConfigIfAny();
            _settings.UseFixedSeed = true;
            _settings.Seed = seed;
            Generate(seed);
        }

        [ContextMenu("Generate With Debug Seed")]
        private void GenerateWithDebugSeed()
        {
            GenerateWithSeed(_debugSeed);
        }

        /// <summary>
        /// StageSessionState.SelectedStageIndex에 해당하는 StageConfig가 있으면
        /// 기본 _settings/_tileSet 대신 그 값을 사용하도록 교체한다.
        /// </summary>
        private void ApplyStageConfigIfAny()
        {
            ActiveStageConfig = ResolveStageConfig();
            if (ActiveStageConfig == null) return;

            _settings = ActiveStageConfig.MapSettings;
            _tileSet = ActiveStageConfig.TileSet;
        }

        private StageConfig ResolveStageConfig()
        {
            int index = StageSessionState.SelectedStageIndex;
            if (_stageConfigs == null || index < 0 || index >= _stageConfigs.Length) return null;

            return _stageConfigs[index];
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (_floorTilemap != null) _floorTilemap.ClearAllTiles();
            if (_wallTilemap != null) _wallTilemap.ClearAllTiles();

            if (_cells != null && _gridManager != null)
            {
                RegisterBlockedCells(new MapCellType[_cells.GetLength(0), _cells.GetLength(1)],
                    _cells.GetLength(0), _cells.GetLength(1));
            }

            _cells = null;
        }

        private void Generate(int seed)
        {
            if (_gridManager == null) _gridManager = GetComponent<GridManager>();

            if (_tileSet == null || _floorTilemap == null || _wallTilemap == null)
            {
                Debug.LogWarning("[ProceduralMapGenerator] TileSet/Tilemap이 연결되지 않아 생성을 건너뜁니다.");
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            int width = _settings.Width;
            int height = _settings.Height;
            float tileSize = _settings.TileSize;
            var origin = new Vector2(-width / 2f * tileSize, -height / 2f * tileSize);

            _gridManager.Configure(width, height, tileSize, origin);
            AlignTilemapGrid(origin, tileSize);

            int maxAttempts = _settings.MaxGenerationAttempts;
            bool connected = false;
            List<Vector2Int> connectedCells = null;
            int floorCount = 0;
            int attempt = 0;

            for (attempt = 0; attempt < maxAttempts; attempt++)
            {
                int attemptSeed = unchecked(seed + attempt * 97_003);
                var random = new System.Random(attemptSeed);

                _cells = GenerateNoise(width, height, random, _settings.ObstacleDensity);
                for (int i = 0; i < _settings.SmoothingIterations; i++)
                {
                    _cells = Smooth(_cells, width, height);
                }

                _playerStartIndex = SelectPlayerStart(_cells, width, height, random);
                (connectedCells, floorCount) = EvaluateConnectivity(_cells, width, height, _playerStartIndex);
                connected = floorCount > 0 && (float)connectedCells.Count / floorCount >= _settings.MinConnectedFloorRatio;

                _lastSeed = attemptSeed;

                if (connected) break;
            }

            if (!connected)
            {
                Debug.LogWarning(
                    $"[ProceduralMapGenerator] {maxAttempts}번 시도했지만 연결성 기준({_settings.MinConnectedFloorRatio:P0})을 만족하지 못해 마지막 결과를 사용합니다.");
            }

            _walkableCells = connectedCells;
            _spawnableCells = BuildSpawnableCells(_walkableCells, _playerStartIndex);

            RegisterBlockedCells(_cells, width, height);
            Paint(_cells, width, height, new System.Random(_lastSeed));
            ReregisterPlayer(_playerStartIndex);

            stopwatch.Stop();
            int attemptsUsed = Mathf.Min(attempt + 1, maxAttempts);
            LogResult(width, height, _cells, connectedCells.Count, floorCount, connected, attemptsUsed, stopwatch.ElapsedMilliseconds);

            OnMapGenerated?.Invoke();
        }

        private List<Vector2Int> BuildSpawnableCells(List<Vector2Int> walkableCells, Vector2Int playerStart)
        {
            int radius = _settings.ObjectSpawnExclusionRadius;
            var spawnable = new List<Vector2Int>(walkableCells.Count);

            foreach (var cell in walkableCells)
            {
                int distance = Mathf.Max(Mathf.Abs(cell.x - playerStart.x), Mathf.Abs(cell.y - playerStart.y));
                if (distance <= radius) continue;

                spawnable.Add(cell);
            }

            return spawnable;
        }

        /// <summary>
        /// 시작 셀에서 4방향 BFS로 도달 가능한 Floor 셀 목록과 전체 Floor 수를 반환한다.
        /// </summary>
        private static (List<Vector2Int> connectedCells, int totalFloorCount) EvaluateConnectivity(
            MapCellType[,] cells, int width, int height, Vector2Int start)
        {
            int totalFloor = 0;
            foreach (var cell in cells)
            {
                if (cell == MapCellType.Floor) totalFloor++;
            }

            var connectedCells = new List<Vector2Int>();
            if (cells[start.x, start.y] != MapCellType.Floor) return (connectedCells, totalFloor);

            var visited = new bool[width, height];
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited[start.x, start.y] = true;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                connectedCells.Add(current);

                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height) continue;
                    if (visited[next.x, next.y]) continue;
                    if (cells[next.x, next.y] != MapCellType.Floor) continue;

                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }

            return (connectedCells, totalFloor);
        }

        private void RegisterBlockedCells(MapCellType[,] cells, int width, int height)
        {
            var occupancy = _gridManager.Occupancy;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool blocked = cells[x, y] == MapCellType.Wall || cells[x, y] == MapCellType.Obstacle;
                    occupancy.SetBlocked(new Vector2Int(x, y), blocked);
                }
            }
        }

        /// <summary>
        /// GridManager.Awake()가 지형 생성보다 먼저 실행돼 플레이어가 옛/빈 Occupancy에
        /// 등록됐을 수 있으므로, 지형과 벽 등록이 끝난 뒤 선정된 시작 셀로 옮기고 다시 등록한다.
        /// </summary>
        private void ReregisterPlayer(Vector2Int startIndex)
        {
            var player = _gridManager.Player;
            var clickToMove = _gridManager.PlayerClickToMove;
            if (player == null || clickToMove == null) return;

            player.transform.position = _gridManager.Grid.GridToWorldCenter(startIndex);
            _gridManager.Initialize(player, clickToMove);
        }

        /// <summary>
        /// 가장자리에서 margin 이상 떨어져 있고, 주변 반경의 바닥 비율이 기준 이상인 Floor 셀 중
        /// 하나를 시드 기반으로 무작위 선정한다. 조건을 만족하는 셀이 없으면 margin/비율 조건을
        /// 완화해 재시도하고, 그래도 없으면 맵 중앙에서 가장 가까운 Floor 셀로 대체한다.
        /// </summary>
        private Vector2Int SelectPlayerStart(MapCellType[,] cells, int width, int height, System.Random random)
        {
            int margin = _settings.PlayerStartEdgeMargin;
            float minOpenRatio = _settings.PlayerStartMinOpenRatio;

            for (int attempt = 0; attempt < 4; attempt++)
            {
                var candidates = CollectStartCandidates(cells, width, height, margin, minOpenRatio);
                if (candidates.Count > 0)
                {
                    return candidates[random.Next(candidates.Count)];
                }

                // 조건을 만족하는 셀이 없으면 완화해서 재시도한다.
                margin = Mathf.Max(0, margin - 1);
                minOpenRatio = Mathf.Max(0f, minOpenRatio - 0.2f);
            }

            Debug.LogWarning("[ProceduralMapGenerator] 조건을 만족하는 시작 위치를 찾지 못해 가장 가까운 Floor 셀로 대체합니다.");
            return FindNearestFloor(cells, width, height, new Vector2Int(width / 2, height / 2));
        }

        private List<Vector2Int> CollectStartCandidates(
            MapCellType[,] cells, int width, int height, int margin, float minOpenRatio)
        {
            var candidates = new List<Vector2Int>();

            for (int x = margin; x < width - margin; x++)
            {
                for (int y = margin; y < height - margin; y++)
                {
                    if (cells[x, y] != MapCellType.Floor) continue;
                    if (OpenFloorRatio(cells, x, y, width, height) < minOpenRatio) continue;

                    candidates.Add(new Vector2Int(x, y));
                }
            }

            return candidates;
        }

        private float OpenFloorRatio(MapCellType[,] cells, int x, int y, int width, int height)
        {
            int radius = _settings.PlayerStartOpenRadius;
            int total = 0;
            int floor = 0;

            for (int nx = x - radius; nx <= x + radius; nx++)
            {
                for (int ny = y - radius; ny <= y + radius; ny++)
                {
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;

                    total++;
                    if (cells[nx, ny] == MapCellType.Floor) floor++;
                }
            }

            return total == 0 ? 0f : (float)floor / total;
        }

        private static Vector2Int FindNearestFloor(MapCellType[,] cells, int width, int height, Vector2Int from)
        {
            for (int radius = 0; radius < Mathf.Max(width, height); radius++)
            {
                for (int x = from.x - radius; x <= from.x + radius; x++)
                {
                    for (int y = from.y - radius; y <= from.y + radius; y++)
                    {
                        if (x < 0 || y < 0 || x >= width || y >= height) continue;
                        if (cells[x, y] == MapCellType.Floor) return new Vector2Int(x, y);
                    }
                }
            }

            return from;
        }

        private void AlignTilemapGrid(Vector2 origin, float tileSize)
        {
            Grid grid = _floorTilemap.GetComponentInParent<Grid>();
            if (grid == null) return;

            grid.transform.position = new Vector3(origin.x, origin.y, 0f);
            grid.cellSize = new Vector3(tileSize, tileSize, 1f);
        }

        private static MapCellType[,] GenerateNoise(int width, int height, System.Random random, float obstacleDensity)
        {
            var cells = new MapCellType[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = IsBorder(x, y, width, height)
                        ? MapCellType.Wall
                        : RollCell(random, obstacleDensity);
                }
            }

            return cells;
        }

        private static MapCellType RollCell(System.Random random, float obstacleDensity)
        {
            return random.NextDouble() < obstacleDensity ? MapCellType.Wall : MapCellType.Floor;
        }

        private MapCellType[,] Smooth(MapCellType[,] source, int width, int height)
        {
            var result = new MapCellType[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsBorder(x, y, width, height))
                    {
                        result[x, y] = MapCellType.Wall;
                        continue;
                    }

                    int wallNeighbors = CountWallNeighbors(source, x, y, width, height);
                    result[x, y] = wallNeighbors >= 5 ? MapCellType.Wall : MapCellType.Floor;
                }
            }

            return result;
        }

        private static int CountWallNeighbors(MapCellType[,] cells, int x, int y, int width, int height)
        {
            int count = 0;

            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                for (int ny = y - 1; ny <= y + 1; ny++)
                {
                    if (nx == x && ny == y) continue;

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        count++; // 맵 밖은 벽으로 취급해 가장자리를 자연스럽게 막는다.
                        continue;
                    }

                    if (cells[nx, ny] == MapCellType.Wall) count++;
                }
            }

            return count;
        }

        private static bool IsBorder(int x, int y, int width, int height)
        {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
        }

        private void Paint(MapCellType[,] cells, int width, int height, System.Random random)
        {
            _floorTilemap.ClearAllTiles();
            _wallTilemap.ClearAllTiles();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var position = new Vector3Int(x, y, 0);

                    _floorTilemap.SetTile(position, _tileSet.PickTile(MapCellType.Floor, random));

                    if (cells[x, y] == MapCellType.Wall)
                    {
                        _wallTilemap.SetTile(position, _tileSet.PickTile(MapCellType.Wall, random));
                    }
                }
            }
        }

        private void LogResult(
            int width, int height, MapCellType[,] cells,
            int connectedCount, int floorCount, bool connected, int attemptsUsed, long elapsedMs)
        {
            int wallCount = 0;

            foreach (var cell in cells)
            {
                if (cell == MapCellType.Wall) wallCount++;
            }

            string stage = ActiveStageConfig != null ? ActiveStageConfig.StageId.ToString() : "-";

            Debug.Log(
                $"[ProceduralMapGenerator] Stage: {stage}\n" +
                $"Seed: {_lastSeed}\n" +
                $"Map Size: {width} x {height}\n" +
                $"Wall Cells: {wallCount}\n" +
                $"Floor Cells: {floorCount}\n" +
                $"Walkable Cells: {connectedCount}\n" +
                $"Spawnable Cells: {_spawnableCells.Count}\n" +
                $"Player Start: {_playerStartIndex}\n" +
                $"Connectivity: {(connected ? "PASS" : "FAIL")}\n" +
                $"Generation Attempts: {attemptsUsed}\n" +
                $"Generation Time: {elapsedMs} ms");
        }

        private void OnDrawGizmosSelected()
        {
            if (_cells == null) return;

            GridManager gridManager = _gridManager != null ? _gridManager : GetComponent<GridManager>();
            if (gridManager == null) return;

            GridSystem grid = gridManager.Grid;
            Vector3 cellSize = new Vector3(grid.CellSize, grid.CellSize, 0.1f) * 0.9f;

            if (_showBlockedGizmos)
            {
                DrawCellTypeGizmos(grid, new Color(0.85f, 0.2f, 0.2f, 0.35f), cellSize, MapCellType.Wall);
                DrawCellTypeGizmos(grid, new Color(0.85f, 0.2f, 0.2f, 0.35f), cellSize, MapCellType.Obstacle);
            }

            if (_showWalkableGizmos)
            {
                DrawCellListGizmos(_walkableCells, grid, new Color(0.2f, 0.7f, 0.9f, 0.2f), cellSize);
            }

            if (_showSpawnableGizmos)
            {
                DrawCellListGizmos(_spawnableCells, grid, new Color(0.95f, 0.8f, 0.2f, 0.35f), cellSize);
            }

            if (_showPlayerStartGizmo)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(grid.GridToWorldCenter(_playerStartIndex), grid.CellSize * 0.35f);
            }
        }

        private void DrawCellTypeGizmos(GridSystem grid, Color color, Vector3 cellSize, MapCellType type)
        {
            Gizmos.color = color;
            for (int x = 0; x < grid.Columns; x++)
            {
                for (int y = 0; y < grid.Rows; y++)
                {
                    if (_cells[x, y] != type) continue;
                    Gizmos.DrawCube(grid.GridToWorldCenter(new Vector2Int(x, y)), cellSize);
                }
            }
        }

        private static void DrawCellListGizmos(List<Vector2Int> cells, GridSystem grid, Color color, Vector3 cellSize)
        {
            Gizmos.color = color;
            foreach (Vector2Int cell in cells)
            {
                Gizmos.DrawCube(grid.GridToWorldCenter(cell), cellSize);
            }
        }
    }
}
