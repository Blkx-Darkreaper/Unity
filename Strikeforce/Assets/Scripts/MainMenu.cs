using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Strikeforce
{
    public class MainMenu : Menu
    {
        protected const string PROFILE = "Profile";
        protected const string MATCHMAKING = "Matchmaking";
        public Menu ProfileMenu;
        public Menu MatchmakingMenu;
        public Menu OptionsMenu;

        protected override void Awake()
        {
            base.Awake();

            DisableMatchmaking();
        }

        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { PROFILE, MATCHMAKING, OPTIONS, EXIT };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { PROFILE, MATCHMAKING, OPTIONS, exitText };
        }

        protected void EnableMatchmaking()
        {
            if (ProfileManager.Singleton == null)
            {
                return;
            }

            bool profileSelected = ProfileManager.Singleton.CurrentProfile != null;
            if (profileSelected == false)
            {
                return;
            }

            Button matchmakingButton = allButtons[MATCHMAKING];
            matchmakingButton.interactable = true;
        }

        protected void DisableMatchmaking()
        {
            Button matchmakingButton = allButtons[MATCHMAKING];
            if (matchmakingButton == null)
            {
                return;
            }

            matchmakingButton.interactable = false;
        }

        public override void ShowMenu()
        {
            base.ShowMenu();

            EnableMatchmaking();
        }

        protected override void HandleKeyboardActivity()
        {
            return;
        }

        protected override void HandleButtonPress(string buttonName)
        {
            switch (buttonName)
            {
                case PROFILE:
                    Profile();
                    break;

                case MATCHMAKING:
                    Matchmaking();
                    break;

                case OPTIONS:
                    Options();
                    break;

                case EXIT:
                    ExitGame();
                    break;
            }
        }

        private void Profile()
        {
            SwitchToMenu(ProfileMenu);
        }

        protected void Matchmaking()
        {
            SwitchToMenu(MatchmakingMenu);
        }

        protected void Options()
        {
            SwitchToMenu(OptionsMenu);
        }

        public override void DisableMenu()
        {
            GetComponent<MainMenu>().enabled = false;
        }
    }
}