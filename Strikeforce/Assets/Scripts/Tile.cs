using System.Drawing;

namespace Strikeforce {
    public class Tile
    {
        //public Point Corner { get; protected set; }
        public int TilesetIndex { get; set; }

        public Tile(int index, Point corner)
        {
            this.TilesetIndex = index;
            //this.Corner = corner;
        }
    }
}