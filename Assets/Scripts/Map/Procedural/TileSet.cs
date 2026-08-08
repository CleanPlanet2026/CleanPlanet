using UnityEngine;
using UnityEngine.Tilemaps;

namespace CleanPlanet.Map.Procedural
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "CleanPlanet/Map/Tile Set")]
    public sealed class TileSet : ScriptableObject
    {
        [SerializeField] private TileBase[] _floorTiles;
        [SerializeField] private TileBase[] _wallTiles;
        [SerializeField] private TileBase[] _obstacleTiles;
        [SerializeField] private TileBase[] _decorationTiles;

        public TileBase[] FloorTiles => _floorTiles;
        public TileBase[] WallTiles => _wallTiles;
        public TileBase[] ObstacleTiles => _obstacleTiles;
        public TileBase[] DecorationTiles => _decorationTiles;

        public TileBase PickTile(MapCellType cellType, System.Random random)
        {
            TileBase[] pool = cellType switch
            {
                MapCellType.Floor => _floorTiles,
                MapCellType.Wall => _wallTiles,
                MapCellType.Obstacle => _obstacleTiles,
                MapCellType.Decoration => _decorationTiles,
                _ => null
            };

            if (pool == null || pool.Length == 0) return null;
            return pool[random.Next(pool.Length)];
        }
    }
}
