using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class PauseMenu : Menu
    {
        public GUISkin PauseMenuSkin;
        protected const string RESUME = "Resume";

        protected override void SetButtonNames()
        {
            buttons = new string[] { RESUME, EXIT };
        }

        protected override void OnGUI()
        {
            GUI.skin = PauseMenuSkin;

            float x = Screen.width / 2 - MenuAttributes.Width / 2;
            float y = Screen.height / 2 - MenuAttributes.pauseMenuHeight / 2;
            float width = MenuAttributes.Width;
            float height = MenuAttributes.pauseMenuHeight;

            GUI.BeginGroup(new Rect(x, y, width, height));

            // Background box
            x = 0;
            y = 0;
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            // Header image
            x = MenuAttributes.Padding;
            y = MenuAttributes.Padding;
            width = MenuAttributes.HeaderWidth;
            height = MenuAttributes.HeaderHeight;
            GUI.DrawTexture(new Rect(x, y, width, height), HeaderImage);

            // Menu buttons
            x = MenuAttributes.Width / 2 - MenuAttributes.ButtonWidth / 2;
            y = 2 * MenuAttributes.Padding + HeaderImage.height;
            width = MenuAttributes.ButtonWidth;
            height = MenuAttributes.ButtonHeight;

            for (int i = 0; i < buttons.Length; i++)
            {
                string buttonName = buttons[i];
                bool buttonPressed = GUI.Button(new Rect(x, y, width, height), buttonName);

                y += MenuAttributes.ButtonHeight + MenuAttributes.Padding;

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
}