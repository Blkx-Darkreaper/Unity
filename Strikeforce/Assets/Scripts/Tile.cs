using System.Drawing;

namespace Strikeforce {
    public class Tile
    {
        public int TilesetIndex { get; set; }

        public Tile(Point corner, int index)
        {
            this.TilesetIndex = index;
        }
    }
}