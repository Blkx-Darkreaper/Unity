using UnityEngine;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Team : MonoBehaviour
    {
        public string Name { get; protected set; }
        public Level HomeBase { get; protected set; }
        public Color Colour;
        public int TotalRank { get; protected set; }
        public Dictionary<string, Profile> Members;
        public Inventory SharedInventory;
        public bool IsRaidInProgress { get; protected set; }
        public float RaidCountdown { get; protected set; }
        public float RaidWindowRemaining { get; protected set; }

        protected void Awake()
        {
            this.TotalRank = 0;
            this.Members = new Dictionary<string, Profile>();
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public void SetHomeBase(Level homeBase)
        {
            this.HomeBase = homeBase;    
        }

        protected void Update()
        {
            float timeElapsed = Time.deltaTime;

            if(IsRaidInProgress == false)
            {
                this.RaidCountdown = Mathf.Clamp(RaidCountdown - timeElapsed, 0, RaidCountdown - timeElapsed);
            } else
            {
                this.RaidWindowRemaining = Mathf.Clamp(RaidWindowRemaining - timeElapsed, 0, RaidWindowRemaining - timeElapsed);
            }
        }

        public void AddPlayer(Profile playerToAdd)
        {
            Members.Add(playerToAdd.Username, playerToAdd);
            playerToAdd.Player.CurrentTeam = this;

            int rank = playerToAdd.Ranking.Grade;
            TotalRank += rank;
        }

        public void RemovePlayer(Profile playerToRemove)
        {
            int rank = playerToRemove.Ranking.Grade;
            TotalRank -= rank;

            Members.Remove(playerToRemove.Username);
            playerToRemove.Player.CurrentTeam = null;
        }

        public void LaunchRaid()
        {
            this.IsRaidInProgress = true;
        }

        public void ResetRaidCountdown(int teamPlayers, int enemyPlayers, float totalDamageValueFromPreviousRaid = 0f, float elapsedGameTime = 0f)
        {
            this.RaidCountdown = (12 - 2) * (1 - (float)Math.Exp(-totalDamageValueFromPreviousRaid / 5000f)) + 2 + (teamPlayers - enemyPlayers) / 2f;
            this.RaidWindowRemaining = 0.5f + 0.1f * (float)Math.Floor((elapsedGameTime + RaidCountdown) / 5) - 0.02f * teamPlayers;
            this.IsRaidInProgress = false;

            // Remove all checkpoints
            foreach(Profile account in Members.Values)
            {
                account.Player.PreviousCheckpoint = null;
            }
        }
    }
}