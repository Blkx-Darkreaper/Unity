using UnityEngine;
using System.Collections;
using RTS;

public class PauseMenu : MenuController {

    protected const string RESUME = "Resume";
    protected const string MAIN_MENU = "Main Menu";

    protected override void SetButtons()
    {
        buttons = new string[] { RESUME, MAIN_MENU };
    }

    protected override void HandleButtonPress(string buttonName)
    {
        switch (buttonName)
        {
            case RESUME:
                Resume();
                break;

            case MAIN_MENU:
                ReturnToMainMenu();
                break;
        }
    }

    protected override float GetMenuHeight()
    {
        float messageHeight = ResourceManager.Menu.textHeight + ResourceManager.Menu.padding;

        float menuHeight = ResourceManager.Menu.pauseMenuHeight + messageHeight;
        return menuHeight;
    }

    private void ReturnToMainMenu()
    {
        string levelToLoad = ResourceManager.Levels.MAIN_MENU;
        Application.LoadLevel(levelToLoad);
        Cursor.visible = true;
    }
}