using UnityEngine;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class TeamSelectionMenu : Menu
    {
        protected Team teamA;
        public const string JOIN_TEAM = "Join";
        public string JoinTeamA;
        protected Team teamB;
        public string JoinTeamB;
        public const string QUIT = "Leave Game";

        protected override void Start()
        {
            base.Start();

            this.teamA = GameManager.Singleton.AllTeams[0];
            this.JoinTeamA = string.Format("{0} {1}", JOIN_TEAM, teamA.Name);

            this.teamB = GameManager.Singleton.AllTeams[1];
            this.JoinTeamB = string.Format("{0} {1}", JOIN_TEAM, teamB.Name);
        }

        protected override void SetButtonNames()
        {
            this.buttonNames = new string[] { JoinTeamA, JoinTeamB, QUIT };
        }

        protected override string[] GetMenuButtonNamesToAdd()
        {
            return new string[] { JoinTeamA, JoinTeamB };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            if (buttonName.Equals(JoinTeamA) == true) {
                SelectTeam(teamA, teamB);
                return;
            }

            if (buttonName.Equals(JoinTeamB) == true) {
                SelectTeam(teamB, teamA);
                return;
            }

            switch(buttonName) {
                case QUIT:
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
