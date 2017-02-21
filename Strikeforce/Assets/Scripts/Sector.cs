using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class Sector : Region
    {
        public int SectorId { get; protected set; }
        public Spawnpoint Spawn { get; protected set; }
        public Player Owner { get; protected set; }

        public Sector(int sectorId) : this(sectorId, 0, 0, 0, 0, null) { }

        public Sector(int sectorId, int x, int y, int width, int height, Spawnpoint spawn) : base(x, y, width, height)
        {
            this.SectorId = sectorId;
            this.Spawn = spawn;
            this.Owner = null;
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
    }
}