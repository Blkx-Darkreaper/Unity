using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class UserInput : NetworkBehaviour
    {
        public Profile profile;
        protected KeyMap gamepadBinds;
        protected KeyMap keyboardBinds;
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
        public struct Side
        {
            public const string LEFT = "Left";
            public const string RIGHT = "Right";
        }

        protected void Awake()
        {
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
            keyboardBinds.Action1 = KeyCode.A;
            keyboardBinds.Action2 = KeyCode.S;
            keyboardBinds.Special1 = KeyCode.D;
            keyboardBinds.Special2 = KeyCode.F;
            keyboardBinds.LeftTrigger = KeyCode.E;
            keyboardBinds.RightTrigger = KeyCode.R;
            keyboardBinds.Menu = KeyCode.Escape;
            keyboardBinds.Back = KeyCode.Backspace;

            keyboardBinds.LeftStick.Up = KeyCode.UpArrow;
            keyboardBinds.LeftStick.Down = KeyCode.DownArrow;
            keyboardBinds.LeftStick.Left = KeyCode.LeftArrow;
            keyboardBinds.LeftStick.Right = KeyCode.RightArrow;

            keyboardBinds.RightStick.Up = KeyCode.Keypad8;
            keyboardBinds.RightStick.Down = KeyCode.Keypad2;
            keyboardBinds.RightStick.Left = KeyCode.Keypad4;
            keyboardBinds.RightStick.Right = KeyCode.Keypad6;
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

            RespondToKeyActivity();
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

        protected void RespondToKeyActivity()
        {
            //if (isLocalPlayer == false)
            //{
            //    return;
            //}

            MovePlayer();

            if (Input.GetKeyDown(gamepadBinds.Menu))
            {
                OpenPauseMenu();
            }

            if (Input.GetKeyDown(gamepadBinds.Action1))
            {
                // Command function is called from the client, but invoked on the server
                CmdFire();
            }
        }

        protected void MovePlayer()
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }
            Raider raider = player.CurrentRaider;
            if (raider == null)
            {
                return;
            }

			string leftStickHor = string.Format ("{0} {1}", Side.LEFT, Axis.HORIZONTAL);
			string leftStickVert = string.Format ("{0} {1}", Side.LEFT, Axis.VERTICAL);

            float x = Input.GetAxis(leftStickHor) * 0.1f;
            float z = Input.GetAxis(leftStickVert) * 0.1f;

            profile.Player.MovePlayer(x, 0, z);
        }

        [Command]
        void CmdFire()
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }
            Raider raider = player.CurrentRaider;
            if (raider == null)
            {
                return;
            }

            raider.FirePrimary();  // Testing
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