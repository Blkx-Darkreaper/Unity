using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class UserInput : NetworkBehaviour
    {
        public Profile profile;
        protected KeyMap keyBinds;
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

        protected void Awake()
        {
            InitKeyBinds();
        }

        protected void InitKeyBinds()
        {
            keyBinds = new KeyMap();
            keyBinds.Action1 = KeyCode.A;
            keyBinds.Action2 = KeyCode.S;
            keyBinds.Special1 = KeyCode.D;
            keyBinds.Special2 = KeyCode.F;
            keyBinds.LeftTrigger =KeyCode.E;
            keyBinds.RightTrigger = KeyCode.R;
            keyBinds.Menu = KeyCode.Escape;
            keyBinds.Back = KeyCode.Backspace;

            keyBinds.LeftStick.Up = KeyCode.UpArrow;
            keyBinds.LeftStick.Down = KeyCode.DownArrow;
            keyBinds.LeftStick.Left = KeyCode.LeftArrow;
            keyBinds.LeftStick.Right = KeyCode.RightArrow;

            keyBinds.RightStick.Up = KeyCode.Keypad8;
            keyBinds.RightStick.Down = KeyCode.Keypad2;
            keyBinds.RightStick.Left = KeyCode.Keypad4;
            keyBinds.RightStick.Right = KeyCode.Keypad6;
        }

        protected void Start()
        {
            profile = ProfileManager.ActiveInstance.CurrentProfile;
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

            RespondToKeyboardActivity();
            //RespondToMouseActivity();
            //UpdateCamera();
        }

        protected void UpdateCamera()
        {
            MoveCamera();
            RotateCamera();
        }

        protected void MoveCamera()
        {
            bool altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (altKeyPressed == true)
            {
                return;
            }

            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;

            float horizontalScrollRate = getHorizontalScrollRate(x);
            float verticalScrollRate = getVerticalScrollRate(y);
            //if (Mathf.Abs(horizontalScrollRate) + Mathf.Abs(verticalScrollRate) == 0)
            //{
            //    player.hud.SetCursorState(CursorState.select);
            //}

            Vector3 movement = new Vector3(horizontalScrollRate, 0, verticalScrollRate);

            // Scroll the camera in the direction it is pointing but ignore the vertical tilt
            movement = Camera.main.transform.TransformDirection(movement);

            // Change camera zoom
            movement.y = getCameraZoomRate();

            Vector3 origin = Camera.main.transform.position;

            Vector3 destination = origin;
            destination.x += movement.x;
            destination.y += movement.y;
            destination.z += movement.z;

            destination = new Vector3(
                destination.x,
                destination.y = Mathf.Clamp(destination.y, GlobalAssets.Camera.MinHeight, GlobalAssets.Camera.MaxHeight),
                destination.z
            );

            if (destination == origin)
            {
                //player.PlayerHud.SetCursorState(CursorState.select);
                return;
            }

            Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * GlobalAssets.Camera.ScrollSpeed);
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

        protected float getCameraZoomRate()
        {
            // Away from ground is positive
            float scrollwheelInput = Input.GetAxis(KeyMappings.SCROLL_WHEEL);
            float zoomRate = -GlobalAssets.Camera.ScrollSpeed * scrollwheelInput;

            return zoomRate;
        }

        protected void RotateCamera()
        {
            Vector3 origin = Camera.main.transform.eulerAngles;
            Vector3 destination = origin;

            // Detect rotation amount if Alt and the right mouse button are held down
            bool altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (altKeyPressed == false)
            {
                return;
            }

            bool rightMousePressed = Input.GetMouseButton(1);
            if (rightMousePressed == false)
            {
                return;
            }

            float rotationAmount = GlobalAssets.Camera.RotationAmount;

            float horizontalInput = Input.GetAxis(KeyMappings.MOUSE_Y_AXIS);
            destination.x -= horizontalInput * rotationAmount;

            float verticalInput = Input.GetAxis(KeyMappings.MOUSE_X_AXIS);
            destination.y += verticalInput * rotationAmount;

            if (destination == origin)
            {
                return;
            }

            Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * GlobalAssets.Camera.RotationSpeed);
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

        protected void RespondToMouseActivity()
        {
            if (Input.GetMouseButton(0) == true)
            {
                LeftMouseClick();
            }
            else if (Input.GetMouseButton(1) == true)
            {
                RightMouseClick();
            }

            MouseOver();
        }

        protected void MouseOver()
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            bool mouseInBounds = player.PlayerHud.MouseInPlayArea();
            if (mouseInBounds == false)
            {
                return;
            }

            //if (player.IsSettingConstructionPoint == true)
            //{
            //    player.SetConstructionPoint();
            //    return;
            //}

            GameObject entityUnderMouse = GetHitGameObject();
            if (entityUnderMouse == null)
            {
                return;
            }

            if (player.SelectedEntity != null)
            {
                player.SelectedEntity.SetHoverState(entityUnderMouse);
            }
            else
            {
                bool entityIsGround = entityUnderMouse.CompareTag(Tags.GROUND);
                if (entityIsGround == true)
                {
                    return;
                }

                Player owner = entityUnderMouse.GetComponentInParent<Player>();
                if (owner == null)
                {
                    return;
                }

                Selectable unit = null;
                if (entityUnderMouse.CompareTag(Tags.UNIT) == true)
                {
                    unit = entityUnderMouse.GetComponentInParent<Selectable>();
                }

                Structure structure = null;
                if (entityUnderMouse.CompareTag(Tags.STRUCTURE) == true)
                {
                    structure = entityUnderMouse.GetComponentInParent<Structure>();
                }
                bool entityNeitherUnitOrStructure = unit == null && structure == null;
                if (entityNeitherUnitOrStructure == true)
                {
                    return;
                }

                bool playerOwnsEntity = owner.PlayerId == player.PlayerId;
                if (playerOwnsEntity == false)
                {
                    return;
                }

                player.PlayerHud.SetCursorState(CursorState.select);
            }
        }

        protected void LeftMouseClick()
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            bool mouseInBounds = player.PlayerHud.MouseInPlayArea();
            if (mouseInBounds == false)
            {
                return;
            }

            //bool settingBuildPoint = player.IsSettingConstructionPoint;
            //if (settingBuildPoint == true)
            //{
            //    bool validBuildPoint = player.IsConstructionSiteValid();
            //    if (validBuildPoint == false)
            //    {
            //        return;
            //    }

            //    player.StartConstruction();
            //    return;
            //}

            GameObject hitEntity = GetHitGameObject();
            if (hitEntity == null)
            {
                return;
            }

            Vector3 hitPoint = GetHitPoint();
            if (hitPoint == GlobalAssets.InvalidPoint)
            {
                return;
            }

            if (player.SelectedEntity != null)
            {
                player.SelectedEntity.MouseClick(hitEntity, hitPoint, player);
            }
            else
            {
                bool isGround = hitEntity.CompareTag(Tags.GROUND);
                if (isGround == true)
                {
                    return;
                }

                Selectable entity = hitEntity.GetComponentInParent<Selectable>();
                if (entity == null)
                {
                    return;
                }

                SelectEntity(entity);
            }
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

        protected void RightMouseClick()
        {
            Player player = profile.Player;
            if (player == null)
            {
                return;
            }

            bool mouseInBounds = player.PlayerHud.MouseInPlayArea();
            if (mouseInBounds == false)
            {
                return;
            }

            bool altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (altKeyPressed == true)
            {
                return;
            }
            if (player.SelectedEntity == null)
            {
                return;
            }

            bool settingConstructionSite = player.IsSettingConstructionPoint;
            if (settingConstructionSite == true)
            {
                //player.CancelConstruction();
            }
            else
            {
                Deselect();
            }
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

        protected void RespondToKeyboardActivity()
        {
            //if (isLocalPlayer == false)
            //{
            //    return;
            //}

            MovePlayer();

            if (Input.GetKeyDown(keyBinds.Menu))
            {
                OpenPauseMenu();
            }

            if (Input.GetKeyDown(keyBinds.Action1))
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

            float x = Input.GetAxis(Axis.HORIZONTAL) * 0.1f;
            float z = Input.GetAxis(Axis.VERTICAL) * 0.1f;

            raider.transform.Translate(x, 0, z);    //testing
            //profile.Player.MovePlayer(x, 0, z);
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

            // create the bullet object from the bullet prefab
            GameObject bullet = (GameObject)Instantiate(
                NetworkManager.singleton.spawnPrefabs[0],
                raider.transform.position + raider.transform.forward,
                Quaternion.identity);

            // make the bullet move away in front of the player
            bullet.GetComponentInChildren<Rigidbody>().velocity = raider.transform.forward * 4;

            // spawn the bullet on the clients
            NetworkServer.Spawn(bullet);

            // make bullet disappear after 2 seconds
            Destroy(bullet, 2.0f);
        }

        protected void OpenPauseMenu()
        {
            Time.timeScale = 0f;
            GetComponentInChildren<PauseMenu>().enabled = true;
            GetComponent<UserInput>().enabled = false;
            Cursor.visible = true;
            GameManager.ActiveInstance.IsMenuOpen = true;
        }
    }
}