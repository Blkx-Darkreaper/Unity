using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Drawing;

namespace Strikeforce
{
    public class StrikeforceMap
    {
        // Meta data
        public string Author { get; set; }
        public DateTime DateCreated { get; set; }

        // Map data
        public string TilesetFilename { get; set; }
        public int TileLength { get; set; }
        public int NextSector { get; set; }
        public Size MapSize { get; set; }
        public List<Grid> AllMapGrids { get; set; }
        public List<Checkpoint> AllCheckpoints { get; set; }
        public List<Spawnpoint> AllSpawnpoints { get; set; }

        [JsonConstructor]
        public StrikeforceMap(string author, DateTime dateCreated, string tilesetFilename, int tileLength, int nextSector, Size mapSize, List<Grid> allMapGrids, List<Checkpoint> allCheckpoints, List<Spawnpoint> allSpawnpoints)
        {
            this.Author = author;
            this.DateCreated = dateCreated;
            this.TilesetFilename = tilesetFilename;
            this.TileLength = tileLength;
            this.NextSector = nextSector;
            this.MapSize = mapSize;
            this.AllMapGrids = allMapGrids;
            this.AllCheckpoints = allCheckpoints;
            this.AllSpawnpoints = allSpawnpoints;
        }
    }
}