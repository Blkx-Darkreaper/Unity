using UnityEngine;
using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Spawnpoint
    {
        public Vector3 Location;
        public bool IsHeadquartersLocation;

        [JsonConstructor]
        public Spawnpoint(bool isHQSpawn, Point location, Size size)
        {
            this.Location = new Vector3(location.X, 0, location.Y);
            this.IsHeadquartersLocation = isHQSpawn;
        }
    }
}