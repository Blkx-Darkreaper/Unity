using UnityEngine;
using System;
using System.Collections;

namespace Strikeforce
{
    public class GameMenu : Menu
    {
        public const string RESUME = "Resume";
        public const string RAID = "Raid";
        public const string MANAGE_TEAM = "ManageTeam";
        public const string MARKET = "Market";
        public const string QUIT_MATCH = "QuitGame";
        public string raidText = "Launch/Join Raid";
        public string manageTeamText = "Manage Team";
        public string marketText = "Black Market";
        public string quitMatchText = "Resign";

        protected override void Awake()
        {
            Init();
        }

        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { RESUME, RAID, MANAGE_TEAM, MARKET, QUIT_MATCH, EXIT };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { RESUME, raidText, manageTeamText, marketText, OPTIONS, quitMatchText, exitText };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            switch(buttonName)
            {
                case RESUME:
                    MenuManager.Singleton.Resume();
                    break;

                case RAID:
                    RaidMenu();
                    break;

                case MANAGE_TEAM:
                    TeamMenu();
                    break;

                case MARKET:
                    BlackMarketMenu();
                    break;

                case OPTIONS:
                    OptionsMenu();
                    break;

                case QUIT_MATCH:
                    QuitMatch();
                    break;

                case EXIT:
                    QuitMatch();
                    ExitGame();
                    break;
            }
        }

        protected virtual void RaidMenu()
        {

        }

        protected virtual void TeamMenu()
        {

        }

        protected virtual void BlackMarketMenu()
        {

        }

        protected virtual void OptionsMenu()
        {

        }

        protected virtual void QuitMatch()
        {
            MenuManager.Singleton.SetLoadingScreenActive(true);
            MenuManager.Singleton.ShowMenu(PreviousMenu);

            // Drop player
            ProfileManager.singleton.CurrentProfile.DropPlayer();
        }
    }
}