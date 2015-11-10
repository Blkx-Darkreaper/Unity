using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        Debug.Log(string.Format("Player: {0}", player.username));
    }

    private void Update()
    {
        UpdateCamera();
        RespondToMouseActivity();
    }

    private void UpdateCamera()
    {
        if (player.isNPC == true)
        {
            return;
        }

        MoveCamera();
        RotateCamera();
    }

    private void MoveCamera()
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
            Mathf.Clamp(destination.y, ResourceManager.Camera.minHeight, ResourceManager.Camera.maxHeight),
            destination.z
        );

        if (destination == origin)
        {
            player.hud.SetCursorState(CursorState.select);
            return;
        }

        Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.Camera.scrollSpeed);
    }

    private float getHorizontalScrollRate(float x)
    {
        float scrollRate = 0;
        int boundaryWidth = ResourceManager.Camera.scrollArea;
        int screenWidth = Screen.width;

        int leftBoundary = boundaryWidth;
        int rightBoundary = screenWidth - boundaryWidth;

        if (x >= 0 && x < leftBoundary)
        {
            scrollRate = -ResourceManager.Camera.scrollSpeed;
            player.hud.SetCursorState(CursorState.panLeft);
        }
        else if (x <= screenWidth && x > rightBoundary)
        {
            scrollRate = ResourceManager.Camera.scrollSpeed;
            player.hud.SetCursorState(CursorState.panRight);
        }

        return scrollRate;
    }

    private float getVerticalScrollRate(float y)
    {
        float scrollRate = 0;
        int boundaryWidth = ResourceManager.Camera.scrollArea;
        int screenHeight = Screen.height;

        int bottomBoundary = boundaryWidth;
        int topBoundary = screenHeight - boundaryWidth;

        if (y >= 0 && y < bottomBoundary)
        {
            scrollRate = -ResourceManager.Camera.scrollSpeed;
            player.hud.SetCursorState(CursorState.panDown);
        }
        else if (y <= screenHeight && y > topBoundary)
        {
            scrollRate = ResourceManager.Camera.scrollSpeed;
            player.hud.SetCursorState(CursorState.panUp);
        }

        return scrollRate;
    }

    private float getCameraZoomRate()
    {
        // Away from ground is positive
        float scrollwheelInput = Input.GetAxis(KeyMappings.scrollWheel);
        float zoomRate = -ResourceManager.Camera.scrollSpeed * scrollwheelInput;

        return zoomRate;
    }

    private void RotateCamera()
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

        float rotationAmount = ResourceManager.Camera.rotationAmount;

        float horizontalInput = Input.GetAxis(KeyMappings.mouseYAxis);
        destination.x -= horizontalInput * rotationAmount;

        float verticalInput = Input.GetAxis(KeyMappings.mouseXAxis);
        destination.y += verticalInput * rotationAmount;

        if (destination == origin)
        {
            return;
        }

        Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.Camera.rotationSpeed);
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
            return ResourceManager.invalidPoint;
        }

        return hit.point;
    }

    private void RespondToMouseActivity()
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

    private void MouseOver()
    {
        bool mouseInBounds = player.hud.MouseInPlayArea();
        if (mouseInBounds == false)
        {
            return;
        }

        if (player.isSettingConstructionPoint == true)
        {
            player.SetConstructionPoint();
            return;
        }

        GameObject entityUnderMouse = GetHitGameObject();
        if (entityUnderMouse == null)
        {
            return;
        }

        if (player.selectedEntity != null)
        {
            player.selectedEntity.SetOverState(entityUnderMouse);
        }
        else
        {
            bool entityIsGround = entityUnderMouse.CompareTag(Tags.ground);
            if (entityIsGround == true)
            {
                return;
            }

            PlayerController owner = entityUnderMouse.GetComponentInParent<PlayerController>();
            if (owner == null)
            {
                return;
            }

            UnitController unit = null;
            if(entityUnderMouse.CompareTag(Tags.unit) == true) {
                unit = entityUnderMouse.GetComponentInParent<UnitController>();
            }

            StructureController structure = null;
            if(entityUnderMouse.CompareTag(Tags.structure) == true) {
                structure = entityUnderMouse.GetComponentInParent<StructureController>();
            }
            bool entityNeitherUnitOrStructure = unit == null && structure == null;
            if (entityNeitherUnitOrStructure == true)
            {
                return;
            }

            bool playerOwnsEntity = owner.username == player.username;
            if (playerOwnsEntity == false)
            {
                return;
            }

            player.hud.SetCursorState(CursorState.select);
        }
    }

    private void LeftMouseClick()
    {
        bool mouseInBounds = player.hud.MouseInPlayArea();
        if (mouseInBounds == false)
        {
            return;
        }

        bool settingBuildPoint = player.isSettingConstructionPoint;
        if (settingBuildPoint == true)
        {
            bool validBuildPoint = player.CheckConstructionSiteIsValid();
            if (validBuildPoint == false)
            {
                return;
            }

            player.StartConstruction();
            return;
        }

        GameObject hitEntity = GetHitGameObject();
        if (hitEntity == null)
        {
            return;
        }

        Vector3 hitPoint = GetHitPoint();
        if (hitPoint == ResourceManager.invalidPoint)
        {
            return;
        }

        if (player.selectedEntity != null)
        {
            player.selectedEntity.MouseClick(hitEntity, hitPoint, player);
        }
        else
        {
            bool isGround = hitEntity.CompareTag(Tags.ground);
            if (isGround == true)
            {
                return;
            }

            EntityController entity = hitEntity.GetComponentInParent<EntityController>();
            if (entity == null)
            {
                return;
            }

            SelectEntity(entity);
        }
    }

    private void SelectEntity(EntityController entityToSelect)
    {
        player.selectedEntity = entityToSelect;
        entityToSelect.SetSelection(true);
    }

    private void RightMouseClick()
    {
        bool mouseInBounds = player.hud.MouseInPlayArea();
        if (mouseInBounds == false)
        {
            return;
        }

        bool altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (altKeyPressed == true)
        {
            return;
        }
        if (player.selectedEntity == null)
        {
            return;
        }

        bool settingConstructionSite = player.isSettingConstructionPoint;
        if (settingConstructionSite == true)
        {
            player.CancelConstruction();
        }
        else
        {
            Deselect();
        }
    }

    private void Deselect()
    {
        player.selectedEntity.SetSelection(false);
        player.selectedEntity = null;
    }
}
