namespace Sketch.Generation
{
    public record RoomData
    {
        public int Width;
        public int Height;

        public TileType[,] Data;
    }
}