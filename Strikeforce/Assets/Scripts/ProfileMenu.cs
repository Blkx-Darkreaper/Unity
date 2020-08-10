using UnityEngine;
using System;
using System.Collections;

namespace Strikeforce
{
    public class ProfileMenu : Menu
    {
        private string selectedUsername;
        public int UsernameCharacterLimit = 14;
        public Texture2D[] Avatars;
        private int avatarIndex = 0;
        public GUISkin SelectionSkin;

        protected void Start()
        {
            selectedUsername = GlobalAssets.DefaultUsername;
            LoadAvatars();
            LoadUsernames();
        }

        protected override void OnGUI()
        {
            bool doubleClick = SelectionList.MouseDoubleClick();
            if (doubleClick == true)
            {
                selectedUsername = SelectionList.GetCurrentEntry();
                SelectProfile();
            }

            base.OnGUI();
        }

        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { };
        }

        private static void LoadUsernames()
        {
            if (ProfileManager.singleton == null)
            {
                throw new InvalidOperationException("Profile Manager failed to load");
            }

            string[] allUsernames = ProfileManager.singleton.GetAllUsernames();
            SelectionList.AddAllEntries(allUsernames);
        }

        private void LoadAvatars()
        {
            GlobalAssets.SetAvatars(Avatars);
        }

        protected override void DrawMenu()
        {
            GUI.skin = MenuSkin;

            float menuHeight = GetMenuHeight();

            float x = Screen.width / 2 - Attributes.Width / 2;
            float y = Screen.height / 2 - menuHeight / 2;
            float width = Attributes.Width;
            float height = menuHeight;
            Rect rect = new Rect(x, y, width, height);

            GUI.BeginGroup(rect);

            // Background box
            x = 0;
            y = 0;
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            // Menu buttons
            x = Attributes.Width / 2 - Attributes.ButtonWidth / 2;
            y = menuHeight - Attributes.Padding - Attributes.ButtonHeight;
            width = Attributes.ButtonWidth;
            height = Attributes.ButtonHeight;
            bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "Select");
            if (buttonPressed == true)
            {
                SelectProfile();
            }

            // Text area to enter new username
            x = Attributes.Padding;
            y = menuHeight - 2 * Attributes.Padding - Attributes.ButtonHeight - Attributes.TextHeight;
            width = Attributes.Width - 2 * Attributes.Padding;
            height = Attributes.TextHeight;
            selectedUsername = GUI.TextField(new Rect(x, y, width, height), selectedUsername, UsernameCharacterLimit);
            SelectionList.SetCurrentEntryToFirstMatch(selectedUsername);

            DrawAvatar(y);

            GUI.EndGroup();

            string previousSelection = SelectionList.GetCurrentEntry();
            DrawSelectionList(rect);

            string currentSelection = SelectionList.GetCurrentEntry();
            if (previousSelection != currentSelection)
            {
                SelectProfile(currentSelection);
            }
        }

        protected void SelectProfile()
        {
            SelectProfile(selectedUsername);
        }

        protected void SelectProfile(string usernameToFind)
        {
            if (ProfileManager.singleton == null)
            {
                throw new InvalidOperationException("Profile Manager failed to load");
            }

            Profile profile = ProfileManager.singleton.SwitchToProfile(usernameToFind, avatarIndex);
            if (profile == null)
            {
                return;
            }

            selectedUsername = profile.Username;
            avatarIndex = profile.AvatarId;
            Debug.Log(string.Format("Selected {0}'s account", selectedUsername));

            if (PreviousMenu == null)
            {
                return;
            }

            menuManager.ShowMenu(PreviousMenu);
        }

        protected void DrawSelectionList(Rect menu)
        {
            float x = menu.x + Attributes.Padding;
            float y = menu.y + Attributes.Padding;
            float width = menu.width - 2 * Attributes.Padding;
            float menuItemsHeight = GetMenuItemsHeight();
            float height = menu.height - menuItemsHeight - Attributes.Padding;
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

            float x = Attributes.Padding;
            float y = textY - Attributes.Padding - Attributes.ButtonHeight;
            float width = Attributes.ButtonHeight;
            float height = Attributes.ButtonHeight;
            bool buttonPressed = GUI.Button(new Rect(x, y, width, height), "<");
            if (buttonPressed == true)
            {
                avatarIndex -= 1;
                avatarIndex += Avatars.Length;
                avatarIndex %= Avatars.Length;
            }

            x = Attributes.Width - Attributes.Padding - Attributes.ButtonHeight;
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
            x = Attributes.Width / 2 - avatarImage.width / 2;
            y = textY - Attributes.Padding - avatarImage.height;
            width = avatarImage.width;
            height = avatarImage.height;
            GUI.DrawTexture(new Rect(x, y, width, height), avatarImage);
        }

        protected override float GetMenuHeight()
        {
            float menuHeight = Attributes.ButtonHeight + Attributes.TextHeight + 3 * Attributes.Padding;

            float menuItemsHeight = GetMenuItemsHeight();
            menuHeight += menuItemsHeight;

            return menuHeight;
        }

        protected float GetAvatarHeight()
        {
            float avatarHeight = 0;
            if (Avatars.Length > 0)
            {
                avatarHeight = Avatars[avatarIndex].height + 2 * Attributes.Padding;
            }
            return avatarHeight;
        }

        protected override float GetMenuItemsHeight()
        {
            float menuItemsHeight = Attributes.ButtonHeight + Attributes.TextHeight + 3 * Attributes.Padding;

            float avatarHeight = GetAvatarHeight();
            menuItemsHeight += avatarHeight;

            //menuItemsHeight += 50;

            return menuItemsHeight;
        }
    }
}