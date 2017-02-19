using UnityEngine;
using System;
using System.Drawing;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

namespace Strikeforce
{
    public struct RectangularPrism
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Depth { get; private set; }

        public RectangularPrism(Vector3 position, Vector3 size) : this(position.x, position.y, position.z, size.x, size.y, size.z) { }

        public RectangularPrism(float x, float y, float z, float width, float height, float depth)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
        }
    }

    public class Level : MonoBehaviour
    {
        public int Columns;
        public int Rows;
        public int TileLength = 32;
        public string LevelName;
        [HideInInspector]
        public GameObject BoundingBox;
        public Rectangle Bounds { get { return new Rectangle(0, 0, Columns, Rows); } }
        public Vector2 HeadquartersSpawn;
        public Vector2 RaiderSpawn;
        public GameObject TilePrefab;
        public Sprite[] Tileset;
        protected List<GameObject> allMapTiles;
        protected float PercentObstacles;
        protected GameObject[] GroundTiles;
        protected GameObject[] ObstacleTiles;
        protected GameObject allGridObjects;
        protected List<Vector3> gridPositions = new List<Vector3>();
        protected Rectangle[] allSectors;
        protected Vector2[] allSectorSpawns;
        public const string BOUNDING_BOX = "BoundingBox";

        public void Start()
        {
            //InitGrid();
            allMapTiles = new List<GameObject>();

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

            int width = (int)map.MapSize.Width;
            int halfWidth = width / 2;
            int height = (int)map.MapSize.Height;
            this.Columns = width / TileLength;
            this.Rows = height / TileLength;

            // Set Raider spawn and build Bounding box
            this.RaiderSpawn = new Vector2();    // TODO

            LoadBoundingBox(Columns, Rows);

            allGridObjects = new GameObject("GridsObject");
            allGridObjects.transform.parent = gameObject.transform;

            List<Grid> allMapGrids = map.AllMapGrids;
            foreach (Grid grid in allMapGrids)
            {
                int tileIndex = grid.Tile.TilesetIndex;
                Sprite sprite = Tileset[tileIndex];

                int x = grid.Corner.X;
                x -= halfWidth;
                x /= TileLength;

                int z = height - grid.Corner.Y;
                z /= TileLength;
                Vector3 position = new Vector3(x, 0, z);

                GameObject tile = Instantiate(TilePrefab, position, Quaternion.Euler(new Vector3(90, 0, 0))) as GameObject;
                tile.transform.parent = allGridObjects.transform;

                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;

                allMapTiles.Add(tile);
            }
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

        public void KeepInBounds(float x, float z, ref float deltaX, ref float deltaZ)
        {
            // Keep in level bounds
            float finalX = x + deltaX;
            float finalZ = z + deltaZ;

            float halfWidth = Bounds.Width / 2;

            deltaX = Mathf.Clamp(finalX, -halfWidth, halfWidth) - x;
            deltaZ = Mathf.Clamp(finalZ, 0, Bounds.Height) - z;
        }

        //protected Texture2D LoadMapSprite(string filename)
        //{
        //    string json = GlobalAssets.ReadTextFile(filename);
        //    StrikeforceMap map = JsonConvert.DeserializeObject<StrikeforceMap>(json);
        //    Texture2D mapImage = GetMapImage(map.MapSize, map.AllMapGrids);
        //    return mapImage;
        //}

        //public Texture2D GetMapImage(Size mapSize, List<Grid> allMapGrids)
        //{
        //    int width = mapSize.Width;
        //    int height = mapSize.Height;

        //    Columns = width / TileLength;
        //    Rows = height / TileLength;

        //    Texture2D displayImage = new Texture2D(width, height);
        //    DrawAllGridsOntoImage(ref displayImage, allMapGrids, Columns);

        //    return displayImage;
        //}

        //public void DrawAllGridsOntoImage(ref Texture2D image, List<Grid> allMapGrids, int columns)
        //{
        //    for (int i = 0; i < allMapGrids.Count; i++)
        //    {
        //        Grid grid = allMapGrids[i];

        //        int x = grid.Corner.X;
        //            int y = grid.Corner.Y;

        //        Tile tile = grid.Tile;

        //        DrawTileOntoImage(ref image, tile, x, y);
        //    }
        //}

        //public void DrawTileOntoImage(ref Bitmap image, Tile tile, int x, int y)
        //{
        //    using (Graphics g = Graphics.FromImage(image))
        //    {
        //        Image tileImage = GetTileImage(tile);
        //        g.DrawImage(tileImage, x, y, TileLength, TileLength);
        //    }
        //}

        //public Bitmap GetTileImage(Tile tile)
        //{
        //    if (tilesetImage == null)
        //    {
        //        throw new NullReferenceException("No image loaded");
        //    }

        //    Point corner = tile.Corner;
        //    int x = corner.X;
        //    int y = corner.Y;

        //    Rectangle bounds = new Rectangle(x, y, TileLength, TileLength);
        //    Bitmap tileImage = tilesetImage.Clone(bounds, PixelFormat.Format24bppRgb);
        //    return tileImage;
        //}

        protected void InitGrid()
        {
            gridPositions.Clear();

            int columnOffset = Columns / 2;
            int rowOffset = Rows / 2 + 1;
            for (int x = 0; x < Columns; x++)
            {
                for (int z = 0; z < Rows; z++)
                {
                    Vector3 position = new Vector3(x - columnOffset, 0.1f, z - rowOffset);
                    gridPositions.Add(position);

                    GameObject tilePrefab;

                    float random = Random.Range(0, 100) / 100f;
                    if (random <= PercentObstacles)
                    {
                        tilePrefab = ObstacleTiles[Random.Range(0, ObstacleTiles.Length)];
                    }
                    else
                    {
                        tilePrefab = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    }

                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity) as GameObject;
                    tile.transform.eulerAngles = new Vector3(90, 0, 0);
                    tile.transform.SetParent(gameObject.transform);
                }
            }
        }
    }
}