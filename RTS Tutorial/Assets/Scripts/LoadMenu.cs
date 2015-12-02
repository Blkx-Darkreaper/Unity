using UnityEngine;
using System.Collections;
using RTS;

public class LoadMenu : MenuController {

    public GUISkin selectionSkin;

    protected override void Start()
    {
        Init();
    }

    public void Init()
    {
        string[] savedGames = GameManager.activeInstance.GetSavedGames();
        SelectionList.AddAllEntries(savedGames);
    }

    protected override void HandleKeyboardActivity()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    protected override void DrawMenu()
    {
        if (SelectionList.MouseDoubleClick() == true)
        {
            TryToLoad();
        }

        GUI.skin = menuSkin;
        float menuHeight = GetMenuHeight();

        float x = Screen.width / 2 - Menu.width / 2;
        float y = Screen.height / 2 - menuHeight / 2;
        float width = Menu.width;
        float height = menuHeight;
        Rect box = new Rect(x, y, width, height);

        GUI.BeginGroup(box);

        // Background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        // Menu buttons
        x = Menu.padding;
        y = menuHeight - Menu.padding - Menu.buttonHeight;
        width = Menu.buttonWidth;
        height = Menu.buttonHeight;
        bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "Load Game");
        if (buttonPressed == true)
        {
            TryToLoad();
			//return;
        }

        x += Menu.buttonWidth + Menu.padding;
        buttonPressed = GUI.Button(new Rect(x, y, width, height), "Cancel");
        if (buttonPressed == true)
        {
            Back();
			//return;
        }

        GUI.EndGroup();

        //selection list, needs to be called outside of the group for the menu
        float menuItemsHeight = GetMenuItemsHeight();

        x = box.x + Menu.padding;
        y = box.y + Menu.padding;
        width = box.width - 2 * Menu.padding;
        height = box.height - menuItemsHeight - Menu.padding;
        SelectionList.Draw(x, y, width, height, selectionSkin);
    }

    protected override float GetMenuHeight()
    {
        float menuHeight = 250;

        float menuItemsHeight = GetMenuItemsHeight();
        menuHeight += menuItemsHeight;

        return menuHeight;
    }

    protected override float GetMenuItemsHeight()
    {
        float menuItemsHeight = Menu.buttonHeight + Menu.textHeight + 3 * Menu.padding;
        return menuItemsHeight;
    }

    protected void TryToLoad()
    {
        string saveFileToLoad = SelectionList.GetCurrentEntry();

        bool emptyName = saveFileToLoad.Equals(string.Empty);
        if (emptyName == true)
        {
            return;
        }

        string levelToLoad = Levels.loadTest;
        GameManager.activeInstance.LoadGame(saveFileToLoad, levelToLoad);
        Application.LoadLevel(levelToLoad);
        //LoadManager.LoadGame(saveFileToLoad, levelToLoad);
        Time.timeScale = 1f;
    }

    protected override void Back()
    {
        GetComponent<LoadMenu>().enabled = false;
        PauseMenu pauseMenu = GetComponent<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.enabled = true;
        }
        else
        {
            MainMenu mainMenu = GetComponent<MainMenu>();
            if (mainMenu == null)
            {
                return;
            }

            mainMenu.enabled = true;
        }
    }
}