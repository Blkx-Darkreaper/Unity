using UnityEngine;

public class UserInput : MonoBehaviour
{
    public struct CameraProperties
    {
        public static float scrollSpeed = 25f;
        public static int scrollArea = 15;
        public static float minHeight = 10;
        public static float maxHeight = 40;
        public static float rotationAmount = 10f;
        public static float rotationSpeed = 100f;
    }

    public struct KeyMappings
    {
        public const string mouseXAxis = "Mouse X";
        public const string mouseYAxis = "Mouse Y";
        public const string scrollWheel = "Mouse ScrollWheel";
    }

    #region Unity
    public void Update()
    {
        if (Player.singleton.isNPC == true)
        {
            return;
        }

        UpdateCamera();
        RespondToMouseActivity();
        RespondToKeyboardActivity();
    }
    #endregion Unity

    #region Camera
    protected void UpdateCamera()
    {
        //MoveCamera();
        //RotateCamera();
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

        float horizontalScrollRate = GetHorizontalScrollRate(x);
        float verticalScrollRate = GetVerticalScrollRate(y);
        //if (Mathf.Abs(horizontalScrollRate) + Mathf.Abs(verticalScrollRate) == 0)
        //{
        //    player.hud.SetCursorState(CursorState.select);
        //}

        Vector3 movement = new Vector3(horizontalScrollRate, 0, verticalScrollRate);

        // Scroll the camera in the direction it is pointing but ignore the vertical tilt
        movement = Camera.main.transform.TransformDirection(movement);

        // Change camera zoom
        movement.y = GetCameraZoomRate();

        Vector3 origin = Camera.main.transform.position;

        Vector3 destination = origin;
        destination.x += movement.x;
        destination.y += movement.y;
        destination.z += movement.z;

        destination = new Vector3(
            destination.x,
            destination.y = Mathf.Clamp(destination.y, CameraProperties.minHeight, CameraProperties.maxHeight),
            destination.z
        );

        if (destination == origin)
        {
            CursorManager.singleton.SetCursorState(CursorManager.CursorState.select);
            return;
        }

        Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * CameraProperties.scrollSpeed);
    }

    #region Camera Movement
    protected float GetHorizontalScrollRate(float x)
    {
        float scrollRate = 0;
        int boundaryWidth = CameraProperties.scrollArea;
        int screenWidth = Screen.width;

        int leftBoundary = boundaryWidth;
        int rightBoundary = screenWidth - boundaryWidth;

        if (x >= 0 && x < leftBoundary)
        {
            scrollRate = -CameraProperties.scrollSpeed;
            CursorManager.singleton.SetCursorState(CursorManager.CursorState.panLeft);
        }
        else if (x <= screenWidth && x > rightBoundary)
        {
            scrollRate = CameraProperties.scrollSpeed;
            CursorManager.singleton.SetCursorState(CursorManager.CursorState.panRight);
        }

        return scrollRate;
    }

    protected float GetVerticalScrollRate(float y)
    {
        float scrollRate = 0;
        int boundaryWidth = CameraProperties.scrollArea;
        int screenHeight = Screen.height;

        int bottomBoundary = boundaryWidth;
        int topBoundary = screenHeight - boundaryWidth;

        if (y >= 0 && y < bottomBoundary)
        {
            scrollRate = -CameraProperties.scrollSpeed;
            CursorManager.singleton.SetCursorState(CursorManager.CursorState.panDown);
        }
        else if (y <= screenHeight && y > topBoundary)
        {
            scrollRate = CameraProperties.scrollSpeed;
            CursorManager.singleton.SetCursorState(CursorManager.CursorState.panUp);
        }

        return scrollRate;
    }

    protected float GetCameraZoomRate()
    {
        // Away from ground is positive
        float scrollwheelInput = Input.GetAxis(KeyMappings.scrollWheel);
        float zoomRate = -CameraProperties.scrollSpeed * scrollwheelInput;

        return zoomRate;
    }

    #endregion Camera Movement
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

        float rotationAmount = CameraProperties.rotationAmount;

        float horizontalInput = Input.GetAxis(KeyMappings.mouseYAxis);
        destination.x -= horizontalInput * rotationAmount;

        float verticalInput = Input.GetAxis(KeyMappings.mouseXAxis);
        destination.y += verticalInput * rotationAmount;

        if (destination == origin)
        {
            return;
        }

        Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * CameraProperties.rotationSpeed);
    }

    #endregion Camera

    #region Mouse
    protected void RespondToMouseActivity()
    {
        if (Input.GetMouseButton(0) == true)
        {
            Player.singleton.LeftMouseClick();
        }
        else if (Input.GetMouseButton(1) == true)
        {
            Player.singleton.RightMouseClick();
        }

        Player.singleton.MouseOver();
    }

    #endregion Mouse

    #region Keyboard
    private void RespondToKeyboardActivity()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
        }
    }

    #endregion Keyboard
}