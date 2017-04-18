using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Strikeforce
{
    public enum ActionKey { Action1, Action2, Special1, Special2, LeftTrigger, RightTrigger, DUp, DDown, DLeft, DRight, Menu, Back, LeftStick, RightStick }

    public struct MouseControls
    {
        public const string X_AXIS = "Mouse X";
        public const string Y_AXIS = "Mouse Y";
        public const string SCROLL_WHEEL = "Mouse ScrollWheel";
    }

    public struct Direction
    {
        public const string LEFT = "Left";
        public const string RIGHT = "Right";
    }

    public class UserInput : NetworkBehaviour
    {
        public Profile profile;
        protected KeyMap gamepadBinds;
        protected KeyMap keyboardBinds;
        public float KeyHoldDuration = 1f;
        public float KeyDoubleTapDelay = 0.5f;
        protected Dictionary<ActionKey, KeyEvent> incompleteKeyEvents { get; set; }
        protected Queue<KeyEvent> completeKeyEvents { get; set; }
        protected bool rightTriggerDown = false;
        protected bool leftTriggerDown = false;
        public const string TRIGGER = "Trigger";
        public struct KeyMap
        {
            public KeyCode Action1;
            public KeyCode Action2;
            public KeyCode Special1;
            public KeyCode Special2;
            public KeyCode LeftTrigger;
            public KeyCode RightTrigger;
            public KeyCode Menu;
            public KeyCode Back;
            public KeyCode DUp;
            public KeyCode DDown;
            public KeyCode DLeft;
            public KeyCode DRight;
            public AxisBind LeftStick;
            public AxisBind RightStick;
        }
        public struct AxisBind
        {
            public KeyCode Up;
            public KeyCode Down;
            public KeyCode Left;
            public KeyCode Right;
        }
        public struct Axis
        {
            public const string HORIZONTAL = "Horizontal";
            public const string VERTICAL = "Vertical";
        }

        protected void Awake()
        {
            incompleteKeyEvents = new SortedList<float, KeyEvent>();
            completeKeyEvents = new Queue<KeyEvent>();

            InitKeyBinds();
        }

        protected void InitKeyBinds()
        {
            InitKeyboardBinds();
            InitGamepadBinds();
        }

        protected void InitKeyboardBinds()
        {
            keyboardBinds = new KeyMap();
            keyboardBinds.Action1 = KeyCode.K;
            keyboardBinds.Action2 = KeyCode.L;
            keyboardBinds.Special1 = KeyCode.I;
            keyboardBinds.Special2 = KeyCode.O;
            keyboardBinds.LeftTrigger = KeyCode.J;
            keyboardBinds.RightTrigger = KeyCode.Semicolon;
            keyboardBinds.Menu = KeyCode.Escape;
            keyboardBinds.Back = KeyCode.Backspace;

            keyboardBinds.LeftStick.Up = KeyCode.W;
            keyboardBinds.LeftStick.Down = KeyCode.S;
            keyboardBinds.LeftStick.Left = KeyCode.A;
            keyboardBinds.LeftStick.Right = KeyCode.D;

            keyboardBinds.RightStick.Up = KeyCode.UpArrow;
            keyboardBinds.RightStick.Down = KeyCode.DownArrow;
            keyboardBinds.RightStick.Left = KeyCode.LeftArrow;
            keyboardBinds.RightStick.Right = KeyCode.RightArrow;
        }

        protected void InitGamepadBinds()
        {
            gamepadBinds = new KeyMap();
            gamepadBinds.Action1 = KeyCode.Joystick1Button0;
            gamepadBinds.Action2 = KeyCode.Joystick1Button1;
            gamepadBinds.Special1 = KeyCode.Joystick1Button2;
            gamepadBinds.Special2 = KeyCode.Joystick1Button3;
            gamepadBinds.LeftTrigger = KeyCode.Joystick1Button9;
            gamepadBinds.RightTrigger = KeyCode.Joystick1Button10;
            gamepadBinds.DUp = KeyCode.
            gamepadBinds.DDown = KeyCode.
            gamepadBinds.DLeft = KeyCode.
            gamepadBinds.DRight = KeyCode.
            gamepadBinds.Menu = KeyCode.Joystick1Button7;
            gamepadBinds.Back = KeyCode.Joystick1Button6;

            //keyBinds.LeftStick.Up = KeyCode.UpArrow;
            //keyBinds.LeftStick.Down = KeyCode.DownArrow;
            //keyBinds.LeftStick.Left = KeyCode.LeftArrow;
            //keyBinds.LeftStick.Right = KeyCode.RightArrow;

            //keyBinds.RightStick.Up = KeyCode.Keypad8;
            //keyBinds.RightStick.Down = KeyCode.Keypad2;
            //keyBinds.RightStick.Left = KeyCode.Keypad4;
            //keyBinds.RightStick.Right = KeyCode.Keypad6;
        }

        protected void Start()
        {
            profile = ProfileManager.Singleton.CurrentProfile;
            if (profile == null)
            {
                return;
            }

            Debug.Log(string.Format("User: {0}", profile.Username));
        }

        //public override void OnStartLocalPlayer()
        //{
        //    GetComponent<MeshRenderer>().material.color = Color.red;
        //}

        protected void Update()
        {
            //if (isLocalPlayer == false)
            //{
            //    return;
            //}

            LeftStick();
            RightStick();

            LeftTrigger();
            RightTrigger();

            CheckForPressedKeys();
            CheckForReleasedKeys();
            HandleKeyEvents();
        }

        protected float getHorizontalScrollRate(float x)
        {
            float scrollRate = 0;
            int boundaryWidth = GlobalAssets.Camera.ScrollArea;
            int screenWidth = Screen.width;

            int leftBoundary = boundaryWidth;
            int rightBoundary = screenWidth - boundaryWidth;

            if (x >= 0 && x < leftBoundary)
            {
                scrollRate = -GlobalAssets.Camera.ScrollSpeed;
                //player.PlayerHud.SetCursorState(CursorState.panLeft);
            }
            else if (x <= screenWidth && x > rightBoundary)
            {
                scrollRate = GlobalAssets.Camera.ScrollSpeed;
                //player.PlayerHud.SetCursorState(CursorState.panRight);
            }

            return scrollRate;
        }

        protected float getVerticalScrollRate(float y)
        {
            float scrollRate = 0;
            int boundaryWidth = GlobalAssets.Camera.ScrollArea;
            int screenHeight = Screen.height;

            int bottomBoundary = boundaryWidth;
            int topBoundary = screenHeight - boundaryWidth;

            if (y >= 0 && y < bottomBoundary)
            {
                scrollRate = -GlobalAssets.Camera.ScrollSpeed;
                //player.PlayerHud.SetCursorState(CursorState.panDown);
            }
            else if (y <= screenHeight && y > topBoundary)
            {
                scrollRate = GlobalAssets.Camera.ScrollSpeed;
                //player.PlayerHud.SetCursorState(CursorState.panUp);
            }

            return scrollRate;
        }

        public static GameObject GetHitGameObject()
        {
            Vector3 mousePosition = Input.mousePosition;
            return GetHitGameObject(mousePosition);
        }

        public static GameObject GetHitGameObject(Vector3 origin)
        {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            RaycastHit hit;
            bool foundEntity = Physics.Raycast(ray, out hit);
            if (foundEntity == false)
            {
                return null;
            }

            GameObject gameObject = hit.collider.gameObject;
            return gameObject;
        }

        public static Vector3 GetHitPoint()
        {
            Vector3 mousePosition = Input.mousePosition;
            return GetHitPoint(mousePosition);
        }

        public static Vector3 GetHitPoint(Vector3 origin)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool foundPoint = Physics.Raycast(ray, out hit);
            if (foundPoint == false)
            {
                return GlobalAssets.InvalidPoint;
            }

            return hit.point;
        }

        protected void SelectEntity(Selectable entityToSelect)
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            player.SelectedEntity = entityToSelect;
            entityToSelect.SetSelection(true);
        }

        protected void Deselect()
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            player.SelectedEntity.SetSelection(false);
            player.SelectedEntity = null;
        }

        protected void LeftStick()
        {
            string leftStickHor = string.Format("{0} {1}", Direction.LEFT, Axis.HORIZONTAL);
            string leftStickVert = string.Format("{0} {1}", Direction.LEFT, Axis.VERTICAL);

            float x = Input.GetAxis(leftStickHor) * 0.1f;
            float z = Input.GetAxis(leftStickVert) * 0.1f;
            if (x == 0f)
            {
                if (z == 0f)
                {
                    return;
                }
            }

            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            player.LeftStick(x, 0, z);
        }

        protected void RightStick()
        {
            string rightStickHor = string.Format("{0} {1}", Direction.RIGHT, Axis.HORIZONTAL);
            string rightStickVert = string.Format("{0} {1}", Direction.RIGHT, Axis.VERTICAL);

            float x = Input.GetAxis(rightStickHor) * 0.1f;
            float z = Input.GetAxis(rightStickVert) * 0.1f;

            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            player.RightStick(x, 0, z);
        }

        protected void LeftTrigger()
        {
            KeyEvent keyEvent = null;

			string leftTrigger = string.Format ("{0} {1}", Direction.LEFT, TRIGGER);
            int trigger = (int)Input.GetAxis(leftTrigger);
            if (trigger == 0)
            {
                if (leftTriggerDown == false)
                {
                    return;
                }

                leftTriggerDown = false;
                keyEvent = GetKeyEvent(ActionKey.LeftTrigger, false);
                completeKeyEvents.Enqueue(keyEvent);
                return;
            }

            if (leftTriggerDown == true)
            {
                return;
            }

            leftTriggerDown = true;
            keyEvent = GetKeyEvent(ActionKey.LeftTrigger, true);
            completeKeyEvents.Enqueue(keyEvent);
        }

        protected void RightTrigger()
        {
            KeyEvent keyEvent = null;

			string rightTrigger = string.Format ("{0} {1}", Direction.RIGHT, TRIGGER);
            int trigger = (int)Input.GetAxis(rightTrigger);
            if (trigger == 0)
            {
                if (rightTriggerDown == false)
                {
                    return;
                }

                rightTriggerDown = false;
                keyEvent = GetKeyEvent(ActionKey.RightTrigger, false);
                completeKeyEvents.Enqueue(keyEvent);
                return;
            }

            if (rightTriggerDown == true)
            {
                return;
            }

            rightTriggerDown = true;
            keyEvent = GetKeyEvent(ActionKey.RightTrigger, true);
            completeKeyEvents.Enqueue(keyEvent);
        }

        protected void CheckForPressedKeys()
        {
            //if (isLocalPlayer == false)
            //{
            //    return;
            //}

            if (Input.anyKeyDown == false)
            {
                return;
            }

            KeyEvent keyEvent = null;

            if (Input.GetKeyDown(gamepadBinds.Action1) || Input.GetKeyDown(keyboardBinds.Action1))
            {
                AddKeyEvent(ActionKey.Action1);
            }

            if (Input.GetKeyDown(keyboardBinds.RightTrigger))
            {
                AddKeyEvent(ActionKey.RightTrigger);
            }

            if (Input.GetKeyDown(gamepadBinds.Menu) || Input.GetKeyDown(keyboardBinds.Menu))
            {
                AddKeyEvent(ActionKey.Menu);
            }
        }

        protected void CheckForReleasedKeys()
        {
            KeyEvent keyEvent = null;

            if (Input.GetKeyUp(gamepadBinds.Action1) || Input.GetKeyUp(keyboardBinds.Action1))
            {
                keyEvent = GetKeyEvent(ActionKey.Action1, false);
                completeKeyEvents.Enqueue(keyEvent);
            }

            if (Input.GetKeyUp(keyboardBinds.RightTrigger))
            {
                keyEvent = GetKeyEvent(ActionKey.RightTrigger, false);
                completeKeyEvents.Enqueue(keyEvent);
            }

            if (Input.GetKeyUp(gamepadBinds.Menu) || Input.GetKeyUp(keyboardBinds.Menu))
            {
                keyEvent = GetKeyEvent(ActionKey.Menu, false);
                OpenPauseMenu();
            }
        }

        // KeyDown
        protected void AddKeyEvent(ActionKey key)
        {
            KeyEvent keyEvent = new KeyEvent(key, KeyEvent.Type.Pressed, Time.time);
            incompleteKeyEvents.Add(key, keyEvent);
        }

        // KeyUp
        protected KeyEvent GetKeyEvent(ActionKey key, bool keyDownEvent)
        {
            KeyEvent keyEvent;

            if(incompleteKeyEvents.ContainsKey(key) == false)
            {
                keyEvent = new KeyEvent(key, KeyEvent.Type.Pressed, Time.time);
            } else
            {
                keyEvent = incompleteKeyEvents[key];
            }

            keyEvent.Release(Time.time);

            return keyEvent;
        }

        protected void HandleKeyEvents()
        {
            while (completeKeyEvents.Count > 0)
            {
                KeyEvent keyEvent = completeKeyEvents.Dequeue();

                CheckForDoubleTap(ref keyEvent);

                PlayerHandleKeyEvent(keyEvent);
            }
        }

        protected void CheckForDoubleTap(ref KeyEvent current)
        {
            KeyEvent next = completeKeyEvents.Peek();

            if(next.Key != current.Key)
            {
                return;
            }

            float delay = next.PressedTime - current.PressedTime;
            if(delay > KeyDoubleTapDelay)
            {
                return;
            }

            current.DoubleTap();
            completeKeyEvents.Dequeue();
        }

        protected void PlayerHandleKeyEvent(KeyEvent keyEvent)
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            player.RespondToKeyEvent(keyEvent);
        }

        protected void OpenPauseMenu()
        {
            Time.timeScale = 0f;
            GetComponentInChildren<PauseMenu>().enabled = true;
            //GetComponent<UserInput>().enabled = false;
            Cursor.visible = true;
            profile.Player.IsMenuOpen = true;
        }
    }
}