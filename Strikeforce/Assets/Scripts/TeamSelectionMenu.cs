﻿using UnityEngine;
using UnityEngine.UI;
using System;

namespace Strikeforce
{
    public class TeamSelectionMenu : Menu
    {
        protected const string AUTO_JOIN = "AutoJoin";
        protected const string JOIN_TEAM_A = "JoinTeamA";
        protected const string JOIN_TEAM_B = "JoinTeamB";
        protected const string QUIT_MATCH = "QuitMatch";
        protected string autoJoinText = "Auto-Assign to Team";
        protected string withDrawText = "Leave Match";
        public const string JOIN_TEAM = "Join";
        protected Team teamA;
        [HideInInspector]
        public string JoinTeamAText;
        protected Team teamB;
        [HideInInspector]
        public string JoinTeamBText;

        protected override void Awake()
        {
            Init();
        }

        protected override void Init()
        {
            base.Init();
        }

        protected override void BuildMenu()
        {
            this.teamA = GameplayManager.singleton.allTeams[0];
            this.JoinTeamAText = string.Format("{0} {1}", JOIN_TEAM, teamA.name);

            this.teamB = GameplayManager.singleton.allTeams[1];
            this.JoinTeamBText = string.Format("{0} {1}", JOIN_TEAM, teamB.name);

            base.BuildMenu();
        }

        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { AUTO_JOIN, JOIN_TEAM_A, JOIN_TEAM_B, QUIT_MATCH };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { autoJoinText, JoinTeamAText, JoinTeamBText, withDrawText };
        }

        protected override string[] GetMenuButtonNamesToAdd()
        {
            return new string[] { JOIN_TEAM_A, JOIN_TEAM_B };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            switch(buttonName)
            {
                case AUTO_JOIN:
                    break;

                case JOIN_TEAM_A:
                    SelectTeam(teamA);
                    break;

                case JOIN_TEAM_B:
                    SelectTeam(teamB);
                    break;

                case QUIT_MATCH:
                    break;
            }
        }

        protected void SelectTeam(Team selectedTeam)
        {
            Profile playerAccount = ProfileManager.singleton.CurrentProfile;
            if(playerAccount == null)
            {
                SelectionError("No profile loaded");
                return;
            }

            bool canJoinTeam = GameplayManager.singleton.CanJoinTeam(playerAccount, selectedTeam);
            if(canJoinTeam == false)
            {
                SelectionError(string.Format("Cannot join team {0}. Teams would be too unbalanced.", selectedTeam.name));
                return;
            }

            //GameplayManager.singleton.CmdJoinTeam(playerAccount.Username, selectedTeam.name);
            GameplayManager.singleton.CmdJoinTeam(playerAccount.Username, selectedTeam.teamId);

            GameplayManager.singleton.CmdStartGame();  //Testing
        }

        protected void SelectionError(string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
