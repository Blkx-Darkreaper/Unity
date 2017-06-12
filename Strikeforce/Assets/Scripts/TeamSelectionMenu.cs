using UnityEngine;
using UnityEngine.UI;
using System;

namespace Strikeforce
{
    public class TeamSelectionMenu : Menu
    {
        public const string AUTO_JOIN = "Auto-Assign to Team";
        public const string JOIN_TEAM = "Join";
        protected Team teamA;
        protected string joinTeamAButtonName { get; set; }
        [HideInInspector]
        public string JoinTeamA;
        protected Team teamB;
        protected string joinTeamBButtonName { get; set; }
        [HideInInspector]
        public string JoinTeamB;
        public const string WITHDRAW = "Leave Match";

        protected override void Awake()
        {
            Init();

            this.joinTeamAButtonName = AllButtonNames[1];
            this.joinTeamBButtonName = AllButtonNames[2];
        }

        protected override void BuildMenu()
        {
            this.teamA = GameManager.Singleton.AllTeams[0];
            this.JoinTeamA = string.Format("{0} {1}", JOIN_TEAM, teamA.Name);

            this.teamB = GameManager.Singleton.AllTeams[1];
            this.JoinTeamB = string.Format("{0} {1}", JOIN_TEAM, teamB.Name);

            base.BuildMenu();
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { AUTO_JOIN, JoinTeamA, JoinTeamB, WITHDRAW };
        }

        protected override string[] GetMenuButtonNamesToAdd()
        {

            return new string[] { joinTeamAButtonName, joinTeamBButtonName };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            if(buttonName.Equals(AUTO_JOIN))
            {
                
            }

            if (buttonName.Equals(JoinTeamA)) {
                SelectTeam(teamA);
                return;
            }

            if (buttonName.Equals(JoinTeamB)) {
                SelectTeam(teamB);
                return;
            }

            if(buttonName.Equals(WITHDRAW))
            {

            }
        }

        protected void SelectTeam(Team selectedTeam)
        {
            Profile playerAccount = ProfileManager.Singleton.CurrentProfile;
            if(playerAccount == null)
            {
                SelectionError("No profile loaded");
                return;
            }

            bool canJoinTeam = GameManager.Singleton.CanJoinTeam(playerAccount, selectedTeam);
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
