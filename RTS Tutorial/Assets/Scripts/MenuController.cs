using UnityEngine;
using System.Collections;
using RTS;

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

        float x = Screen.width / 2 - ResourceManager.Menu.width / 2;
        float y = Screen.height / 2 - menuHeight / 2;
        float width = ResourceManager.Menu.width;
        float height = menuHeight;
        GUI.BeginGroup(new Rect(x, y, width, height));

        // Background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        // Header image
        width = ResourceManager.Menu.headerWidth;
        height = ResourceManager.Menu.headerHeight;
        x = ResourceManager.Menu.width / 2 - width / 2;
        y = ResourceManager.Menu.padding;
        GUI.DrawTexture(new Rect(x, y, width, height), headerImage);

        // Welcome message
        width = ResourceManager.Menu.width - 2 * ResourceManager.Menu.padding;
        height = ResourceManager.Menu.textHeight;
        x = ResourceManager.Menu.width / 2 - width / 2;
        y = 2 * ResourceManager.Menu.padding + headerImage.height;
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

        x = ResourceManager.Menu.width / 2 - ResourceManager.Menu.buttonWidth / 2;
        //y = 2 * ResourceManager.Menu.padding + headerImage.height;
        y += ResourceManager.Menu.textHeight + ResourceManager.Menu.padding;
        width = ResourceManager.Menu.buttonWidth;
        height = ResourceManager.Menu.buttonHeight;

        for (int i = 0; i < buttons.Length; i++)
        {
            string buttonName = buttons[i];
            bool buttonPressed = GUI.Button(new Rect(x, y, width, height), buttonName);

            y += ResourceManager.Menu.buttonHeight + ResourceManager.Menu.padding;

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
        float paddingHeight = 2 * ResourceManager.Menu.padding;
        if (buttons != null)
        {
            buttonHeight = buttons.Length * ResourceManager.Menu.buttonHeight;
            paddingHeight += buttons.Length * ResourceManager.Menu.padding;
        }

        float messageHeight = ResourceManager.Menu.textHeight + ResourceManager.Menu.padding;

        float menuHeight = ResourceManager.Menu.headerHeight + buttonHeight + paddingHeight + messageHeight;
        return menuHeight;
    }

    protected virtual void HandleButtonPress(string buttonName)
    {
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