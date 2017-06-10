using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Color = UnityEngine.Color;

namespace Strikeforce
{
    public class Hud : NetworkBehaviour
    {
        protected bool areStylesSet = false;
        public GUIStyle HealthStyle;
        public GUIStyle BackStyle;
        public GUISkin SelectionBoxSkin, PlayerDetailsSkin;
        [HideInInspector] public GUISkin MouseCursorSkin;
        private const int SELECTION_NAME_HEIGHT = 20;
        private const int ICON_WIDTH = 32, ICON_HEIGHT = 32;
        private const int TEXT_WIDTH = 128;
        private const int TEXT_HEIGHT = 32;
        private const int BUTTON_PADDING = 7;
        private const int SCROLL_BAR_WIDTH = 22;
        protected Profile profile;
        [HideInInspector] public Texture2D ActiveCursor;
        private CursorState activeCursorState;
        private int currentFrame = 0;
        public CursorState PreviousCursorState { get; private set; }
        public Rectangle Bounds = new Rectangle();

        public Texture2D[] ResourceIcons;
        private Dictionary<ResourceType, Texture2D> allResources;
        private Selectable previousSelection = null;
        private float sliderValue;
        public Texture2D ButtonHover, ButtonClick;
        public Texture2D BuildFrame, BuildMask;
        public Texture2D SmallButtonHover, SmallButtonClick;
        protected struct Styles
        {
            public const string label = "label";
        }

        protected void Awake()
        {
            enabled = false;
        }

        protected void Start()
        {
            profile = ProfileManager.Singleton.CurrentProfile;

            SetCursorState(CursorState.select);
            InitResources();
        }

        protected void InitStyles()
        {
            if (areStylesSet == true)
            {
                return;
            }

            this.HealthStyle = new GUIStyle(GUI.skin.box);
            this.HealthStyle.normal.background = GlobalAssets.MakeTexture(2, 2, new Color(0f, 1f, 0f, 1.0f));

            this.BackStyle = new GUIStyle(GUI.skin.box);
            this.BackStyle.normal.background = GlobalAssets.MakeTexture(2, 2, new Color(0f, 0f, 0f, 1.0f));

            this.areStylesSet = true;
        }

        protected void InitResources()
        {
            allResources = new Dictionary<ResourceType, Texture2D>();
            foreach (Texture2D resource in ResourceIcons)
            {
                string resourceName = resource.name;
                switch (resourceName)
                {
                    case ResourceProperties.MONEY:
                        allResources.Add(ResourceType.Money, resource);
                        break;

                    case ResourceProperties.FUEL:
                        allResources.Add(ResourceType.Fuel, resource);
                        break;

                    case ResourceProperties.ROCKETS:
                        allResources.Add(ResourceType.Rockets, resource);
                        break;

                    case ResourceProperties.MISSILES:
                        allResources.Add(ResourceType.Missiles, resource);
                        break;

                    case ResourceProperties.BOMBS:
                        allResources.Add(ResourceType.Bombs, resource);
                        break;

                    case ResourceProperties.MATERIEL:
                        allResources.Add(ResourceType.Materiel, resource);
                        break;

                    case ResourceProperties.UNKNOWN:
                        break;

                    default:
                        break;
                }
            }
        }

        protected void OnGUI()
        {
            if (isLocalPlayer == false)
            {
                return;
            }

            if (profile.Player == null)
            {
                Debug.Log(string.Format("Player is null"));
                return;
            }

            if (profile.Player.IsNPC == true)
            {
                //Debug.Log(string.Format("Non-Human player"));
                return;
            }

            InitStyles();
            //DrawPlayerDetails();
            //DrawHealthBar();
            //DrawMouseCursor();
        }

        protected void DrawBuildHud()
        {

        }

        protected void DrawRaidHud()
        {

        }

