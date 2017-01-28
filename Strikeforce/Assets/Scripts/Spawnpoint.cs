using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Spawnpoint
    {
        public Point Location;
        public int ParentSectorId;
        public bool IsHeadquartersLocation;

        [JsonConstructor]
        public Spawnpoint(Point location, Size size, int parentSectorId, bool isHeadquartersLocation)
        {
            this.ParentSectorId = parentSectorId;
        }
    }
}