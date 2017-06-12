using UnityEngine;
using System;
using System.Collections;

namespace Strikeforce
{
    public class GameMenu : Menu
    {
        public const string RESUME = "Resume";
        public const string RAID = "Launch/Join Raid";
        public const string MANAGE_TEAM = "Manage Team";
        public const string MARKET = "Black Market";
        public const string OPTIONS = "Options";
        public const string QUIT_GAME = "Resign";

        protected override void Awake()
        {
            Init();
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { RESUME, RAID, MANAGE_TEAM, MARKET, OPTIONS, QUIT_GAME, EXIT };
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

                case QUIT_GAME:
                    QuitGame();
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

        protected virtual void QuitGame()
        {
            MenuManager.Singleton.SetLoadingScreenActive(true);
            MenuManager.Singleton.ShowMenu(PreviousMenu);

            // Drop player
            ProfileManager.Singleton.CurrentProfile.DropPlayer();
        }
    }
}