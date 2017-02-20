using UnityEngine;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Checkpoint
    {
        public Vector2 Location;
        public Size Size;

        [JsonConstructor]
        public Checkpoint(Point location, Size size)
        {
            this.Location = new Vector2(location.X, location.Y);
            this.Size = size;
        }
    }
}