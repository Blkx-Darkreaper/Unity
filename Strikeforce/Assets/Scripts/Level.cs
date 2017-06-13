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
        [HideInInspector]
        public int Width;
        [HideInInspector]
        public int Height;
        public static int TileLength = 32;
        protected string levelName;
        [HideInInspector]
        public GameObject BoundingBox;
        public Rectangle Bounds { get { return new Rectangle(0, 0, Width, Height); } }
        public Spawnpoint HeadquartersSpawn;
        public Sprite[] Tileset;
        protected List<GameObject> allMapTiles;
        protected GameObject allGridObjects;
        protected SortedList<int, Zone> allZones;
        protected Dictionary<int, Checkpoint> allCheckpoints;
        protected Zone lastUnlockedZone;
        protected Sector nextAvailableSector { get; set; }
        public const string BOUNDING_BOX = "BoundingBox";

        public void Awake()
        {
            this.allMapTiles = new List<GameObject>();
            this.allZones = new SortedList<int, Zone>();
            this.allCheckpoints = new Dictionary<int, Checkpoint>();
        }

        public void LoadMap(string mapName)
        {
            if(mapName == null)
            {
                throw new NullReferenceException("Map name is null");
            }
            if(mapName.Equals(string.Empty))
            {
                throw new InvalidOperationException("No map name provided");
            }

            this.levelName = mapName;
            LoadMap();
        }

        protected void LoadMap()
        {
            string appPath = Application.dataPath;
            string levelPath = string.Format("{0}/Levels/{1}.json", appPath, levelName);
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

            // Testing
            //Tile testTile = new Tile(new Point(0, 0), 0);

            //List<Grid> mapGrids = new List<Grid>();
            //mapGrids.Add(new Grid(1, new Point(0, 0), true, true, true, testTile));
            //mapGrids.Add(new Grid(2, new Point(1, 0), true, true, true, testTile));

            //List<Zone> allZones = new List<Zone>();

            //List<Sector> allSectors = new List<Sector>();
            //allSectors.Add(new Sector(1, new Point(-13, 3), new Size(13,7), new Spawnpoint(false, new Point(6, 6), new Size(1, 1))));
            //allZones.Add(new Zone(1, new Point(0, 3), new Size(26, 7), allSectors));

            //List<Checkpoint> allCheckpoints = new List<Checkpoint>();
            //allCheckpoints.Add(new Checkpoint(new Point(0, 9), new Size(26, 1)));

            //map = new StrikeforceMap("Nic Bunting", new DateTime(2017, 05, 26), "\\tilea2.png", 32, 13, 7, new Size(26, 50), mapGrids, allZones, allCheckpoints);
            //string unityJson = JsonUtility.ToJson(map);
            //try
            //{
            //    levelPath = string.Format("{0}/Levels/{1}-unity.json", appPath, LevelName);
            //    GlobalAssets.WriteTextFile(levelPath, unityJson);
            //} catch(Exception ex)
            //{
            //    Debug.Log(ex.Message);
            //}
            // End testing

            try
            {
                map = JsonConvert.DeserializeObject<StrikeforceMap>(json);
                //map = JsonUtility.FromJson<StrikeforceMap>(json);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return;
            }

            Level.TileLength = map.TileLength;

            this.Width = (int)map.MapSize.Width;
            int halfWidth = Width / 2;
            this.Height = (int)map.MapSize.Height;

            // Set build Bounding box
            LoadBoundingBox(Width, Height);

            allGridObjects = new GameObject("GridsObject");
            allGridObjects.transform.parent = gameObject.transform;

            List<Grid> allMapGrids = map.AllMapGrids;
            foreach (Grid grid in allMapGrids)
            {
                int tileIndex = grid.Tile.TilesetIndex;
                Sprite sprite = Tileset[tileIndex];

                int x = (int)grid.Location.x;
                x -= halfWidth;

                int z = Height - (int)grid.Location.y;
                Vector3 position = new Vector3(x, 0, z);

                GameObject tilePrefab = GameManager.Singleton.TilePrefab;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.Euler(new Vector3(90, 0, 0))) as GameObject;
                tile.transform.parent = allGridObjects.transform;

                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;

                allMapTiles.Add(tile);
            }

            AddZones(map.AllZones);

            this.lastUnlockedZone = allZones[1];
            LinkZones();
            AddCheckpoints(map.AllCheckpoints);
        }

        protected void AddZones(List<Zone> zonesToAdd)
        {
            this.allZones = new SortedList<int, Zone>();

            foreach (Zone zone in zonesToAdd)
            {
                int zoneId = zone.ZoneId;
                if (zoneId == 0)
                {
                    return;
                }

                bool zoneExists = allZones.ContainsKey(zoneId);
                if (zoneExists == true)
                {
                    throw new InvalidOperationException(string.Format("Zone {0} has already been loaded", zoneId));
                }

                allZones.Add(zoneId, zone);

                if(nextAvailableSector == null)
                {
                    Sector firstSector = zone.FirstSector;
                    nextAvailableSector = firstSector;
                }

                bool hasHQ = zone.HasHeadquartersSpawn;
                if(hasHQ == false)
                {
                    continue;
                }

                SetHeadquartersSpawn(zone.HeadquartersSector);
            }
        }

        protected void AddCheckpoints(List<CheckpointJson> checkpointsToAdd)
        {
            this.allCheckpoints = new Dictionary<int, Checkpoint>();

            GameObject checkpointPrefab = GameManager.Singleton.CheckpointPrefab;
            
            //foreach (Vector2 location in checkpointsToAdd)
            //{
            //    int y = (int)location.y;
            //    GameObject gameObject = Instantiate(CheckpointPrefab, new Vector3(0, y, 0), Quaternion.identity) as GameObject;
            //    Checkpoint checkpoint = gameObject.GetComponent<Checkpoint>();

            //    allCheckpoints.Add(y, checkpoint);
            //}
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

        public void SetHeadquartersSpawn(Sector sector)
        {
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

        protected void LoadBoundingBox(int width, int height)
        {
            int halfHeight = height / 2;
            Vector3 position = new Vector3(0, 10, halfHeight);
            GameObject boundingBoxPrefab = GlobalAssets.GetMiscPrefab(BOUNDING_BOX);
            if (boundingBoxPrefab == null)
            {
                return;
            }

            this.BoundingBox = Instantiate(boundingBoxPrefab, position, Quaternion.identity) as GameObject;
            this.BoundingBox.transform.localScale = new Vector3(width, 20, height);
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
            GlobalAssets.KeepInBounds(Bounds, x, z, ref deltaX, ref deltaZ);
        }
    }
}