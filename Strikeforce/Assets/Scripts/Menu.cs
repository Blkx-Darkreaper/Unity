using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class Menu : MonoBehaviour
    {
        public GUISkin MenuSkin;
        public Texture2D HeaderImage;
        public bool IsOpen { get { return animator.GetBool("IsOpen"); } set { animator.SetBool("IsOpen", value); } }
        protected Animator animator;
        protected CanvasGroup canvasGroup;
        protected Player player;
        protected string[] buttons;
        protected const string EXIT = "Quit Game";

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();

            RectTransform rect = GetComponent<RectTransform>();
            rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
        }

        protected virtual void Start()
        {
            player = transform.root.GetComponent<Player>();
            SetButtons();
            SetUsername();
        }

        protected void Update()
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
            {
                canvasGroup.blocksRaycasts = canvasGroup.interactable = false;
                return;
            }

            canvasGroup.blocksRaycasts = canvasGroup.interactable = true;

            HandleKeyboardActivity();
        }

        protected virtual void OnGUI()
        {
            //DrawMenu();
        }

        protected virtual void SetButtons()
        {
            buttons = new string[] { };
        }

        protected virtual void SetUsername()
        {
            if (GameManager.ActiveInstance == null)
            {
                return;
            }
            if (GameManager.ActiveInstance.currentPlayerAccount == null)
            {
                return;
            }

            string username = GameManager.ActiveInstance.currentPlayerAccount.Username;
            SetUsername(username);
        }

        protected virtual void SetUsername(string username)
        {
            if (player == null)
            {
                return;
            }
            bool alreadySet = !player.Username.Equals(string.Empty);
            if (alreadySet == true)
            {
                return;
            }

            player.Username = username;
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
            width = MenuAttributes.Width - 2 * MenuAttributes.Padding;
            height = MenuAttributes.TextHeight;
            x = MenuAttributes.Width / 2 - width / 2;
            y = 2 * MenuAttributes.Padding + HeaderImage.height;
            string welcomeMessage = string.Format("Welcome {0}", GameManager.ActiveInstance.currentPlayerAccount.Username);
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

            x = MenuAttributes.Width / 2 - MenuAttributes.ButtonWidth / 2;
            //y = 2 * MenuAttributes.padding + headerImage.height;
            y += MenuAttributes.TextHeight + MenuAttributes.Padding;
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

                HandleButtonPress(buttonName);
            }

            GUI.EndGroup();
        }

        protected virtual float GetMenuHeight()
        {
            float buttonHeight = 0;
            float paddingHeight = 2 * MenuAttributes.Padding;
            if (buttons != null)
            {
                buttonHeight = buttons.Length * MenuAttributes.ButtonHeight;
                paddingHeight += buttons.Length * MenuAttributes.Padding;
            }

            float messageHeight = MenuAttributes.TextHeight + MenuAttributes.Padding;

            float menuHeight = MenuAttributes.HeaderHeight + buttonHeight + paddingHeight + messageHeight;
            return menuHeight;
        }

        protected virtual float GetMenuItemsHeight()
        {
            return 0f;
        }

        protected virtual void HideCurrentMenu()
        {
            GetComponent<Menu>().enabled = false;
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
            GameManager.ActiveInstance.IsMenuOpen = false;
        }

        protected virtual void Back()
        {
        }

        protected virtual void ExitGame()
        {
            string playerName = "Unknown player";
            if (player != null)
            {
                playerName = player.Username;
            }

            Debug.Log(string.Format("{0} has left the game", playerName));
            Application.Quit();
        }
    }
}