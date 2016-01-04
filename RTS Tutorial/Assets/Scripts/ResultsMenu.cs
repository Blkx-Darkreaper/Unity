using UnityEngine;
using System.Collections.Generic;
using RTS;

public class ResultsMenu : MenuController {

    protected PlayerController winner;
    protected VictoryCondition metWinCondition { get; set; }
    protected struct Message {
    public const string NEW_GAME = "New Game";
    public const string MAIN_MENU = "Main Menu";
    public const string GAME_OVER = "Game Over";
    }

    protected override void DrawMenu()
    {
        GUI.skin = menuSkin;

        float x = 0;
        float y = 0;
        float width = Screen.width;
        float height = Screen.height;
        GUI.BeginGroup(new Rect(x, y, width, height));

        // Display
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        string message = Message.GAME_OVER;
        if (winner != null)
        {
            message = string.Format("Congratulations {0}! You have won by {1}.", winner.username, metWinCondition.GetDescription());
        }

        float padding = Menu.padding;
        float buttonHeight = Menu.buttonHeight;
        float buttonWidth = Menu.buttonWidth;
        x = padding;
        y = padding;
        width = Screen.width - 2 * padding;
        height = buttonHeight;
        GUI.Label(new Rect(x, y, width, height), message);

        x = Screen.width / 2 - padding / 2 - buttonWidth;
        y += buttonHeight + padding;

        bool buttonPressed = GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), Message.NEW_GAME);
        if (buttonPressed == true)
        {
            Time.timeScale = 1f;
            GameManager.activeInstance.isMenuOpen = false;
            Application.LoadLevel(Levels.game);
        }

        x += padding + buttonWidth;

        buttonPressed = GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), Message.MAIN_MENU);
        if (buttonPressed == true)
        {
            GameManager.activeInstance.LoadLevel(string.Empty);
            Application.LoadLevel(Levels.mainMenu);
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