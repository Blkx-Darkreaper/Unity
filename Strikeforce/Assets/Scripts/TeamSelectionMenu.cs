using UnityEngine;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class TeamSelectionMenu : Menu
    {
        protected Team teamA;
        public const string JOIN_TEAM_A = "Join Maurauders";
        protected Team teamB;
        public const string JOIN_TEAM_B = "Join Privateers";

        protected override void Start()
        {
            base.Start();

            this.teamA = GameManager.Singleton.AllTeams[0];
            this.teamB = GameManager.Singleton.AllTeams[1];
        }

        protected override void SetButtonNames()
        {
            this.buttonNames = new string[] { };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            switch(buttonName)
            {
                case JOIN_TEAM_A:
                    SelectTeam(teamA, teamB);
                    break;

                case JOIN_TEAM_B:
                    SelectTeam(teamB, teamA);
                    break;
            }
        }

        protected void SelectTeam(Team selectedTeam, Team otherTeam)
        {
            Profile playerAccount = ProfileManager.Singleton.CurrentProfile;
            if(playerAccount == null)
            {
                SelectionError("No profile loaded");
                return;
            }

            bool canJoinTeam = GameManager.Singleton.CanJoinTeam(playerAccount, selectedTeam, otherTeam);
            if(canJoinTeam == false)
            {
                SelectionError(string.Format("Cannot join team {0}. Teams would be too unbalanced.", selectedTeam.Name));
                return;
            }

            GameManager.Singleton.JoinTeam(playerAccount, selectedTeam);
        }

        protected void SelectionError(string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
