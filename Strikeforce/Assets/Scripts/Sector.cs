using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class Sector
    {
        public Vector2 Location { get; protected set; }
        public Size Size {get; protected set;}
        public Spawnpoint Spawn { get; protected set; }
        public Player Owner { get; protected set; }

        public Sector(int x, int y, int width, int height, Spawnpoint spawn)
        {
            this.Location = new Vector2(x, y);
            this.Size = new Size(width, height);
            this.Spawn = spawn;
            this.Owner = null;
        }
    }
}