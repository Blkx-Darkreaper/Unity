using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Strikeforce
{
    public class MainMenu : Menu
    {
        protected const string PROFILE = "Profile";
        public Menu ProfileMenu;
        protected const string MATCHMAKING = "Matchmaking";
        protected string matchmakingButtonName { get; set; }
        public Menu MatchmakingMenu;
        protected const string OPTIONS = "Options";
        public Menu OptionsMenu;

        protected override void Awake()
        {
            base.Awake();

            DisableMatchmaking();
        }

        protected override void Init()
        {
            this.matchmakingButtonName = AllButtonNames[1];

            base.Init();
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

            Button matchmakingButton = allButtons[matchmakingButtonName];
            matchmakingButton.interactable = true;
        }

        protected void DisableMatchmaking()
        {
            Button matchmakingButton = allButtons[matchmakingButtonName];
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

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { PROFILE, MATCHMAKING, OPTIONS, EXIT };
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