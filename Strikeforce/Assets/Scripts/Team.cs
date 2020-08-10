using UnityEngine;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Team : MonoBehaviour
    {
        public int teamId { get; protected set; }
        public Level homeBase { get; protected set; }
        public Color colour;
        public int totalRank { get; protected set; }
        protected Dictionary<string, Profile> members;
        public int totalMembers { get { return members.Count; } }
        [HideInInspector]
        public Inventory sharedInventory;
        public bool isRaidInProgress { get; protected set; }
        public float raidCountdown { get; protected set; }
        public float raidWindowRemaining { get; protected set; }
        protected LinkedList<Profile> raidingMembers { get; set; }
        public bool areMembersCurrentlyRaiding { get { return raidingMembers.Count > 0; } }
        public float totalDamageInflictedDuringRaid { get; protected set; }

        protected void Awake()
        {
            this.totalRank = 0;
            this.members = new Dictionary<string, Profile>();
            this.raidingMembers = new LinkedList<Profile>();
        }

        public void Init(int teamId, Level homeBase)
        {
            this.teamId = teamId;
            this.homeBase = homeBase;
        }

        protected void Update()
        {
            float timeElapsed = Time.deltaTime;

            if(isRaidInProgress == false)
            {
                this.raidCountdown = Mathf.Clamp(raidCountdown - timeElapsed, 0, raidCountdown - timeElapsed);
            } else
            {
                this.raidWindowRemaining = Mathf.Clamp(raidWindowRemaining - timeElapsed, 0, raidWindowRemaining - timeElapsed);
            }
        }

        public void AddPlayer(Profile playerToAdd)
        {
            members.Add(playerToAdd.Username, playerToAdd);
            playerToAdd.player.currentTeam = this;

            int rank = playerToAdd.Ranking.Grade;
            totalRank += rank;
        }

        public void RemovePlayer(Profile playerToRemove)
        {
            int rank = playerToRemove.Ranking.Grade;
            totalRank -= rank;

            members.Remove(playerToRemove.Username);
            playerToRemove.player.currentTeam = null;
        }

        public bool LaunchRaid(Profile playerAccount)
        {
            if(raidCountdown > 0)
            {
                return false;
            }

            this.isRaidInProgress = true;
            raidingMembers.AddLast(playerAccount);

            return true;
        }

        public void CompleteRaid(Profile playerAccount, float damageInflictedDuringRaid)
        {
            this.totalDamageInflictedDuringRaid += damageInflictedDuringRaid;
            this.raidingMembers.Remove(playerAccount);
        }

        public void ResetRaidCountdown(int teamPlayers, int enemyPlayers, float totalDamageValueFromPreviousRaid = 0f, float elapsedGameTime = 0f)
        {
            this.raidCountdown = (12 - 2) * (1 - (float)Math.Exp(-totalDamageValueFromPreviousRaid / 5000f)) + 2 + (teamPlayers - enemyPlayers) / 2f;
            this.raidWindowRemaining = 0.5f + 0.1f * (float)Math.Floor((elapsedGameTime + raidCountdown) / 5) - 0.02f * teamPlayers;
            this.totalDamageInflictedDuringRaid = 0f;
            this.isRaidInProgress = false;

            // Clear all checkpoints
            foreach(Profile account in members.Values)
            {
                account.player.raidMode.previousCheckpoint = null;
            }
        }
    }
}