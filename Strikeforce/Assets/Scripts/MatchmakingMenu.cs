using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class MatchmakingMenu : Menu
    {
        public const string TEST = "Test Game"; // ToDo remove

        protected override void SetButtonNames()
        {
            buttons = new string[] { TEST };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            switch (buttonName)
            {
                case TEST:
                    JoinGame();
                    break;
            }
        }

        protected virtual void JoinGame()
        {
            Resume();

            int levelIndex = 1;
            Application.LoadLevel(levelIndex);
        }
    }
}