using UnityEngine;
using System;
using System.Drawing;
using Newtonsoft.Json;

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

        [JsonConstructor]
        public Sector(int id, Point location, Size size, Spawnpoint spawn) : base(location.X, location.Y, size.Width, size.Height)
        {
            this.SectorId = id;
            this.Spawn = spawn;
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