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
            buttonNames = new string[] { RESUME, EXIT };
        }

        protected override void OnGUI()
        {
            GUI.skin = PauseMenuSkin;

            float x = Screen.width / 2 - Attributes.Width / 2;
            float y = Screen.height / 2 - Attributes.pauseMenuHeight / 2;
            float width = Attributes.Width;
            float height = Attributes.pauseMenuHeight;

            GUI.BeginGroup(new Rect(x, y, width, height));

            // Background box
            x = 0;
            y = 0;
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            // Header image
            x = Attributes.Padding;
            y = Attributes.Padding;
            width = Attributes.HeaderWidth;
            height = Attributes.HeaderHeight;
            GUI.DrawTexture(new Rect(x, y, width, height), HeaderImage);

            // Menu buttons
            x = Attributes.Width / 2 - Attributes.ButtonWidth / 2;
            y = 2 * Attributes.Padding + HeaderImage.height;
            width = Attributes.ButtonWidth;
            height = Attributes.ButtonHeight;

            for (int i = 0; i < buttonNames.Length; i++)
            {
                string buttonName = buttonNames[i];
                bool buttonPressed = GUI.Button(new Rect(x, y, width, height), buttonName);

                y += Attributes.ButtonHeight + Attributes.Padding;

                if (buttonPressed == false)
                {
                    continue;
                }

                switch (buttonName)
                {
                    case "Resume":
                        menuManager.Resume();
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