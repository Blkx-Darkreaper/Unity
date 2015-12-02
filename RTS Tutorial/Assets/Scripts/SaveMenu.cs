using UnityEngine;
using System.Collections;
using RTS;

public class SaveMenu : MenuController
{

    public GUISkin selectionSkin;
    private string saveName;
    private int saveNameCharacterLimit = 60;
    private ConfirmDialogController confirmDialog;

    protected override void Start()
    {
        Init();
    }

    protected override void OnGUI()
    {
        bool confirming = confirmDialog.isConfirming;
        if (confirming == true)
        {
            string message = string.Format("\"{0}\" already exists. Do you want to overwrite?", saveName);
            confirmDialog.ShowDialog(message, menuSkin);
        }

        bool selectionMade = confirmDialog.hasMadeSelection;
        if (selectionMade == true)
        {
            bool confirmed = confirmDialog.hasClickedYes;
            if (confirmed == true)
            {
                SaveGame();
            }

            confirmDialog.Done();
        }

        base.OnGUI();

        if (Event.current.keyCode == KeyCode.Return)
        {
            SaveGame();
        }

        bool doubleClicked = SelectionList.MouseDoubleClick();
        if (doubleClicked == true)
        {
            saveName = SelectionList.GetCurrentEntry();
            SaveGame();
        }
    }

    public void Init()
    {
        confirmDialog = new ConfirmDialogController();

        if (GameManager.activeInstance == null)
        {
            return;
        }

        saveName = GetSaveName();

        string[] savedGames = GameManager.activeInstance.GetSavedGames();
        SelectionList.AddAllEntries(savedGames);
    }

    protected override void HandleKeyboardActivity()
    {
        // Escape key handler
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool confirming = confirmDialog.isConfirming;
            if (confirming == true)
            {
                confirmDialog.Done();
                return;
            }

            Back();
        }

        // Enter key handler
        if(Input.GetKeyDown(KeyCode.Return)) {
            bool confirming = confirmDialog.isConfirming;
            if (confirming == true)
            {
                confirmDialog.Done();
                SaveGame();
            }
        }
    }

    private string GetSaveName()
    {
        string saveName = GameManager.activeInstance.defaultSaveName;

        string currentLevelName = GameManager.activeInstance.currentGameName;
        if (currentLevelName != null)
        {
            if (!currentLevelName.Equals(string.Empty))
            {
                saveName = currentLevelName;
            }
        }

        string currentSaveName = GameManager.activeInstance.currentSaveGameName;
        if (currentSaveName != null)
        {
            if (!currentSaveName.Equals(string.Empty))
            {
                saveName = currentSaveName;
            }
        }

        return saveName;
    }

    protected override void DrawMenu()
    {
        GUI.skin = menuSkin;

        float menuHeight = GetMenuHeight();
        float x = Screen.width / 2 - Menu.width / 2;
        float y = Screen.height / 2 - menuHeight / 2;
        float width = Menu.width;
        float height = menuHeight;
        Rect box = new Rect(x, y, width, height);

        GUI.BeginGroup(box);

        //background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        //menu buttons
        x = Menu.padding;
        y = menuHeight - Menu.padding - Menu.buttonHeight;
        width = Menu.buttonWidth;
        height = Menu.buttonHeight;
        bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "Save Game");
        if (buttonPressed == true)
        {
            TryToSaveGame();
        }

        x += Menu.buttonWidth + Menu.padding;
        buttonPressed = GUI.Button(new Rect(x, y, width, height), "Cancel");
        if (buttonPressed == true)
        {
            Back();
        }

        //text area for player to type new name
        x = Menu.padding;
        y = menuHeight - 2 * Menu.padding - Menu.buttonHeight - Menu.textHeight;
        width = Menu.width - 2 * Menu.padding;
        height = Menu.textHeight;
        saveName = GUI.TextField(new Rect(x, y, width, height), saveName, saveNameCharacterLimit);

        SelectionList.SetCurrentEntryToFirstMatch(saveName);

        GUI.EndGroup();

        //selection list, needs to be called outside of the group for the menu
        string previousSelection = SelectionList.GetCurrentEntry();
        float menuItemsHeight = GetMenuItemsHeight();

        x = box.x + Menu.padding;
        y = box.y + Menu.padding;
        width = box.width - 2 * Menu.padding;
        height = box.height - menuItemsHeight - Menu.padding;
        SelectionList.Draw(x, y, width, height, selectionSkin);

        string currentSelection = SelectionList.GetCurrentEntry();
        //set saveName to be name selected in list if selection has changed
        if (previousSelection != currentSelection)
        {
            saveName = currentSelection;
        }
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

    private void TryToSaveGame()
    {
        bool overwrite = SelectionList.Contains(saveName);
        if (overwrite == true)
        {
            confirmDialog.Confirm();
        }
        else
        {
            SaveGame();
        }
    }

    private void SaveGame()
    {
        GameManager.activeInstance.currentSaveGameName = saveName;
        SaveManager.SaveGame(saveName);
        Back();
    }

    protected override void Back()
    {
        GetComponent<SaveMenu>().enabled = false;
        PauseMenu pauseMenu = GetComponent<PauseMenu>();
        if (pauseMenu == null)
        {
            return;
        }

        pauseMenu.enabled = true;
    }
}