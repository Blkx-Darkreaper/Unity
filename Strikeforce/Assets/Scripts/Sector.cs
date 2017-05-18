using UnityEngine;
using System;
using System.Drawing;

namespace Strikeforce
{
    public class Sector : Region
    {
        public int SectorId { get; protected set; }
        public Zone Parent { get; protected set; }
        public Spawnpoint Spawn { get; protected set; }
        public Player Owner { get; protected set; }
        protected int totalBuildings { get; set; }
        public float BuildingConstructionTimeBonus = 0.05f;

        public Sector(int sectorId, Zone parent) : this(sectorId, parent, 0, 0, 0, 0, null) { }

        public Sector(int sectorId, Zone parent, int x, int y, int width, int height, Spawnpoint spawn)
            : base(x, y, width, height)
        {
            this.SectorId = sectorId;
            this.Parent = parent;
            this.Spawn = spawn;
            this.Owner = null;
            this.totalBuildings = 0;
        }

        public override void AddGrid(Grid gridToAdd)
        {
            base.AddGrid(gridToAdd);

            bool isSpawnpoint = gridToAdd.IsSectorSpawn || gridToAdd.IsHeadquartersSpawn;
            if (isSpawnpoint == false)
            {
                return;
            }

            int x = (int)gridToAdd.Location.x;
            int y = (int)gridToAdd.Location.y;

            this.Spawn = new Spawnpoint(x, y, gridToAdd.IsHeadquartersSpawn);
        }

        public bool CanConstructStructure(int cost)
        {
            return Parent.CanConstructStructure(cost);
        }

        public void UpdateDevelopment(int amount)
        {
            Parent.UpdateDevelopment(amount);
        }

        public void SetOwnership(Player owner)
        {
            if (Owner != null)
            {
                Owner.Sectors.Remove(this);
            }

            this.Owner = owner;
            if (owner == null)
            {
                return;
            }

            owner.Sectors.AddFirst(this);
        }

        public void RemoveOwnership()
        {
            if (Owner == null)
            {
                return;
            }

            SetOwnership(null);
        }

        public void UpdateBuildings(int amount)
        {
            this.totalBuildings += amount;

            if (totalBuildings >= 0)
            {
                return;
            }

            throw new InvalidOperationException(string.Format("Sector {0} cannot have less than 0 buildings", SectorId));
        }

        public float GetReducedConstructionTime(float structTime)
        {
            float reducedStructTime = structTime * (float)Math.Pow(1 + BuildingConstructionTimeBonus, totalBuildings);
            return reducedStructTime;
        }
    }
}