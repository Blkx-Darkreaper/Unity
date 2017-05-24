using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Strikeforce
{
    public class Zone : Region
    {
        public int ZoneId { get; protected set; }
        public Zone NextZone { get; protected set; }
        public Dictionary<int, Sector> AllSectors { get; protected set; }
        public bool IsLocked { get; protected set; }
        public int CurrentDevelopment { get; protected set; }
        protected int unlockThreshold { get; set; }

        public Zone(int zoneId) : base() {
            this.ZoneId = zoneId;
            this.NextZone = null;
            this.AllSectors = new Dictionary<int, Sector>();
            this.IsLocked = true;
            this.unlockThreshold = 0;
        }

        public Zone(int zoneId, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this.ZoneId = zoneId;
            this.NextZone = null;
            this.AllSectors = new Dictionary<int, Sector>();
            this.IsLocked = true;
            this.unlockThreshold = 0;
        }

        public void UpdateUnlockThreshold()
        {
            int tilesWide = Size.Width;
            int tilesHigh = Size.Height;
            int area = tilesWide * tilesHigh;

            this.unlockThreshold = (int)Math.Round(area * (2.5 - 0.05 * (ZoneId - 1)), 0);
        }

        public void SetNextZone(Zone zone)
        {
            if (zone == null)
            {
                return;
            }

            if (zone.ZoneId != ZoneId + 1)
            {
                return;
            }

            this.NextZone = zone;
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
                sector = new Sector(sectorId, this);
                AllSectors.Add(sectorId, sector);
            }

            sector = AllSectors[sectorId];

            sector.AddGrid(grid);
        }

        public int GetMaxDevelopment()
        {
            int size = Size.Width * Size.Height;

            int baseMax = (int)Math.Round((5 - 0.05 * (ZoneId - 1)) * size,0);

            int maxDevelopment = baseMax;

            while (NextZone != null)
            {
                int development = NextZone.CurrentDevelopment;
                if (development == 0)
                {
                    break;
                }

                int modifier = (int)Math.Round(development * 0.5 * (NextZone.ZoneId - ZoneId),0);
                maxDevelopment += modifier;
            }

            return maxDevelopment;
        }

        public bool CanConstructStructure(int cost)
        {
            int maxDevelopment = GetMaxDevelopment();

            int futureDevelopment = CurrentDevelopment + cost;

            return futureDevelopment > maxDevelopment;
        }

        public void UpdateDevelopment(int amount)
        {
            this.CurrentDevelopment += amount;

            this.IsLocked = CurrentDevelopment < unlockThreshold;
        }
    }
}