using UnityEngine;
using System.Collections;
using RTS;

public class PauseMenuController : MenuController {

    public GUISkin pauseMenuSkin;
    public Texture2D headerImage;

    protected override void Start()
    {
        base.Start();
        buttons = new string[] { "Resume", "Exit" };
    }

    protected void OnGUI()
    {
        GUI.skin = pauseMenuSkin;

        float x = Screen.width / 2 - ResourceManager.Menu.width / 2;
        float y = Screen.height / 2 - ResourceManager.Menu.pauseMenuHeight / 2;
        float width = ResourceManager.Menu.width;
        float height = ResourceManager.Menu.pauseMenuHeight;

        GUI.BeginGroup(new Rect(x, y, width, height));

        // Background box
        x = 0;
        y = 0;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        // Header image
        x = ResourceManager.Menu.padding;
        y = ResourceManager.Menu.padding;
        width = ResourceManager.Menu.headerWidth;
        height = ResourceManager.Menu.headerHeight;
        GUI.DrawTexture(new Rect(x, y, width, height), headerImage);

        // Menu buttons
        x = ResourceManager.Menu.width / 2 - ResourceManager.Menu.buttonWidth / 2;
        y = 2 * ResourceManager.Menu.padding + headerImage.height;
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

            switch (buttonName)
            {
                case "Resume":
                    Resume();
                    break;

                case "Exit":
                    ExitGame();
                    break;
            }
        }

        GUI.EndGroup();
    }
}