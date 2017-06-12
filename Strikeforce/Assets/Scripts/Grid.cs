using UnityEngine;
using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Grid
    {
        public int Id { get; protected set; }
        public Vector2 Location { get; protected set; }
        public bool AllowsDriving { get; protected set; }
        public bool AllowsFlying { get; protected set; }
        public bool AllowsConstruction { get; protected set; }
        public Tile Tile { get; set; }

        [JsonConstructor]
        public Grid(int id, Point location, bool allowsDriving, bool allowsFlying, bool allowsConstruction, Tile tile)
        {
            this.Id = id;
            this.Location = new Vector2(location.X, location.Y);
            this.AllowsDriving = allowsDriving;
            this.AllowsFlying = allowsFlying;
            this.AllowsConstruction = allowsConstruction;
            this.Tile = tile;
        }
    }
}