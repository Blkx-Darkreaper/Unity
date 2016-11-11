using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Strikeforce
{
    public class UserInput : NetworkBehaviour
    {
        protected Player player;
        public Profile profile;

        protected void Start()
        {
            Debug.Log(string.Format("User: {0}", profile.Username));
        }

        protected void Update()
        {
            if (player == null)
            {
                return;
            }

            if (player.IsNPC == true)
            {
                return;
            }
            if (isLocalPlayer == false)
            {
                return;
            }

            RespondToKeyboardActivity();
            RespondToMouseActivity();
            UpdateCamera();
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
                destination.y = Mathf.Clamp(destination.y, GlobalAssets.Camera.minHeight, GlobalAssets.Camera.maxHeight),
                destination.z
            );

            if (destination == origin)
            {
                player.PlayerHud.SetCursorState(CursorState.select);
                return;
            }

            Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * GlobalAssets.Camera.scrollSpeed);
        }

        protected float getHorizontalScrollRate(float x)
        {
            float scrollRate = 0;
            int boundaryWidth = GlobalAssets.Camera.scrollArea;
            int screenWidth = Screen.width;

            int leftBoundary = boundaryWidth;
            int rightBoundary = screenWidth - boundaryWidth;

            if (x >= 0 && x < leftBoundary)
            {
                scrollRate = -GlobalAssets.Camera.scrollSpeed;
                player.PlayerHud.SetCursorState(CursorState.panLeft);
            }
            else if (x <= screenWidth && x > rightBoundary)
            {
                scrollRate = GlobalAssets.Camera.scrollSpeed;
                player.PlayerHud.SetCursorState(CursorState.panRight);
            }

            return scrollRate;
        }

        protected float getVerticalScrollRate(float y)
        {
            float scrollRate = 0;
            int boundaryWidth = GlobalAssets.Camera.scrollArea;
            int screenHeight = Screen.height;

            int bottomBoundary = boundaryWidth;
            int topBoundary = screenHeight - boundaryWidth;

            if (y >= 0 && y < bottomBoundary)
            {
                scrollRate = -GlobalAssets.Camera.scrollSpeed;
                player.PlayerHud.SetCursorState(CursorState.panDown);
            }
            else if (y <= screenHeight && y > topBoundary)
            {
                scrollRate = GlobalAssets.Camera.scrollSpeed;
                player.PlayerHud.SetCursorState(CursorState.panUp);
            }

            return scrollRate;
        }

        protected float getCameraZoomRate()
        {
            // Away from ground is positive
            float scrollwheelInput = Input.GetAxis(KeyMappings.SCROLL_WHEEL);
            float zoomRate = -GlobalAssets.Camera.scrollSpeed * scrollwheelInput;

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

            float rotationAmount = GlobalAssets.Camera.rotationAmount;

            float horizontalInput = Input.GetAxis(KeyMappings.MOUSE_Y_AXIS);
            destination.x -= horizontalInput * rotationAmount;

            float verticalInput = Input.GetAxis(KeyMappings.MOUSE_X_AXIS);
            destination.y += verticalInput * rotationAmount;

            if (destination == origin)
            {
                return;
            }

            Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * GlobalAssets.Camera.rotationSpeed);
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
            player.SelectedEntity = entityToSelect;
            entityToSelect.SetSelection(true);
        }

        protected void RightMouseClick()
        {
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
            player.SelectedEntity.SetSelection(false);
            player.SelectedEntity = null;
        }

        protected void RespondToKeyboardActivity()
        {
            if (isLocalPlayer == false)
            {
                return;
            }

            var x = Input.GetAxis(Axis.HORIZONTAL) * 0.1f;
            var z = Input.GetAxis(Axis.VERTICAL) * 0.1f;

            transform.Translate(x, 0, z);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenPauseMenu();
            }
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