        protected void DrawPlayerDetails()
        {
            GUI.skin = PlayerDetailsSkin;

            float x = 0;
            float y = 0;
            float width = Screen.width;
            float height = Screen.height;
            GUI.BeginGroup(new Rect(x, y, width, height));

            height = Menu.Attributes.TextHeight;
            x = Menu.Attributes.TextHeight;
            y = Screen.height - x - Menu.Attributes.Padding;

            x = DrawPlayerAvatar(x, y, height);

            float minWidth = 0, maxWidth = 0;

            string username = profile.Username;
            //string currentPlayerUsername = GameManager.activeInstance.currentPlayerAccount.username;
            //if (currentPlayerUsername.Equals(username) == false)
            //{
            //    Debug.Log(string.Format("Usernames do not match"));
            //}

            PlayerDetailsSkin.GetStyle(Styles.label).CalcMinMaxWidth(new GUIContent(username), out minWidth, out maxWidth);
            GUI.Label(new Rect(x, y, maxWidth, height), username);

            GUI.EndGroup();
        }

        protected void DrawHealthBar()
        {
            Raider raider = profile.Player.CurrentRaider;
            if (raider == null)
            {
                return;
            }

            //Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 pos = new Vector3(50, Screen.height);

            // draw health bar background
            GUI.color = Color.grey;
            GUI.backgroundColor = Color.grey;
            GUI.Box(new Rect(pos.x - 26, Screen.height - pos.y + 20, raider.MaxHitPoints / 2, 7), ".", BackStyle);

            // draw health bar amount
            GUI.color = Color.green;
            GUI.backgroundColor = Color.green;
            GUI.Box(new Rect(pos.x - 25, Screen.height - pos.y + 21, raider.CurrentHitPoints / 2, 5), ".", HealthStyle);
        }

        protected float DrawPlayerAvatar(float x, float y, float height)
        {
            int avatarId = profile.AvatarId;
            Texture2D avatar = GlobalAssets.GetAvatar(avatarId);
            if (avatar == null)
            {
                return x;
            }

            GUI.DrawTexture(new Rect(x, y, height, height), avatar);

            x += height + Menu.Attributes.Padding;
            return x;
        }

        protected void DrawMouseCursor()
        {
            bool drawCustomCursor = MouseInPlayArea();
            if (activeCursorState == CursorState.panRight)
            {
                drawCustomCursor = true;
            }
            if (activeCursorState == CursorState.panUp)
            {
                drawCustomCursor = true;
            }
            //drawCustomCursor = drawCustomCursor && MouseOnScreen();
            if (profile.Player.IsSettingConstructionPoint == true)
            {
                Cursor.visible = false;
                return;
            }

            if (drawCustomCursor == false)
            {
                Cursor.visible = true;
                return;
            }

            Cursor.visible = false;
            GUI.skin = MouseCursorSkin;

            int x = 0;
            int y = 0;
            int width = Screen.width;
            int height = Screen.height;
            GUI.BeginGroup(new Rect(x, y, width, height));
            UpdateCursorAnimation();
            Rect cursorPosition = GetCursorDrawPosition();
            GUI.Label(cursorPosition, ActiveCursor);
            GUI.EndGroup();
        }

        public void SetCursorState(CursorState state)
        {
            if (activeCursorState != state)
            {
                PreviousCursorState = activeCursorState;
            }

            activeCursorState = state;
            UpdateCursorAnimation();
        }

        protected void UpdateCursorAnimation()
        {
            switch (activeCursorState)
            {
                case CursorState.select:
                    break;

                case CursorState.move:
                    break;

                case CursorState.attack:
                    break;

                case CursorState.harvest:
                    break;

                case CursorState.panLeft:
                    break;

                case CursorState.panRight:
                    break;

                case CursorState.panUp:
                    break;

                case CursorState.panDown:
                    break;

                case CursorState.rallyPoint:
                    break;
            }
        }

