using UnityEngine;
using System.Collections;
using RTS;

public class PauseMenu : MenuController {

    protected const string RESUME = "Resume";
    protected const string MAIN_MENU = "Main Menu";
    protected const string SAVE = "Save Game";
    protected const string LOAD = "Load Game";

    protected override void SetButtons()
    {
        buttons = new string[] { RESUME, SAVE, LOAD, MAIN_MENU };
    }

    protected override void HandleButtonPress(string buttonName)
    {
        switch (buttonName)
        {
            case RESUME:
                Resume();
                break;

            case SAVE:
                SaveMenu();
                break;

            case LOAD:
                LoadMenu();
                break;

            case MAIN_MENU:
                Back();
                break;
        }
    }

    protected override float GetMenuHeight()
    {
        float menuHeight = Menu.headerHeight;

        float menuItemsHeight = GetMenuItemsHeight();
        menuHeight += menuItemsHeight;

        float messageHeight = Menu.textHeight + Menu.padding;
        menuHeight += messageHeight;

        return menuHeight;
    }

    protected override float GetMenuItemsHeight()
    {
        int menuItems = buttons.Length;
        float menuItemsHeight = menuItems * Menu.buttonHeight + 2 * menuItems * Menu.padding;
        return menuItemsHeight;
    }

    private void SaveMenu()
    {
        GetComponent<PauseMenu>().enabled = false;
        SaveMenu saveMenu = GetComponent<SaveMenu>();
        if (saveMenu == null)
        {
            return;
        }

        saveMenu.enabled = true;
        saveMenu.Init();
    }

    protected override void Back()
    {
        GameManager.activeInstance.ExitGame();
        string levelToLoad = Levels.mainMenu;
        Application.LoadLevel(levelToLoad);
        Cursor.visible = true;
    }

    protected override void HideCurrentMenu()
    {
        GetComponent<PauseMenu>().enabled = false;
    }
}