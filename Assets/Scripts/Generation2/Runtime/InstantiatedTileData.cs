using Sketch.Grid;
using UnityEngine;

namespace Sketch.Generation2.Runtime
{
    // Information about a room that was instantiated in the world
    public record InstantiatedTileData : ITileData
    {
        public SpriteRenderer SR { set; get; }
        public TileType Tile { set; get; }
        public RuntimeRoom RR { set; get; }
    }
}
