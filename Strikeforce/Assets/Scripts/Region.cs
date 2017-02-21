using UnityEngine;
using System;
using System.Drawing;

namespace Strikeforce
{
    public class Region
    {
        public Vector2 Location { get; protected set; }
        public Size Size { get; protected set; }

        public Region(int x, int y, int width, int height)
        {
            this.Location = new Vector2(x, y);
            this.Size = new Size(width, height);
        }

        public virtual void AddGrid(Grid gridToAdd)
        {
            int gridX = (int)gridToAdd.Location.x;
            int gridY = (int)gridToAdd.Location.y;

            int tileLength = Level.TileLength;

            int outerGridX = gridX + tileLength;
            int outerGridY = gridY + tileLength;

            float x = Math.Min(Location.x, gridX);
            float y = Math.Min(Location.y, gridY);

            float outerX = Location.x + Size.Width;
            float outerY = Location.y + Size.Height;

            this.Location = new Vector2(x, y);

            outerX = Math.Max(outerX, outerGridX);
            outerY = Math.Max(outerY, outerGridY);

            int width = (int)(outerX - x);
            int height = (int)(outerY - y);

            this.Size = new Size(width, height);
        }
    }
}