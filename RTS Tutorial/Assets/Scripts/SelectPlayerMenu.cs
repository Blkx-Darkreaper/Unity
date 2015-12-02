using UnityEngine;
using System.Collections;
using RTS;

public class SelectPlayerMenu : MenuController {

    private string selectedUsername;
    public int usernameCharacterLimit = 14;
    public Texture2D[] avatars;
    private int avatarIndex = 0;
    public GUISkin selectionSkin;

    protected override void Start()
    {
        base.Start();
        selectedUsername = GameManager.activeInstance.defaultUsername;
        LoadAvatars();
        LoadUsernames();
    }

    protected override void OnGUI()
    {
        bool doubleClick = SelectionList.MouseDoubleClick();
        if (doubleClick == true)
        {
            selectedUsername = SelectionList.GetCurrentEntry();
            SelectPlayer();
        }

        base.OnGUI();
    }

    private static void LoadUsernames()
    {
        string[] allUsernames = GameManager.activeInstance.GetAllUsernames();
        SelectionList.AddAllEntries(allUsernames);
    }

    private void LoadAvatars()
    {
        ResourceManager.SetAvatars(avatars);
    }

    protected override void HandleKeyboardActivity()
    {
        return;
    }

    protected override void DrawMenu()
    {
        GUI.skin = menuSkin;

        float menuHeight = GetMenuHeight();

        float x = Screen.width / 2 - Menu.width / 2;
        float y = Screen.height / 2 - menuHeight / 2;
        float width = Menu.width;
        float height = menuHeight;
        Rect rect = new Rect(x, y, width, height);

        GUI.BeginGroup(rect);

        // Background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        // Menu buttons
        x = Menu.width / 2 - Menu.buttonWidth / 2;
        y = menuHeight - Menu.padding - Menu.buttonHeight;
        width = Menu.buttonWidth;
        height = Menu.buttonHeight;
        bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "Select");
        if (buttonPressed == true)
        {
            SelectPlayer();
        }

        // Text area to enter new username
        x = Menu.padding;
        y = menuHeight - 2 * Menu.padding - Menu.buttonHeight - Menu.textHeight;
        width = Menu.width - 2 * Menu.padding;
        height = Menu.textHeight;
        selectedUsername = GUI.TextField(new Rect(x, y, width, height), selectedUsername, usernameCharacterLimit);
        SelectionList.SetCurrentEntryToFirstMatch(selectedUsername);

        DrawAvatar(y);

        GUI.EndGroup();

        string previousSelection = SelectionList.GetCurrentEntry();
        DrawSelectionList(rect);

        string currentSelection = SelectionList.GetCurrentEntry();
        if (previousSelection != currentSelection)
        {
            SelectPlayerAccount(currentSelection);
        }
    }

    private void SelectPlayerAccount(string usernameToFind)
    {
        PlayerAccount account = GameManager.activeInstance.GetPlayerAccount(usernameToFind);
        if (account == null)
        {
            return;
        }

        selectedUsername = account.username;
        avatarIndex = account.avatarId;
    }

    protected void DrawSelectionList(Rect menu)
    {
        float x = menu.x + Menu.padding;
        float y = menu.y + Menu.padding;
        float width = menu.width - 2 * Menu.padding;
        float menuItemsHeight = GetMenuItemsHeight();
        float height = menu.height - menuItemsHeight - Menu.padding;
        SelectionList.Draw(x, y, width, height, selectionSkin);
    }

    protected void DrawAvatar(float textY)
    {
        if (avatars == null)
        {
            return;
        }
        if (avatars.Length == 0)
        {
            return;
        }

        float x = Menu.padding;
        float y = textY - Menu.padding - Menu.buttonHeight;
        float width = Menu.buttonHeight;
        float height = Menu.buttonHeight;
        bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "<");
        if (buttonPressed == true)
        {
            avatarIndex -= 1;
            avatarIndex += avatars.Length;
            avatarIndex %= avatars.Length;
        }

        x = Menu.width - Menu.padding - Menu.buttonHeight;
        buttonPressed = GUI.Button(new Rect(x, y, width, height), ">");
        if (buttonPressed == true)
        {
            avatarIndex += 1;
            avatarIndex %= avatars.Length;
        }

        if (avatarIndex < 0)
        {
            return;
        }

        Texture2D avatarImage = avatars[avatarIndex];
        x = Menu.width / 2 - avatarImage.width / 2;
        y = textY - Menu.padding - avatarImage.height;
        width = avatarImage.width;
        height = avatarImage.height;
        GUI.DrawTexture(new Rect(x, y, width, height), avatarImage);
    }

    protected override float GetMenuHeight()
    {
        float menuHeight = Menu.buttonHeight + Menu.textHeight + 3 * Menu.padding;

        float menuItemsHeight = GetMenuItemsHeight();
        menuHeight += menuItemsHeight;

        return menuHeight;
    }

    private float GetAvatarHeight()
    {
        float avatarHeight = 0;
        if (avatars.Length > 0)
        {
            avatarHeight = avatars[avatarIndex].height + 2 * Menu.padding;
        }
        return avatarHeight;
    }

    protected override float GetMenuItemsHeight()
    {
        float menuItemsHeight = Menu.buttonHeight + Menu.textHeight + 3 * Menu.padding;

        float avatarHeight = GetAvatarHeight();
        menuItemsHeight += avatarHeight;

        //menuItemsHeight += 50;

        return menuItemsHeight;
    }

    private void SelectPlayer()
    {
        GameManager.activeInstance.SetCurrentPlayerAccount(selectedUsername, avatarIndex);
        //SetUsername(username);
        GetComponent<SelectPlayerMenu>().enabled = false;
        MainMenu mainMenu = GetComponent<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.enabled = true;
        }
		
		Debug.Log(string.Format("Selected {0}'s account", selectedUsername));
    }
}