using UnityEngine;
using System;
using System.Drawing;

namespace Strikeforce
{
    public class Region
    {
        public Vector2 Location { get; protected set; }
        public Size Size { get; protected set; }
        protected bool isEmpty { get; set; }

        public Region()
        {
            this.Location = Vector2.zero;
            this.Size = new Size();
            this.isEmpty = true;
        }

        public Region(int x, int y, int width, int height)
        {
            this.Location = new Vector2(x, y);
            this.Size = new Size(width, height);
            this.isEmpty = false;
        }

        public virtual void AddGrid(Grid gridToAdd)
        {
            int gridX = (int)gridToAdd.Location.x;
            int gridY = (int)gridToAdd.Location.y;

            float x = Math.Min(Location.x, gridX);
            float y = Math.Min(Location.y, gridY);

            if (isEmpty == true)
            {
                x = gridX;
                y = gridY;

                isEmpty = false;
            }

            this.Location = new Vector2(x, y);

            int tileLength = Level.TileLength;

            int outerGridX = gridX + tileLength;
            int outerGridY = gridY + tileLength;

            float outerX = Location.x + Size.Width;
            float outerY = Location.y + Size.Height;

            outerX = Math.Max(outerX, outerGridX);
            outerY = Math.Max(outerY, outerGridY);

            int width = (int)(outerX - x);
            int height = (int)(outerY - y);

            this.Size = new Size(width, height);
        }
    }
}