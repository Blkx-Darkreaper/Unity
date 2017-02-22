using UnityEngine;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Spawnpoint
    {
        public Vector3 Location;
        public bool IsHeadquartersLocation;

        public Spawnpoint(int x, int y) : this(x, y, false) { }

        public Spawnpoint(int x, int y, bool isHQLocation) : this(x, 0, y, isHQLocation) { }

        public Spawnpoint(int x, int y, int z) : this(x, y, z, false) { }

        public Spawnpoint(int x, int y, int z, bool isHQLocation)
        {
            this.Location = new Vector3(x, y, z);
            this.IsHeadquartersLocation = isHQLocation;
        }
    }
}