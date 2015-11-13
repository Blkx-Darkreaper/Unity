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

        float x = Screen.width / 2 - ResourceManager.Menu.width / 2;
        float y = Screen.height / 2 - menuHeight / 2;
        float width = ResourceManager.Menu.width;
        float height = menuHeight;
        Rect rect = new Rect(x, y, width, height);

        GUI.BeginGroup(rect);

        // Background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        // Menu buttons
        x = ResourceManager.Menu.width / 2 - ResourceManager.Menu.buttonWidth / 2;
        y = menuHeight - ResourceManager.Menu.padding - ResourceManager.Menu.buttonHeight;
        width = ResourceManager.Menu.buttonWidth;
        height = ResourceManager.Menu.buttonHeight;
        bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "Select");
        if (buttonPressed == true)
        {
            SelectPlayer();
        }

        // Text area to enter new username
        x = ResourceManager.Menu.padding;
        y = menuHeight - 2 * ResourceManager.Menu.padding - ResourceManager.Menu.buttonHeight - ResourceManager.Menu.textHeight;
        width = ResourceManager.Menu.width - 2 * ResourceManager.Menu.padding;
        height = ResourceManager.Menu.textHeight;
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
        float x = menu.x + ResourceManager.Menu.padding;
        float y = menu.y + ResourceManager.Menu.padding;
        float width = menu.width - 2 * ResourceManager.Menu.padding;
        float menuItemsHeight = GetMenuItemsHeight();
        float height = menu.height - menuItemsHeight - ResourceManager.Menu.padding;
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

        float x = ResourceManager.Menu.padding;
        float y = textY - ResourceManager.Menu.padding - ResourceManager.Menu.buttonHeight;
        float width = ResourceManager.Menu.buttonHeight;
        float height = ResourceManager.Menu.buttonHeight;
        bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "<");
        if (buttonPressed == true)
        {
            avatarIndex -= 1;
            avatarIndex += avatars.Length;
            avatarIndex %= avatars.Length;
        }

        x = ResourceManager.Menu.width - ResourceManager.Menu.padding - ResourceManager.Menu.buttonHeight;
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
        x = ResourceManager.Menu.width / 2 - avatarImage.width / 2;
        y = textY - ResourceManager.Menu.padding - avatarImage.height;
        width = avatarImage.width;
        height = avatarImage.height;
        GUI.DrawTexture(new Rect(x, y, width, height), avatarImage);

    }

    protected override float GetMenuHeight()
    {
        float menuHeight = ResourceManager.Menu.buttonHeight + ResourceManager.Menu.textHeight + 3 * ResourceManager.Menu.padding;

        float menuItemsHeight = GetMenuItemsHeight();
        menuHeight += menuItemsHeight;

        return menuHeight;
    }

    private float GetAvatarHeight()
    {
        float avatarHeight = 0;
        if (avatars.Length > 0)
        {
            avatarHeight = avatars[avatarIndex].height + 2 * ResourceManager.Menu.padding;
        }
        return avatarHeight;
    }

    private float GetMenuItemsHeight()
    {
        float menuItemsHeight = ResourceManager.Menu.buttonHeight + ResourceManager.Menu.textHeight + 3 * ResourceManager.Menu.padding;

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
    }
}