using UnityEngine;
using System;
using System.Drawing;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Level : MonoBehaviour
    {
        public int Columns;
        public int Rows;
        public static int TileLength = 32;
        public string LevelName;
        [HideInInspector]
        public GameObject BoundingBox;
        public Rectangle Bounds { get { return new Rectangle(0, 0, Columns, Rows); } }
        public Spawnpoint HeadquartersSpawn;
        public GameObject TilePrefab;
        public Sprite[] Tileset;
        protected List<GameObject> allMapTiles;
        protected GameObject allGridObjects;
        protected Dictionary<int, Zone> allZones;
        protected Zone lastUnlockedZone;
        protected Sector nextAvailableSector { get; set; }
        public const string BOUNDING_BOX = "BoundingBox";

        public void Start()
        {
            this.allMapTiles = new List<GameObject>();
            this.allZones = new Dictionary<int, Zone>();

            LoadMap();
        }

        protected void LoadMap()
        {
            string appPath = Application.dataPath;
            string levelPath = string.Format("{0}/Levels/{1}.json", appPath, LevelName);
            Debug.Log(string.Format("Loading level {0}", levelPath));

            StrikeforceMap map;
            string json = string.Empty;
            try
            {
                json = GlobalAssets.ReadTextFile(levelPath);
            }
            catch (NullReferenceException ex)
            {
                Debug.Log(string.Format("Failed to load level. {0}", ex.Message));
                return;
            }
            try
            {
                map = JsonConvert.DeserializeObject<StrikeforceMap>(json);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return;
            }

            Level.TileLength = map.TileLength;

            this.Columns = (int)map.MapSize.Width;
            int halfWidth = Columns / 2;
            this.Rows = (int)map.MapSize.Height;

            // Set build Bounding box
            LoadBoundingBox(Columns, Rows);

            allGridObjects = new GameObject("GridsObject");
            allGridObjects.transform.parent = gameObject.transform;

            List<Grid> allMapGrids = map.AllMapGrids;
            foreach (Grid grid in allMapGrids)
            {
                int tileIndex = grid.Tile.TilesetIndex;
                Sprite sprite = Tileset[tileIndex];

                int x = (int)grid.Location.x;
                x -= halfWidth;

                int z = Rows - (int)grid.Location.y;
                Vector3 position = new Vector3(x, 0, z);

                GameObject tile = Instantiate(TilePrefab, position, Quaternion.Euler(new Vector3(90, 0, 0))) as GameObject;
                tile.transform.parent = allGridObjects.transform;

                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;

                allMapTiles.Add(tile);

                AddGridToZones(grid);
            }

            this.lastUnlockedZone = allZones[1];
            LinkZones();
        }

        protected void AddGridToZones(Grid grid)
        {
            int zoneId = grid.ZoneId;
            if (zoneId == 0)
            {
                return;
            }

            Zone zone;
            bool zoneExists = allZones.ContainsKey(zoneId);
            if (zoneExists == false)
            {
                zone = new Zone(zoneId);
                allZones.Add(zoneId, zone);
            }

            zone = allZones[zoneId];

            zone.AddGrid(grid);

            AddHeadquartersSpawn(grid);
        }

        protected void LinkZones()
        {
            Zone currentZone = allZones[1];
            currentZone.UpdateDevelopment(0);

            int nextZoneId = 2;
            while (allZones.ContainsKey(nextZoneId) == true)
            {
                Zone nextZone = allZones[nextZoneId];
                nextZone.UpdateUnlockThreshold();

                // Update the last unlocked zone
                if(currentZone.IsLocked == false)
                {
                    this.lastUnlockedZone = currentZone;
                }

                currentZone.SetNextZone(nextZone);
                currentZone = nextZone;
                nextZoneId++;
            }

            if ((nextZoneId - 1) == allZones.Count)
            {
                return;
            }

            throw new InvalidOperationException(string.Format("Only {0} Zones linked out of {1}", nextZoneId - 2, allZones.Count));
        }

        protected void AddHeadquartersSpawn(Grid grid)
        {
            if (grid.IsHeadquartersSpawn == false)
            {
                return;
            }

            int zoneId = grid.ZoneId;
            int sectorId = grid.SectorId;

            Zone zone = allZones[zoneId];
            Sector sector = zone.AllSectors[sectorId];

            this.HeadquartersSpawn = sector.Spawn;
            this.nextAvailableSector = sector;
        }

        public Sector GetNextAvailableSector()
        {
            Sector availableSector = nextAvailableSector;

            int sectorId = availableSector.SectorId;
            Zone zoneToCheck = availableSector.Parent;

            int nextSectorId = sectorId + 1;

            while(!zoneToCheck.AllSectors.ContainsKey(nextSectorId))
            {
                zoneToCheck = zoneToCheck.NextZone;
            }

            this.nextAvailableSector = zoneToCheck.AllSectors[nextSectorId];

            return availableSector;
        }

        protected void LoadBoundingBox(int columns, int rows)
        {
            int midRow = rows / 2;
            Vector3 position = new Vector3(0, 10, midRow);
            GameObject boundingBoxPrefab = GlobalAssets.GetMiscPrefab(BOUNDING_BOX);
            if (boundingBoxPrefab == null)
            {
                return;
            }

            this.BoundingBox = Instantiate(boundingBoxPrefab, position, Quaternion.identity) as GameObject;
            this.BoundingBox.transform.localScale = new Vector3(columns, 20, rows);
            this.BoundingBox.transform.parent = gameObject.transform;
        }

        public Vector3 GetRaiderSpawnLocation()
        {
            Zone spawnZone = lastUnlockedZone.NextZone;
            if(spawnZone == null)
            {
                spawnZone = lastUnlockedZone;
            }

            if(spawnZone == null)
            {
                throw new InvalidOperationException("No unlocked zones. Cannot get raider spawn location");
            }

            Vector2 spawnZoneLocation = spawnZone.Location;
            Size spawnZoneSize = spawnZone.Size;
            float halfHeight = 0.5f * spawnZoneSize.Height;

            float spawnX = 0;
            float spawnY = 5;
            float spawnZ = spawnZoneLocation.y + halfHeight;

            Vector3 raiderSpawn = new Vector3(spawnX, spawnY, spawnZ);
            return raiderSpawn;
        }

        public void KeepInBounds(float x, float z, ref float deltaX, ref float deltaZ)
        {
            // Keep in level bounds
            float finalX = x + deltaX;
            float finalZ = z + deltaZ;

            float halfWidth = Bounds.Width / 2;

            deltaX = Mathf.Clamp(finalX, -halfWidth, halfWidth) - x;
            deltaZ = Mathf.Clamp(finalZ, 0, Bounds.Height) - z;
        }
    }
}