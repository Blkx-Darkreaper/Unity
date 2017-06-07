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
    }
}