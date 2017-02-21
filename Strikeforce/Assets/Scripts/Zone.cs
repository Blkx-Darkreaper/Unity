using UnityEngine;
using System.Collections.Generic;
using System.Drawing;

namespace Strikeforce
{
    public class Zone : Region
    {
        public int ZoneId { get; protected set; }
        public Dictionary<int, Sector> AllSectors { get; protected set; }

        public Zone(int zoneId) : this(zoneId, 0, 0, 0, 0) { }

        public Zone(int zoneId, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this.ZoneId = zoneId;
            this.AllSectors = new Dictionary<int, Sector>();
        }

        public override void AddGrid(Grid gridToAdd)
        {
            base.AddGrid(gridToAdd);

            AddGridToSectors(gridToAdd);
        }

        public void AddGridToSectors(Grid grid)
        {
            int sectorId = grid.SectorId;

            Sector sector;
            bool sectorExists = AllSectors.ContainsKey(sectorId);
            if (sectorExists == false)
            {
                sector = new Sector(sectorId);
                AllSectors.Add(sectorId, sector);
            }

            sector = AllSectors[sectorId];

            sector.AddGrid(grid);
        }
    }
}