using System.Drawing;

namespace Strikeforce {
    public class Tile
    {
        //public Point Location { get; protected set; }
        public int TilesetIndex { get; set; }

        public Tile(Point corner, int index)
        {
            this.TilesetIndex = index;
            //this.Location = location;
        }
    }
}