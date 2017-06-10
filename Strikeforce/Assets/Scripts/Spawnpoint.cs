using UnityEngine;
using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    [Serializable]
    public class Spawnpoint
    {
        public Vector3 Location;
        public bool IsHeadquartersLocation;

        [JsonConstructor]
        public Spawnpoint(bool isHQSpawn, Point location, Size size)
        {
            this.IsHeadquartersLocation = isHQSpawn;
        }
    }
}