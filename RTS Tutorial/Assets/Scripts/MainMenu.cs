using UnityEngine;
using System.Collections;
using RTS;

public class MainMenu : MenuController {

    protected const string NEW_GAME = "Start New Game";

    protected override void SetButtons()
    {
        buttons = new string[] { NEW_GAME, EXIT };
    }

    protected override void HandleKeyboardActivity()
    {
        return;
    }

    protected override void HandleButtonPress(string buttonName)
    {
        switch (buttonName)
        {
            case NEW_GAME:
                StartNewGame();
                break;

            case EXIT:
                ExitGame();
                break;
        }
    }

    private void StartNewGame()
    {
        GameManager.activeInstance.isMenuOpen = false;
        string levelToLoad = ResourceManager.Levels.GAME;
        Application.LoadLevel(levelToLoad);
        Time.timeScale = 1f;
    }
}