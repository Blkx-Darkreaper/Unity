﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Strikeforce
{
    public enum ActionKey { Action1, Action2, Special1, Special2, LeftTrigger, RightTrigger, DUp, DDown, DLeft, DRight, Menu, Back, LeftStick, RightStick }

    public struct MouseControls
    {
        public const string MOUSE = "Mouse";
        public const string SCROLL_WHEEL = "ScrollWheel";
        //public const string X_AXIS = "Mouse X";
        //public const string Y_AXIS = "Mouse Y";
        //public const string SCROLL_WHEEL = "Mouse ScrollWheel";
    }

    public class UserInput : NetworkBehaviour
    {
        public Profile profile;
        protected KeyMap gamepadBinds;
        protected KeyMap keyboardBinds;
        public float MinKeyHoldDuration = 1f;
        public float KeyDoubleTapDelay = 0.5f;
        protected Dictionary<ActionKey, KeyEvent> incompleteKeyEvents { get; set; }
        protected Queue<KeyEvent> allKeyEvents { get; set; }
        protected KeyEvent previousKeyEvent { get; set; }
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
            public AxisBind DVert;
            public AxisBind DHor;
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
            public const string DIRECTION_PAD = "Dpad";
        }

        protected void Awake()
        {
            incompleteKeyEvents = new Dictionary<ActionKey, KeyEvent>();
            allKeyEvents = new Queue<KeyEvent>();

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
            keyboardBinds.DUp = KeyCode.Keypad8;
            keyboardBinds.DDown = KeyCode.Keypad2;
            keyboardBinds.DLeft = KeyCode.Keypad4;
            keyboardBinds.DRight = KeyCode.Keypad6;
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
            gamepadBinds.Action1 = KeyCode.Joystick1Button0;    // A
            gamepadBinds.Action2 = KeyCode.Joystick1Button1;    // B
            gamepadBinds.Special1 = KeyCode.Joystick1Button2;   // X
            gamepadBinds.Special2 = KeyCode.Joystick1Button3;   // Y
            gamepadBinds.LeftTrigger = KeyCode.Joystick1Button9;    // LT
            gamepadBinds.RightTrigger = KeyCode.Joystick1Button10;  // RT
            gamepadBinds.Menu = KeyCode.Joystick1Button7;   // Start
            gamepadBinds.Back = KeyCode.Joystick1Button6;   // Back
        }

        protected void Start()
        {
            profile = ProfileManager.singleton.CurrentProfile;
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
            LeftStick();
            RightStick();

            DirectionPad();

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

        public static Vector3 GetHitLocation()
        {
            Vector3 mousePosition = Input.mousePosition;
            return GetHitLocation(mousePosition);
        }

        public static Vector3 GetHitLocation(Vector3 origin)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool foundPoint = Physics.Raycast(ray, out hit);
            if (foundPoint == false)
            {
                return GlobalAssets.InvalidLocation;
            }

            return hit.point;
        }

        protected void SelectEntity(Selectable entityToSelect)
        {
            Player player = profile.player;
            if (player == null)
            {
                return;
            }

            player.buildMode.selectedEntity = entityToSelect;
            entityToSelect.SetSelection(true);
        }

        protected void Deselect()
        {
            Player player = profile.player;
            if (player == null)
            {
                return;
            }

            player.buildMode.selectedEntity.SetSelection(false);
            player.buildMode.selectedEntity = null;
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

            if (MenuManager.Singleton.IsMenuOpen == true)
            {
                HandleMenuSelection(x, z);
                return;
            }

            Player player = profile.player;
            if (player == null)
            {
                return;
            }

            player.LeftStick(x, 0, z);
        }

        protected void HandleMenuSelection(float x, float z)
        {
            Menu currentMenu = MenuManager.Singleton.CurrentMenu;

            string direction;
            if (x == 0)
            {
                if (z == 0)
                {
                    return;
                }
            }

            direction = Direction.RIGHT;
            if (x < 0)
            {
                direction = Direction.LEFT;
            }
            if (z > 0)
            {
                direction = Direction.UP;
            }
            if (z < 0)
            {
                direction = Direction.DOWN;
            }

            currentMenu.HandleMenuSelection(direction);
        }

        protected void RightStick()
        {
            string rightStickHor = string.Format("{0} {1}", Direction.RIGHT, Axis.HORIZONTAL);
            string rightStickVert = string.Format("{0} {1}", Direction.RIGHT, Axis.VERTICAL);

            float x = Input.GetAxis(rightStickHor) * 0.1f;
            float y = Input.GetAxis(rightStickVert) * 0.1f;

            Player player = profile.player;
            if (player == null)
            {
                return;
            }

            player.RightStick(x, 0, y);
        }

        protected void DirectionPad()
        {
            string dpadX = string.Format("{0} {1}", Axis.DIRECTION_PAD, Axis.HORIZONTAL);
            string dpadY = string.Format("{0} {1}", Axis.DIRECTION_PAD, Axis.VERTICAL);

            float x = Input.GetAxis(dpadX);
            float y = Input.GetAxis(dpadY);

            Player player = profile.player;
            if (player == null)
            {
                return;
            }

            player.DPad(x, 0, y);
        }

        protected void LeftTrigger()
        {
            KeyEvent keyEvent;

            string leftTrigger = string.Format("{0} {1}", Direction.LEFT, TRIGGER);
            int trigger = (int)Input.GetAxis(leftTrigger);
            if (trigger == 0)
            {
                if (leftTriggerDown == false)
                {
                    return;
                }

                leftTriggerDown = false;
                keyEvent = KeyUpEvent(ActionKey.LeftTrigger);
                allKeyEvents.Enqueue(keyEvent);
                return;
            }

            if (leftTriggerDown == true)
            {
                return;
            }

            leftTriggerDown = true;
            keyEvent = KeyDownEvent(ActionKey.LeftTrigger);
            allKeyEvents.Enqueue(keyEvent);
        }

        protected void RightTrigger()
        {
            KeyEvent keyEvent;

            string rightTrigger = string.Format("{0} {1}", Direction.RIGHT, TRIGGER);
            int trigger = (int)Input.GetAxis(rightTrigger);
            if (trigger == 0)
            {
                if (rightTriggerDown == false)
                {
                    return;
                }

                rightTriggerDown = false;
                keyEvent = KeyUpEvent(ActionKey.RightTrigger);
                allKeyEvents.Enqueue(keyEvent);
                return;
            }

            if (rightTriggerDown == true)
            {
                return;
            }

            rightTriggerDown = true;
            keyEvent = KeyDownEvent(ActionKey.RightTrigger);
            allKeyEvents.Enqueue(keyEvent);
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

            CheckAction1KeyPressed();
            CheckRightTriggerKeyPressed();
            CheckMenuKeyPressed();
        }

        protected void CheckMenuKeyPressed()
        {
            if (Input.GetKeyDown(gamepadBinds.Menu) == false)
            {
                if(Input.GetKeyDown(keyboardBinds.Menu) == false)
                {
                    return;
                }
            }

            HandleMenuButtonToggled();
        }

        protected void CheckRightTriggerKeyPressed()
        {
            if (Input.GetKeyDown(keyboardBinds.RightTrigger) == false)
            {
                return;
            }

            KeyEvent keyEvent = KeyDownEvent(ActionKey.RightTrigger);
            allKeyEvents.Enqueue(keyEvent);
        }

        protected void CheckAction1KeyPressed()
        {
            if (Input.GetKeyDown(gamepadBinds.Action1) == false)
            {
                if(Input.GetKeyDown(keyboardBinds.Action1) == false)
                {
                    return;
                }
            }

            KeyEvent keyEvent = KeyDownEvent(ActionKey.Action1);
                allKeyEvents.Enqueue(keyEvent);
        }

        protected void HandleMenuButtonToggled()
        {
            // If not in match go back
            if(profile.player == null)
            {
                Menu currentMenu = MenuManager.Singleton.CurrentMenu;
                MenuManager.Singleton.ShowMenu(currentMenu.PreviousMenu);
                return;
            }

            // If in match toggle game menu
            if (MenuManager.Singleton.IsMenuOpen == true)
            {
                MenuManager.Singleton.Resume();
            }
            else
            {
                MenuManager.Singleton.Pause();
            }
        }

        protected void CheckForReleasedKeys()
        {
            CheckForAction1KeyReleased();
            CheckForRightTriggerReleased();
            CheckForMenuKeyReleased();
        }

        protected void CheckForMenuKeyReleased()
        {
            if (Input.GetKeyUp(gamepadBinds.Menu) == false)
            {
                if (Input.GetKeyUp(keyboardBinds.Menu) == false)
                {
                    return;
                }
            }

            HandleMenuButtonToggled();
        }

        protected void CheckForRightTriggerReleased()
        {
            if (Input.GetKeyUp(keyboardBinds.RightTrigger) == false)
            {
                return;
            }

            KeyEvent keyEvent = KeyUpEvent(ActionKey.RightTrigger);
            allKeyEvents.Enqueue(keyEvent);
        }

        protected void CheckForAction1KeyReleased()
        {
            if (Input.GetKeyUp(gamepadBinds.Action1) == false)
            {
                if (Input.GetKeyUp(keyboardBinds.Action1) == false)
                {
                    return;
                }
            }

            KeyEvent keyEvent = KeyUpEvent(ActionKey.Action1);
            allKeyEvents.Enqueue(keyEvent);
        }

        protected KeyEvent KeyDownEvent(ActionKey key)
        {
            if (incompleteKeyEvents.ContainsKey(key) == true)
            {
                KeyUpEvent(key);
                Debug.Log(string.Format("Duplicate keydown events"));
            }

            KeyEvent keyEvent = new KeyEvent(key, KeyEvent.Type.Pressed, Time.time);
            incompleteKeyEvents.Add(key, keyEvent);

            return keyEvent;
        }

        protected KeyEvent KeyUpEvent(ActionKey key)
        {
            KeyEvent keyEvent;

            if (incompleteKeyEvents.ContainsKey(key) == false)
            {
                keyEvent = new KeyEvent(key, KeyEvent.Type.Pressed, Time.time);
            }
            else
            {
                keyEvent = incompleteKeyEvents[key];
                incompleteKeyEvents.Remove(key);
            }

            keyEvent.Release(Time.time, MinKeyHoldDuration);

            return keyEvent;
        }

        protected void HandleKeyEvents()
        {
            while (allKeyEvents.Count > 0)
            {
                KeyEvent keyEvent = allKeyEvents.Dequeue();

                CheckForDoubleTap(ref keyEvent);

                PlayerHandleKeyEvent(keyEvent);

                if (keyEvent.IsComplete == false)
                {
                    return;
                }

                previousKeyEvent = keyEvent;
            }
        }

        protected void CheckForDoubleTap(ref KeyEvent current)
        {
            if (previousKeyEvent == null)
            {
                return;
            }

            if (previousKeyEvent.Key != current.Key)
            {
                return;
            }

            float delay = current.PressedTime - previousKeyEvent.PressedTime;
            if (delay > KeyDoubleTapDelay)
            {
                return;
            }

            current.DoubleTap(previousKeyEvent.PressedTime, (float)previousKeyEvent.ReleasedTime);
            //Debug.Log(string.Format("{0} key was double-tapped", current.Key));
        }

        protected void PlayerHandleKeyEvent(KeyEvent keyEvent)
        {
            Player player = profile.player;
            if (player == null)
            {
                return;
            }

            player.RespondToKeyEvent(keyEvent);
        }
    }
}