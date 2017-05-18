using UnityEngine;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Team : MonoBehaviour
    {
        public Level HomeBase { get; protected set; }
        public Color Colour;
        public int Value { get; protected set; }
        public Dictionary<string, Profile> Members;
        public bool IsRaidInProgress { get; protected set; }
        public float RaidCountdown { get; protected set; }
        public float RaidWindowRemaining { get; protected set; }

        protected void Awake()
        {
            this.Value = 0;
            this.Members = new Dictonary<string, Profile>();
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

            int rank = playerToAdd.Ranking.Level;
            Value += rank;
        }

        public void LaunchRaid()
        {
            this.IsRaidInProgress = true;
        }

        public void ResetRaidCountdown(float totalDamageValue, float elapsedGameTime, int teamPlayers, int enemyPlayers)
        {
            this.RaidCountdown = (12 - 2) * (1 - (float)Math.Exp(-totalDamageValue / 5000f)) + 2 + (teamPlayers - enemyPlayers) / 2f;
            this.RaidWindowRemaining = 0.5f + 0.1f * (float)Math.Floor((elapsedGameTime + RaidCountdown) / 5) - 0.02f * teamPlayers;
            this.IsRaidInProgress = false;
        }
    }
}