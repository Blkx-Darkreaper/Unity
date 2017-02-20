using UnityEngine;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Spawnpoint
    {
        public Vector2 Location;
        public bool IsHeadquartersLocation;

        public Spawnpoint(int x, int y, bool isHQLocation)
        {
            this.Location = new Vector2(x, y);
            this.IsHeadquartersLocation = isHQLocation;
        }
    }
}