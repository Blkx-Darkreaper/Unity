using System;
using System.Collections.Generic;
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
        public Size MapSize { get; set; }
        public List<Grid> AllMapGrids { get; set; }
        public List<Zone> AllZones { get; set; }
        public List<Checkpoint> AllCheckpoints { get; set; }

        [JsonConstructor]
        public StrikeforceMap(string author, DateTime dateCreated, string tilesetFilename, int tileLength, int nextSector, int nextZone,
            Size mapSize, List<Grid> allMapGrids, List<Zone> allZones, List<Checkpoint> allCheckpoints)
        {
            this.Author = author;
            this.DateCreated = dateCreated;
            this.TilesetFilename = tilesetFilename;
            this.TileLength = tileLength;
            this.MapSize = mapSize;
            this.AllMapGrids = allMapGrids;
            this.AllZones = allZones;
            this.AllCheckpoints = allCheckpoints;
        }
    }
}