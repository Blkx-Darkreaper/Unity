using UnityEngine;
using System;
using System.Collections;
using Strikeforce;

public class MenuController : MonoBehaviour {

    public GUISkin menuSkin;
    public Texture2D headerImage;
    protected PlayerController player;
    protected string[] buttons;
    protected const string EXIT = "Quit Game";

    protected virtual void Start()
    {
        player = transform.root.GetComponent<PlayerController>();
        SetButtons();
        SetUsername();
    }

    protected void Update()
    {
        HandleKeyboardActivity();
    }

    protected virtual void OnGUI()
    {
        DrawMenu();
    }

    protected virtual void SetButtons()
    {
        buttons = new string[] { };
    }

    protected virtual void SetUsername()
    {
        if (GameManager.activeInstance == null)
        {
            return;
        }
        if (GameManager.activeInstance.currentPlayerAccount == null)
        {
            return;
        }

        string username = GameManager.activeInstance.currentPlayerAccount.username;
        SetUsername(username);
    }

    protected virtual void SetUsername(string username)
    {
        if (player == null)
        {
            return;
        }
        bool alreadySet = !player.username.Equals(string.Empty);
        if (alreadySet == true)
        {
            return;
        }

        player.username = username;
    }

    protected virtual void HandleKeyboardActivity()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }

    protected virtual void DrawMenu()
    {
        GUI.skin = menuSkin;

        float menuHeight = GetMenuHeight();

        float x = Screen.width / 2 - Menu.width / 2;
        float y = Screen.height / 2 - menuHeight / 2;
        float width = Menu.width;
        float height = menuHeight;
        GUI.BeginGroup(new Rect(x, y, width, height));

        // Background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        // Header image
        width = Menu.headerWidth;
        height = Menu.headerHeight;
        x = Menu.width / 2 - width / 2;
        y = Menu.padding;
        GUI.DrawTexture(new Rect(x, y, width, height), headerImage);

        // Welcome message
        width = Menu.width - 2 * Menu.padding;
        height = Menu.textHeight;
        x = Menu.width / 2 - width / 2;
        y = 2 * Menu.padding + headerImage.height;
        string welcomeMessage = string.Format("Welcome {0}", GameManager.activeInstance.currentPlayerAccount.username);
        GUI.Label(new Rect(x, y, width, height), welcomeMessage);

        // Menu buttons
        if (buttons == null)
        {
            GUI.EndGroup();
            return;
        }
        if (buttons.Length == 0)
        {
            GUI.EndGroup();
            return;
        }

        x = Menu.width / 2 - Menu.buttonWidth / 2;
        //y = 2 * ResourceManager.Menu.padding + headerImage.height;
        y += Menu.textHeight + Menu.padding;
        width = Menu.buttonWidth;
        height = Menu.buttonHeight;

        for (int i = 0; i < buttons.Length; i++)
        {
            string buttonName = buttons[i];
            bool buttonPressed = GUI.Button(new Rect(x, y, width, height), buttonName);

            y += Menu.buttonHeight + Menu.padding;

            if (buttonPressed == false)
            {
                continue;
            }

            HandleButtonPress(buttonName);
        }

        GUI.EndGroup();
    }

    protected virtual float GetMenuHeight()
    {
        float buttonHeight = 0;
        float paddingHeight = 2 * Menu.padding;
        if (buttons != null)
        {
            buttonHeight = buttons.Length * Menu.buttonHeight;
            paddingHeight += buttons.Length * Menu.padding;
        }

        float messageHeight = Menu.textHeight + Menu.padding;

        float menuHeight = Menu.headerHeight + buttonHeight + paddingHeight + messageHeight;
        return menuHeight;
    }

    protected virtual float GetMenuItemsHeight()
    {
        return 0f;
    }

    protected virtual void HideCurrentMenu()
    {
        GetComponent<MenuController>().enabled = false;
    }

    protected virtual void HandleButtonPress(string buttonName)
    {
    }

    protected void LoadMenu()
    {
        HideCurrentMenu();
        LoadMenu loadMenu = GetComponent<LoadMenu>();
        if (loadMenu == null)
        {
            return;
        }

        loadMenu.enabled = true;
        loadMenu.Init();
    }

    protected virtual void Resume()
    {
        Time.timeScale = 1f;
        GetComponent<PauseMenu>().enabled = false;
        if (player != null)
        {
            player.GetComponent<UserInput>().enabled = true;
        }
        Cursor.visible = false;
        GameManager.activeInstance.isMenuOpen = false;
    }

    protected virtual void Back()
    {
    }

    protected virtual void ExitGame()
    {
        string playerName = "Unknown player";
        if(player != null) {
            playerName = player.username;
        }

        Debug.Log(string.Format("{0} has left the game", playerName));
        Application.Quit();
    }
}