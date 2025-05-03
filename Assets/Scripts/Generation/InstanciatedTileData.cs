using UnityEngine;

namespace Sketch.Generation
{
    // Information about a room that was instanciated in the world
    public record InstanciatedTileData
    {
        public SpriteRenderer SR;
        public TileType Tile;
        public RuntimeRoom RR;
    }
}
