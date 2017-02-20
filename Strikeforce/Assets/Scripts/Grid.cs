using UnityEngine;
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
            public bool IsHeadquartersSpawn { get; protected set; }
            public bool IsSectorSpawn { get; protected set; }
            public int SectorId { get; protected set; }
            public int ZoneId { get; protected set; }
            public Tile Tile { get; set; }

            [JsonConstructor]
            public Grid(int id, Point location, bool allowsDriving, bool allowsFlying, bool allowsConstruction, 
                bool isHQSpawn, bool isSectorSpawn, int sectorId, int zoneId, Tile tile)
            {
                this.Id = id;
                this.Location = new Vector2(location.X, location.Y);
                this.AllowsDriving = allowsDriving;
                this.AllowsFlying = allowsFlying;
                this.AllowsConstruction = allowsConstruction;
                this.IsHeadquartersSpawn = isHQSpawn;
                this.IsSectorSpawn = isSectorSpawn;
                this.SectorId = sectorId;
                this.ZoneId = zoneId;
                this.Tile = tile;
            }
        }
}