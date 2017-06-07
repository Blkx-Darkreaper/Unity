using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Zone : Region
    {
        public int ZoneId { get; protected set; }
        public Zone NextZone { get; protected set; }
        public Dictionary<int, Sector> AllSectors { get; protected set; }
        public bool HasHeadquartersSpawn { get { return HeadquartersSector != null; } }
        public Sector HeadquartersSector { get; protected set; }
        public bool IsLocked { get; protected set; }
        public int CurrentDevelopment { get; protected set; }
        protected int unlockThreshold { get; set; }

        [JsonConstructor]
        public Zone(int id, Vector2 location, Size size, List<Sector> allSectors) : base(location.X, location.Y, size.Width, size.Height)
        {
            this.ZoneId = id;

            AddSectors(allSectors);
        }

        protected void AddSectors(List<Sector> sectorsToAdd)
        {
            this.AllSectors = new Dictionary<int, Sector>();

            foreach (Sector sector in sectorsToAdd)
            {
                int sectorId = sector.SectorId;

                bool sectorExists = AllSectors.ContainsKey(sectorId);
                if (sectorExists == true)
                {
                    throw new InvalidOperationException(string.Format("Sector {0} has already been loaded", sectorId));
                }

                AllSectors.Add(sectorId, sector);

                if(sector.Spawn.IsHeadquartersLocation == false)
                {
                    continue;
                }

                this.HeadquartersSector = sector;
            }
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