        protected void SetActiveCursor(ref Texture2D[] cursorAnimation)
        {
            currentFrame = (int)Time.time % cursorAnimation.Length;
            ActiveCursor = cursorAnimation[currentFrame];
        }

        protected Rect GetCursorDrawPosition()
        {
            // Set base position for custom cursor image
            float leftEdge = Input.mousePosition.x;
            float topEdge = Screen.height - Input.mousePosition.y;  // screen draw coordinates are inverted
            float width = ActiveCursor.width;
            float height = ActiveCursor.height;

            switch (activeCursorState)
            {
                case CursorState.panRight:
                    leftEdge = Screen.width - width;
                    break;

                case CursorState.panDown:
                    topEdge = Screen.height - height;
                    break;

                case CursorState.move:
                case CursorState.select:
                case CursorState.harvest:
                    topEdge -= height / 2;
                    leftEdge -= width / 2;
                    break;

                case CursorState.rallyPoint:
                    topEdge -= ActiveCursor.height;
                    break;
            }

            Rect cursorBox = new Rect(leftEdge, topEdge, width, height);
            return cursorBox;
        }

        protected void DrawAvailableActions()
        {
            if (profile.Player == null)
            {
                return;
            }
            if (profile.Player.SelectedEntity == null)
            {
                return;
            }

            bool ownedByPlayer = profile.Player.SelectedEntity.IsOwnedByPlayer(profile.Player);
            if (ownedByPlayer == false)
            {
                return;
            }

            ResetSlider();
            string[] allActions = profile.Player.SelectedEntity.Actions;
            previousSelection = profile.Player.SelectedEntity;
        }

        protected void ResetSlider()
        {
            if (previousSelection == null)
            {
                return;
            }
            if (previousSelection == profile.Player.SelectedEntity)
            {
                return;
            }

            sliderValue = 0f;
        }

        protected Rect GetScrollArea(int groupHeight)
        {
            int height = groupHeight - 2 * BUTTON_PADDING;
            Rect scrollArea = new Rect(BUTTON_PADDING, BUTTON_PADDING, SCROLL_BAR_WIDTH, height);
            return scrollArea;
        }

        protected void PauseMenu()
        {
            Time.timeScale = 0f;

            MenuManager menuManager = GetComponent<MenuManager>();
            if (menuManager == null)
            {
                return;
            }

            PauseMenu pauseMenu = GetComponent<PauseMenu>();
            if (pauseMenu == null)
            {
                return;
            }

            menuManager.ShowMenu(pauseMenu);
        }

        protected void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topEdge)
        {
            Texture2D icon = allResources[type];
            int value = profile.Player.CurrentInventory.GetResourceAmount(type);
            int maxValue = profile.Player.CurrentInventory.GetMaxResourceAmount(type);
            string text = string.Format("{0} / {1}", value, maxValue);
            GUI.DrawTexture(new Rect(iconLeft, topEdge, ICON_WIDTH, ICON_HEIGHT), icon);
            GUI.Label(new Rect(textLeft, topEdge, TEXT_WIDTH, TEXT_HEIGHT), text);
        }

        public static Rect GetPlayingArea()
        {
            int x = 0;
            int y = 0;
            int width = Screen.width;
            int height = Screen.height;

            Rect playingArea = new Rect(x, y, width, height);
            return playingArea;
        }

        public bool MouseInPlayArea()
        {
            Vector3 position = Input.mousePosition;

            bool xInBounds = (position.x >= 0) && (position.x <= (Screen.width));
            bool yInBounds = (position.y >= 0) && (position.y <= (Screen.height));

            return xInBounds && yInBounds;
        }

        public bool MouseOnScreen()
        {
            Vector3 position = Input.mousePosition;

            bool xInBounds = (position.x >= 0) && (position.x <= Screen.width);
            bool yInBounds = (position.y >= 0) && (position.y <= Screen.height);

            return xInBounds && yInBounds;
        }
    }
}