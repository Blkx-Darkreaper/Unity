using UnityEngine;
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
        public bool IsOpeningMenu { get { return animator.GetBool(IS_OPENING_MENU); } set { animator.SetBool(IS_OPENING_MENU, value); } }
		public bool IsMenuOpen { get { return animator.GetCurrentAnimatorStateInfo (0).IsName (MENU_OPEN); } }
        protected MenuManager menuManager;
        protected Animator animator;
        protected CanvasGroup canvasGroup;
        protected string[] buttonNames;
        protected Dictionary<string, Button> allButtons;
        protected const string EXIT = "Quit Game";

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
            if (IsMenuOpen == false)
            {
                canvasGroup.blocksRaycasts = canvasGroup.interactable = false;
                return;
            }

            canvasGroup.blocksRaycasts = canvasGroup.interactable = true;

            HandleKeyboardActivity();
        }

        protected virtual void OnGUI()
        {
            if (IsMenuOpen == false)
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
            buttonNames = new string[] { EXIT };
        }

        protected virtual void SetMenuButtons()
        {
            if (IsDrawingMenu == true)
            {
                return;
            }

            SetHeaderText();

            if (ButtonPrefab == null)
            {
                return;
            }

            // Get the button group
            VerticalLayoutGroup buttonGroup = GetComponentInChildren<VerticalLayoutGroup>();
            allButtons = new Dictionary<string, Button>();

            foreach (string buttonName in buttonNames)
            {
                GameObject buttonObject = Instantiate(ButtonPrefab) as GameObject;
                buttonObject.transform.SetParent(buttonGroup.transform);
                buttonObject.GetComponentInChildren<Text>().text = buttonName;
                //buttonObject.transform.rotation = buttonGroup.transform.rotation;
                buttonObject.transform.localRotation = buttonGroup.transform.localRotation;
                buttonObject.transform.localScale = buttonGroup.transform.localScale;

                Button button = buttonObject.GetComponent<Button>();
                button.name = buttonName;
                button.onClick.AddListener(() => { HandleButtonPress(button); });
                allButtons.Add(buttonName, button);
            }
        }

        private void SetHeaderText()
        {
            GameObject header = GameObject.FindGameObjectWithTag(Tags.HEADER);
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

            float x = Screen.width / 2 - MenuAttributes.Width / 2;
            float y = Screen.height / 2 - menuHeight / 2;
            float width = MenuAttributes.Width;
            float height = menuHeight;
            GUI.BeginGroup(new Rect(x, y, width, height));

            // Background box
            x = 0;
            y = 0;
            GUI.Box(new Rect(x, y, width, height), string.Empty);

            // Header image
            width = MenuAttributes.HeaderWidth;
            height = MenuAttributes.HeaderHeight;
            x = MenuAttributes.Width / 2 - width / 2;
            y = MenuAttributes.Padding;
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

            x = MenuAttributes.Width / 2 - MenuAttributes.ButtonWidth / 2;
            //y = 2 * MenuAttributes.padding + headerImage.height;
            y += MenuAttributes.TextHeight + MenuAttributes.Padding;
            width = MenuAttributes.ButtonWidth;
            height = MenuAttributes.ButtonHeight;

            for (int i = 0; i < buttonNames.Length; i++)
            {
                string buttonName = buttonNames[i];
                bool buttonPressed = GUI.Button(new Rect(x, y, width, height), buttonName);

                y += MenuAttributes.ButtonHeight + MenuAttributes.Padding;

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
            float paddingHeight = 2 * MenuAttributes.Padding;
            if (buttonNames != null)
            {
                buttonHeight = buttonNames.Length * MenuAttributes.ButtonHeight;
                paddingHeight += buttonNames.Length * MenuAttributes.Padding;
            }

            float messageHeight = MenuAttributes.TextHeight + MenuAttributes.Padding;

            float menuHeight = MenuAttributes.HeaderHeight + buttonHeight + paddingHeight + messageHeight;
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
                case EXIT:
                    ExitGame();
                    break;
            }
        }

        protected virtual void Pause()
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            IsOpeningMenu = true;
        }

        protected virtual void Resume()
        {
            Time.timeScale = 1f;
            //Cursor.visible = false;
            IsOpeningMenu = false;
            //GameManager.ActiveInstance.IsMenuOpen = false;
        }

        protected virtual void Back()
        {
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

        public virtual void ShowMenu()
        {
            this.IsOpeningMenu = true;
        }

        public virtual void HideMenu()
        {
            this.IsOpeningMenu = false;
        }

        protected virtual void ExitGame()
        {
            string username = "Unknown player";
            Profile profile = ProfileManager.ActiveInstance.CurrentProfile;
            if (profile != null)
            {
                username = profile.Username;
            }

            Debug.Log(string.Format("{0} has left the game", username));
            Application.Quit();
        }
    }
}