using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Checkpoint
    {
        public Point Location;
        public Size Size;

        [JsonConstructor]
        public Checkpoint(Point location, Size size)
        {
            this.Location = location;
            this.Size = size;
        }
    }
}