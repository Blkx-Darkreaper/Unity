using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Strikeforce
{
    public class Level : MonoBehaviour
    {
        //[Serializable] public class Range
        //{
        //    public int Min;
        //    public int Max;

        //    public Range(int min, int max)
        //    {
        //        this.Min = min;
        //        this.Max = max;
        //    }
        //}

        public int Columns;
        public int Rows;
        public float PercentObstacles;
        public GameObject[] GroundTiles;
        public GameObject[] ObstacleTiles;
        protected List<Vector3> gridPositions = new List<Vector3>();

        public void Start()
        {
            InitGrid();
        }

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