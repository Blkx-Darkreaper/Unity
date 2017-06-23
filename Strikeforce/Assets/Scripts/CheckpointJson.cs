using UnityEngine;
using Newtonsoft.Json;
using System.Drawing;

namespace Strikeforce
{
    public class CheckpointJson
    {
        public Vector2 Location;
        public Size Size;

        [JsonConstructor]
        public CheckpointJson(Point location, Size size)
        {
            this.Location = new Vector2(location.X, location.Y);
            this.Size = size;
        }
    }
}