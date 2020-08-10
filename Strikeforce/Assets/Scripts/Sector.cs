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
        public Player currentOwner { get; protected set; }
        protected int totalBuildings { get; set; }
        public float BuildingConstructionTimeBonus = 0.05f;

        [JsonConstructor]
        public Sector(int id, Point location, Size size, Spawnpoint spawn) : base(location.X, location.Y, size.Width, size.Height)
        {
            this.SectorId = id;
            this.Spawn = spawn;
        }

        public void SetParent(Zone parent)
        {
            if(parent == null)
            {
                return;
            }
            if(Parent != null)
            {
                Debug.Log(string.Format("Sector {0} already has parent Zone {1}. Zone {2} cannot be set as parent", SectorId, Parent.ZoneId, parent.ZoneId));
                return;
            }

            this.Parent = parent;
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
            if (currentOwner != null)
            {
                currentOwner.buildMode.sectors.Remove(this);
            }

            this.currentOwner = owner;
            if (owner == null)
            {
                return;
            }

            owner.buildMode.sectors.AddFirst(this);
        }

        public void RemoveOwnership()
        {
            if (currentOwner == null)
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