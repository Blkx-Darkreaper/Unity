using UnityEngine;
using System.Collections;
using Strikeforce;

public class MainMenuController : MenuController {
    protected const string NEW_GAME = "Start New Game";
    protected const string MATCHMAKING = "Matchmaking";
    protected const string PROFILE = "Profile";
    protected const string CHANGE_PLAYER = "Change Player Account";
    protected const string LOAD = "Load Game";
    protected const string OPTIONS = "Options";
    protected const string EXIT = "Exit"

    protected void OnLevelWasLoaded()
    {
        Cursor.visible = true;
        PlayerAccount currentAccount = GameManager.activeInstance.currentPlayerAccount;
        if (currentAccount == null)
        {
            GetComponent<MainMenu>().enabled = false;
            GetComponent<SelectPlayerMenu>().enabled = true;
        }
        else
        {
            GetComponent<MainMenu>().enabled = true;
            GetComponent<SelectPlayerMenu>().enabled = false;
        }
    }

    protected override void SetButtons()
    {
        buttons = new string[] { NEW_GAME, LOAD, CHANGE_PLAYER, EXIT };
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

            case LOAD:
                LoadMenu();
                break;

            case CHANGE_PLAYER:
                ChangePlayer();
                break;

            case EXIT:
                ExitGame();
                break;
        }
    }

    private void StartNewGame()
    {
        GameManager.activeInstance.isMenuOpen = false;
        string levelToLoad = Levels.game;
        Application.LoadLevel(levelToLoad);
        Time.timeScale = 1f;
    }

    private void ChangePlayer()
    {
        GetComponent<MainMenu>().enabled = false;
        GetComponent<SelectPlayerMenu>().enabled = true;

        string[] allUsernames = GameManager.activeInstance.GetAllUsernames();
        SelectionList.AddAllEntries(allUsernames);
    }

    protected override void HideCurrentMenu()
    {
        GetComponent<MainMenu>().enabled = false;
    }
}