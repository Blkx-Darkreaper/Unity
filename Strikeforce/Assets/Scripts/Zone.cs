using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Strikeforce
{
    public class Zone : Region
    {
        public int ZoneId { get; protected set; }
        protected Zone next { get; set; }
        public Dictionary<int, Sector> AllSectors { get; protected set; }
        public bool IsLocked { get; protected set; }
        public int CurrentDevelopment { get; protected set; }
        protected int unlockThreshold { get; set; }

        public Zone(int zoneId) : this(zoneId, 0, 0, 0, 0) { }

        public Zone(int zoneId, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this.ZoneId = zoneId;
            this.next = null;
            this.AllSectors = new Dictionary<int, Sector>();

            SetUnlockThreshold();
        }

        protected void SetUnlockThreshold()
        {
            int size = Size.Width * Size.Height;

            this.unlockThreshold = (int)Math.Round(size * (2.5 - 0.05 * (ZoneId - 1)), 0);
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

            this.next = zone;
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

            while (next != null)
            {
                int development = next.CurrentDevelopment;
                if (development == 0)
                {
                    break;
                }

                int modifier = (int)Math.Round(development * 0.5 * (next.ZoneId - ZoneId),0);
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

            this.IsLocked = CurrentDevelopment >= unlockThreshold;
        }
    }
}