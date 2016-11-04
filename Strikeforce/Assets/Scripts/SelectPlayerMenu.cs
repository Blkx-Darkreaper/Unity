using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class SelectPlayerMenu : Menu
    {
        private string selectedUsername;
        public int UsernameCharacterLimit = 14;
        public Texture2D[] Avatars;
        private int avatarIndex = 0;
        public GUISkin SelectionSkin;

        protected override void Start()
        {
            base.Start();
            selectedUsername = GameManager.ActiveInstance.DefaultUsername;
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
            string[] allUsernames = GameManager.ActiveInstance.GetAllUsernames();
            SelectionList.AddAllEntries(allUsernames);
        }

        private void LoadAvatars()
        {
            GlobalAssets.SetAvatars(Avatars);
        }

        protected override void HandleKeyboardActivity()
        {
            return;
        }

        protected override void DrawMenu()
        {
            GUI.skin = MenuSkin;

            float menuHeight = GetMenuHeight();

            float x = Screen.width / 2 - MenuAttributes.Width / 2;
            float y = Screen.height / 2 - menuHeight / 2;
            float width = MenuAttributes.Width;
            float height = menuHeight;
            Rect rect = new Rect(x, y, width, height);

            GUI.BeginGroup(rect);

            // Background box
            x = 0;
            y = 0;
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            // Menu buttons
            x = MenuAttributes.Width / 2 - MenuAttributes.ButtonWidth / 2;
            y = menuHeight - MenuAttributes.Padding - MenuAttributes.ButtonHeight;
            width = MenuAttributes.ButtonWidth;
            height = MenuAttributes.ButtonHeight;
            bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "Select");
            if (buttonPressed == true)
            {
                SelectPlayer();
            }

            // Text area to enter new username
            x = MenuAttributes.Padding;
            y = menuHeight - 2 * MenuAttributes.Padding - MenuAttributes.ButtonHeight - MenuAttributes.TextHeight;
            width = MenuAttributes.Width - 2 * MenuAttributes.Padding;
            height = MenuAttributes.TextHeight;
            selectedUsername = GUI.TextField(new Rect(x, y, width, height), selectedUsername, UsernameCharacterLimit);
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
            PlayerAccount account = GameManager.ActiveInstance.GetPlayerAccount(usernameToFind);
            if (account == null)
            {
                return;
            }

            selectedUsername = account.username;
            avatarIndex = account.avatarId;
        }

        protected void DrawSelectionList(Rect menu)
        {
            float x = menu.x + MenuAttributes.Padding;
            float y = menu.y + MenuAttributes.Padding;
            float width = menu.width - 2 * MenuAttributes.Padding;
            float menuItemsHeight = GetMenuItemsHeight();
            float height = menu.height - menuItemsHeight - MenuAttributes.Padding;
            SelectionList.Draw(x, y, width, height, SelectionSkin);
        }

        protected void DrawAvatar(float textY)
        {
            if (Avatars == null)
            {
                return;
            }
            if (Avatars.Length == 0)
            {
                return;
            }

            float x = MenuAttributes.Padding;
            float y = textY - MenuAttributes.Padding - MenuAttributes.ButtonHeight;
            float width = MenuAttributes.ButtonHeight;
            float height = MenuAttributes.ButtonHeight;
            bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "<");
            if (buttonPressed == true)
            {
                avatarIndex -= 1;
                avatarIndex += Avatars.Length;
                avatarIndex %= Avatars.Length;
            }

            x = MenuAttributes.Width - MenuAttributes.Padding - MenuAttributes.ButtonHeight;
            buttonPressed = GUI.Button(new Rect(x, y, width, height), ">");
            if (buttonPressed == true)
            {
                avatarIndex += 1;
                avatarIndex %= Avatars.Length;
            }

            if (avatarIndex < 0)
            {
                return;
            }

            Texture2D avatarImage = Avatars[avatarIndex];
            x = MenuAttributes.Width / 2 - avatarImage.width / 2;
            y = textY - MenuAttributes.Padding - avatarImage.height;
            width = avatarImage.width;
            height = avatarImage.height;
            GUI.DrawTexture(new Rect(x, y, width, height), avatarImage);
        }

        protected override float GetMenuHeight()
        {
            float menuHeight = MenuAttributes.ButtonHeight + MenuAttributes.TextHeight + 3 * MenuAttributes.Padding;

            float menuItemsHeight = GetMenuItemsHeight();
            menuHeight += menuItemsHeight;

            return menuHeight;
        }

        private float GetAvatarHeight()
        {
            float avatarHeight = 0;
            if (Avatars.Length > 0)
            {
                avatarHeight = Avatars[avatarIndex].height + 2 * MenuAttributes.Padding;
            }
            return avatarHeight;
        }

        protected override float GetMenuItemsHeight()
        {
            float menuItemsHeight = MenuAttributes.ButtonHeight + MenuAttributes.TextHeight + 3 * MenuAttributes.Padding;

            float avatarHeight = GetAvatarHeight();
            menuItemsHeight += avatarHeight;

            //menuItemsHeight += 50;

            return menuItemsHeight;
        }

        private void SelectPlayer()
        {
            GameManager.ActiveInstance.SetCurrentPlayerAccount(selectedUsername, avatarIndex);
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
}