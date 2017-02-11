﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Menu : MonoBehaviour
    {
        public bool IsDrawingMenu;
        public Menu PreviousMenu;
        public GUISkin MenuSkin;
        public string MenuName;
        public Texture2D HeaderImage;
        public GameObject ButtonPrefab;
        public const string MENU_OPEN = "MenuOpen";
        public const string IS_OPENING_MENU = "IsOpeningMenu";
        public bool IsOpening { get { return animator.GetBool(IS_OPENING_MENU); } set { animator.SetBool(IS_OPENING_MENU, value); } }
		public bool IsOpen { get { return animator.GetCurrentAnimatorStateInfo (0).IsName (MENU_OPEN); } }
        protected MenuManager menuManager;
        protected Animator animator;
        protected CanvasGroup canvasGroup;
        protected string[] buttonNames;
        protected Dictionary<string, Button> allButtons;
        protected const string BACK = "Back";
        protected const string EXIT = "Quit Game";
        public struct Attributes
        {
            public static float Width { get { return HeaderWidth + 2 * ButtonHeight + 4 * Padding; } }
            public static float Padding = 10f;
            public static float TextHeight = 25f;
            public static float HeaderWidth = 256f;
            public static float HeaderHeight = 32f;
            public static float ButtonWidth { get { return (Width - 3 * Padding) / 2; } }
            public static float ButtonHeight = 40f;
            public static float pauseMenuHeight = 20f;
        }

        protected virtual void Awake()
        {
            menuManager = GetComponentInParent<MenuManager>();

            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();

            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
            }

            SetButtonNames();
            SetMenuButtons();
        }

        protected virtual void Start()
        {
            //account = transform.root.GetComponent<PlayerAccount>();
        }

        protected void Update()
        {
            if (IsOpen == false)
            {
                canvasGroup.blocksRaycasts = canvasGroup.interactable = false;
                return;
            }

            canvasGroup.blocksRaycasts = canvasGroup.interactable = true;

            HandleKeyboardActivity();
        }

        protected virtual void OnGUI()
        {
            if (IsOpen == false)
            {
                return;
            }

            if (IsDrawingMenu == false)
            {
                return;
            }

            DrawMenu();
        }

        protected virtual void SetButtonNames()
        {
            buttonNames = new string[] { BACK, EXIT };
        }

        protected virtual void SetMenuButtons()
        {
            if (IsDrawingMenu == true)
            {
                return;
            }

            SetHeaderText();

            if (buttonNames == null)
            {
                return;
            }
            if (buttonNames.Length == 0)
            {
                return;
            }

            if (ButtonPrefab == null)
            {
                return;
            }

            // Get the button group
            VerticalLayoutGroup buttonGroup = GetComponentInChildren<VerticalLayoutGroup>();
            if (buttonGroup == null)
            {
                return;
            }

            foreach (string buttonName in buttonNames)
            {
                GameObject buttonObject = Instantiate(ButtonPrefab) as GameObject;
                buttonObject.transform.SetParent(buttonGroup.transform);
                buttonObject.GetComponentInChildren<Text>().text = buttonName;
                //buttonObject.transform.rotation = buttonGroup.transform.rotation;
                buttonObject.transform.localRotation = buttonGroup.transform.localRotation;
                buttonObject.transform.localScale = buttonGroup.transform.localScale;

                Button button = buttonObject.GetComponent<Button>();
                AddButtonHandler(button, buttonName);
            }
        }

        protected virtual void AddButtonHandler(Button button, string buttonName)
        {
            if (button == null)
            {
                return;
            }

            button.name = buttonName;
            button.onClick.AddListener(() => { HandleButtonPress(button); });
            if (allButtons == null)
            {
                allButtons = new Dictionary<string, Button>();
            }

            allButtons.Add(buttonName, button);
        }

        private void SetHeaderText()
        {
            //GameObject header = GameObject.FindGameObjectWithTag(Tags.HEADER);
			Image header = GlobalAssets.GetChildComponentWithTag<Image>(gameObject, Tags.HEADER);
            if (header == null)
            {
                return;
            }

            Text displayText = header.GetComponentInChildren<Text>();
            if (displayText == null)
            {
                return;
            }

            displayText.text = MenuName;
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
            GUI.skin = MenuSkin;

            float menuHeight = GetMenuHeight();

            float x = Screen.width / 2 - Attributes.Width / 2;
            float y = Screen.height / 2 - menuHeight / 2;
            float width = Attributes.Width;
            float height = menuHeight;
            GUI.BeginGroup(new Rect(x, y, width, height));

            // Background box
            x = 0;
            y = 0;
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            // Header image
            width = Attributes.HeaderWidth;
            height = Attributes.HeaderHeight;
            x = Attributes.Width / 2 - width / 2;
            y = Attributes.Padding;
            GUI.DrawTexture(new Rect(x, y, width, height), HeaderImage);

            // Welcome message
            //width = MenuAttributes.Width - 2 * MenuAttributes.Padding;
            //height = MenuAttributes.TextHeight;
            //x = MenuAttributes.Width / 2 - width / 2;
            //y = 2 * MenuAttributes.Padding + HeaderImage.height;
            //string welcomeMessage = string.Format("Welcome {0}", GameManager.ActiveInstance.CurrentPlayerAccount.Username);
            //GUI.Label(new Rect(x, y, width, height), welcomeMessage);

            // Menu buttons
            if (buttonNames == null)
            {
                GUI.EndGroup();
                return;
            }
            if (buttonNames.Length == 0)
            {
                GUI.EndGroup();
                return;
            }

            x = Attributes.Width / 2 - Attributes.ButtonWidth / 2;
            //y = 2 * MenuAttributes.padding + headerImage.height;
            y += Attributes.TextHeight + Attributes.Padding;
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

                HandleButtonPress(buttonName);
            }

            GUI.EndGroup();
        }

        protected virtual float GetMenuHeight()
        {
            float buttonHeight = 0;
            float paddingHeight = 2 * Attributes.Padding;
            if (buttonNames != null)
            {
                buttonHeight = buttonNames.Length * Attributes.ButtonHeight;
                paddingHeight += buttonNames.Length * Attributes.Padding;
            }

            float messageHeight = Attributes.TextHeight + Attributes.Padding;

            float menuHeight = Attributes.HeaderHeight + buttonHeight + paddingHeight + messageHeight;
            return menuHeight;
        }

        protected virtual float GetMenuItemsHeight()
        {
            return 0f;
        }

        protected void HandleButtonPress(Button button)
        {
            bool buttonEnabled = button.interactable;
            if (buttonEnabled == false)
            {
                return;
            }

            string buttonName = button.name;
            HandleButtonPress(buttonName);
        }

        protected virtual void HandleButtonPress(string buttonName)
        {
            switch (buttonName)
            {
                case BACK:
                    Back();
                    break;

                case EXIT:
                    ExitGame();
                    break;
            }
        }

        public virtual void ShowMenu()
        {
            this.IsOpening = true;
        }

        public virtual void HideMenu()
        {
            this.IsOpening = false;
        }

        protected virtual void SwitchToMenu(Menu menu)
        {
            if (menu == null)
            {
                return;
            }

            if (menuManager == null)
            {
                Debug.LogError(string.Format("Menu manager hasn't been loaded."));
                return;
            }

            menuManager.ShowMenu(menu);
        }

        protected virtual void Pause()
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            IsOpening = true;
        }

        protected virtual void Resume()
        {
            Time.timeScale = 1f;
            //Cursor.visible = false;
            IsOpening = false;
        }

        protected virtual void Back()
        {
            SwitchToMenu(PreviousMenu);
        }

        public virtual void EnableMenu()
        {
            GetComponent<Menu>().enabled = true;
        }

        public virtual void DisableMenu()
        {
            GetComponent<Menu>().enabled = false;
        }

        public virtual void ToggleMenu()
        {
            Menu menu = GetComponent<Menu>();

            bool enabled = menu.enabled;
            menu.enabled = !enabled;
        }

        protected virtual void ExitGame()
        {
            string username = "Unknown player";
            Profile profile = ProfileManager.Singleton.CurrentProfile;
            if (profile != null)
            {
                username = profile.Username;
            }

            Debug.Log(string.Format("{0} has left the game", username));
            Application.Quit();
        }
    }
}