using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class MainMenu : Menu
    {
        protected const string PROFILE = "Profile";
        public Menu ProfileMenu;
        protected const string MATCHMAKING = "Matchmaking";
        public Menu MatchmakingMenu;
        protected const string OPTIONS = "Options";
        public Menu OptionsMenu;

        protected void OnLevelWasLoaded()
        {
            //Cursor.visible = true;
            //Profile currentAccount = GameManager.ActiveInstance.CurrentPlayerAccount;
            //if (currentAccount == null)
            //{
            //    GetComponent<MainMenu>().enabled = false;
            //    GetComponent<ProfileMenu>().enabled = true;
            //}
            //else
            //{
            //    GetComponent<MainMenu>().enabled = true;
            //    GetComponent<ProfileMenu>().enabled = false;
            //}
        }

        protected override void SetButtonNames()
        {
            buttons = new string[] { PROFILE, MATCHMAKING, OPTIONS, EXIT };
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
            if (menuManager == null)
            {
                Debug.LogError(string.Format("Menu manager hasn't been loaded."));
                return;
            }

            menuManager.ShowMenu(ProfileMenu);
        }

        protected void Matchmaking()
        {
            if (menuManager == null)
            {
                Debug.LogError(string.Format("Menu manager hasn't been loaded."));
                return;
            }

            menuManager.ShowMenu(MatchmakingMenu);
        }

        protected void Options()
        {
            if (menuManager == null)
            {
                Debug.LogError(string.Format("Menu manager hasn't been loaded."));
                return;
            }

            menuManager.ShowMenu(OptionsMenu);
        }

        public override void HideMenu()
        {
            GetComponent<MainMenu>().enabled = false;
        }
    }
}