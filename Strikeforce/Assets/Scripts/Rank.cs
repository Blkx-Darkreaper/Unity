using System;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Stats
    {
        protected Stats()
        {
            PropertyInfo[] allInfo = this.GetType().GetProperties();
            foreach (PropertyInfo property in allInfo)
            {
                property.SetValue(this, 0, null);
            }
        }

        public override string ToString()
        {
            string output = string.Empty;

            PropertyInfo[] allInfo = this.GetType().GetProperties();
            foreach (PropertyInfo property in allInfo)
            {
                output += string.Format("{0}: {1}\n", property.Name, property.GetValue(this, null));
            }

            return output;
        }

        protected virtual void Combine(Stats other)
        {
            PropertyInfo[] allInfo = this.GetType().GetProperties();
            for (int i = 0; i < allInfo.Length; i++)
            {
                PropertyInfo property = allInfo[i];

                float currentValue = (float)Convert.ChangeType(property.GetValue(this, null), typeof(float));
                float otherValue = (float)Convert.ChangeType(property.GetValue(other, null), typeof(float));
                float updatedValue = currentValue + otherValue;

                var type = property.PropertyType;
                property.SetValue(this, Convert.ChangeType(updatedValue, type), null);
            }
        }
    }

    public class GameStats : Stats
    {
        public int games { get; set; }
        public int victories { get; set; }
        public int defeats { get; set; }
        public int forfeits { get; set; }
        public float hoursPlayed { get; set; }

        public GameStats() : base() { }

        public void Combine(GameStats other)
        {
            base.Combine(other);
        }
    }

    public class RaidModeStats : Stats
    {
        public int damageInflicted { get; set; }
        public int unitsDestroyed { get; set; }
        public int structuresDestroyed { get; set; }
        public int moneyEarned { get; set; }
        public int fuelConsumed { get; set; }
        public int raids { get; set; }
        public int raidersLost { get; set; }
        public int completions { get; set; }
        public int bugOuts { get; set; }

        public RaidModeStats() : base() { }

        public void Combine(RaidModeStats other)
        {
            base.Combine(other);
        }
    }

    public class BuildModeStats : Stats
    {
        public int damageSustained { get; set; }
        public int unitsBuilt { get; set; }
        public int unitsLost { get; set; }
        public int structuresConstructed { get; set; }
        public int structuresRepaired { get; set; }
        public int structuresLost { get; set; }
        public int moneyProduced { get; set; }
        public int fuelProduced { get; set; }
        public int munitionsProduced { get; set; }
        public int raids { get; set; }
        public int raidsRepulsed { get; set; }
        public int raidersDestroyed { get; set; }
        public int raidersBuilt { get; set; }

        public BuildModeStats() : base() { }

        public void Combine(BuildModeStats other)
        {
            base.Combine(other);
        }
    }

    public class Rank
    {
        public int Grade { get; protected set; }
        public int Score { get; protected set; }
        public GameStats currentGameStats { get; protected set; }
        public RaidModeStats currentRaidStats { get; protected set; }
        public BuildModeStats currentBuildStats { get; protected set; }
        public GameStats latestMatchGameStats { get; protected set; }
        public RaidModeStats latestMatchRaidStats { get; protected set; }
        public BuildModeStats latestMatchBuildStats { get; protected set; }

        public Rank() : this(1, 0, new GameStats(), new RaidModeStats(), new BuildModeStats()) { }

        [JsonConstructor]
        public Rank(int ranking, int score, GameStats gameStats, RaidModeStats raidStats, BuildModeStats buildStats)
        {
            if(ranking <= 0)
            {
                ranking = 1;
            }

            this.Grade = ranking;
            this.Score = score;
            if(gameStats == null)
            {
                gameStats = new GameStats();
            }
            this.currentGameStats = gameStats;
            if(raidStats == null)
            {
                raidStats = new RaidModeStats();
            }
            this.currentRaidStats = raidStats;
            if(buildStats == null)
            {
                buildStats = new BuildModeStats();
            }
            this.currentBuildStats = buildStats;

            this.latestMatchGameStats = new GameStats();
            this.latestMatchRaidStats = new RaidModeStats();
            this.latestMatchBuildStats = new BuildModeStats();
        }

        public void AddLatestMatchStats(GameStats gameStats, RaidModeStats raidStats, BuildModeStats buildStats)
        {
            this.currentGameStats.Combine(gameStats);
            this.currentRaidStats.Combine(raidStats);
            this.currentBuildStats.Combine(buildStats);

            UpdateScore();
        }

        public void UpdateLevel()
        {
            /*int scoreNeeded = 100 + 5 * Level * (Level - 1);
            if(Score < scoreNeeded) {
                return;
            }
       	 
            this.Score %= scoreNeeded;
       	 
            this.Level++;*/

            // Level adjusts based on player's score
            this.Grade = (5 + (int)Math.Sqrt(25 - 4 * 5 * (100 - Score))) / 10;
        }

        public void UpdateScore()
        {
            // TODO
            throw new NotImplementedException();
            // Score should be between 0-2000 for player with level 20 or below
        }
    }
}