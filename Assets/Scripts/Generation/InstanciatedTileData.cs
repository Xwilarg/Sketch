using UnityEngine;

namespace Sketch.Generation
{
    // Information about a room that was instanciated in the world
    public record InstanciatedTileData
    {
        public GameObject GameObject;
        public TileType Tile;
        public RuntimeRoom RR;
    }
}
