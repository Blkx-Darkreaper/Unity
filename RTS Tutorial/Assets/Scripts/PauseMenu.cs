using UnityEngine;
using System.Collections;
using RTS;

public class PauseMenu : MenuController {

    protected const string RESUME = "Resume";
    protected const string MAIN_MENU = "Main Menu";
    protected const string SAVE = "Save Game";

    protected override void SetButtons()
    {
        buttons = new string[] { RESUME, SAVE, MAIN_MENU };
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

            case MAIN_MENU:
                Back();
                break;
        }
    }

    protected override float GetMenuHeight()
    {
        float menuHeight = ResourceManager.Menu.headerHeight;

        float menuItemsHeight = GetMenuItemsHeight();
        menuHeight += menuItemsHeight;

        float messageHeight = ResourceManager.Menu.textHeight + ResourceManager.Menu.padding;
        menuHeight += messageHeight;

        return menuHeight;
    }

    protected override float GetMenuItemsHeight()
    {
        int menuItems = buttons.Length;
        float menuItemsHeight = menuItems * ResourceManager.Menu.buttonHeight + 2 * menuItems * ResourceManager.Menu.padding;
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
        string levelToLoad = ResourceManager.Levels.MAIN_MENU;
        Application.LoadLevel(levelToLoad);
        Cursor.visible = true;
    }
}