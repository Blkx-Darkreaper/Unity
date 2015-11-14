using UnityEngine;
using System.Collections;
using RTS;

public class MainMenu : MenuController {

    protected const string NEW_GAME = "Start New Game";
    protected const string CHANGE_PLAYER = "Change Player Account";

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
        buttons = new string[] { NEW_GAME, CHANGE_PLAYER, EXIT };
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
        string levelToLoad = ResourceManager.Levels.GAME;
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
}