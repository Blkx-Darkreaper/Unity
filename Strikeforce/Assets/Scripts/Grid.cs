using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
        public class Grid
        {
            public int Id { get; protected set; }
            public Point Corner { get; protected set; }
            public bool AllowsDriving { get; protected set; }
            public bool AllowsFlying { get; protected set; }
            public bool AllowsConstruction { get; protected set; }
            public int SectorId { get; set; }
            public Tile Tile { get; set; }

            [JsonConstructor]
            public Grid(int id, Point corner, bool allowsDriving, bool allowsFlying, bool allowsConstruction, int sector, int zone, Tile tile)
            {
                this.Id = id;
                this.Corner = corner;
                this.AllowsDriving = allowsDriving;
                this.AllowsFlying = allowsFlying;
                this.AllowsConstruction = allowsConstruction;
                this.SectorId = sector;
                this.Tile = tile;
            }
        }
}