using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace Strikeforce
{
    public class ResultsMenu : Menu
    {
        protected const string NEW_GAME = "NewGame";
        protected const string MAIN_MENU = "MainMenu";
        protected string newGameText = "New Game";
        protected string mainMenuText = "Main Menu";
        protected string gameOverText = "Game Over";
        protected Profile winner;
        protected VictoryCondition metWinCondition { get; set; }

        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { NEW_GAME, MAIN_MENU };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { newGameText, mainMenuText };
        }

        protected override void DrawMenu()
        {
            GUI.skin = MenuSkin;

            float x = 0;
            float y = 0;
            float width = Screen.width;
            float height = Screen.height;
            GUI.BeginGroup(new Rect(x, y, width, height));

            // Display
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            string message = gameOverText;
            if (winner != null)
            {
                message = string.Format("Congratulations {0}! You have won by {1}.", winner.Username, metWinCondition.GetDescription());
            }

            float padding = Attributes.Padding;
            float buttonHeight = Attributes.ButtonHeight;
            float buttonWidth = Attributes.ButtonWidth;
            x = padding;
            y = padding;
            width = Screen.width - 2 * padding;
            height = buttonHeight;
            GUI.Label(new Rect(x, y, width, height), message);

            x = Screen.width / 2 - padding / 2 - buttonWidth;
            y += buttonHeight + padding;

            bool buttonPressed = GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), newGameText);
            if (buttonPressed == true)
            {
                Time.timeScale = 1f;
                IsOpening = false;
                SceneManager.LoadScene(Scenes.Match);
            }

            x += padding + buttonWidth;

            buttonPressed = GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), mainMenuText);
            if (buttonPressed == true)
            {
                GameManager.Singleton.CmdLoadLevel(string.Empty);
                SceneManager.LoadScene(Scenes.MatchLobby);
                Cursor.visible = true;
            }

            GUI.EndGroup();
        }

        public void Victory(VictoryCondition endCondition)
        {
            if (endCondition == null)
            {
                return;
            }

            metWinCondition = endCondition;
            winner = metWinCondition.GetWinningPlayer();
        }
    }
}