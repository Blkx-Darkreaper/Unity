using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager singleton;
    public static Vector3 invalidPoint = new Vector3(-99999f, -99999f, -99999f);

    [ReadOnlyInInspector]
    public GUISkin mouseCursorSkin;
    public enum CursorState
    {
        select, move, attack, panLeft, panRight, panUp, panDown, harvest, rallyPoint
    }

    [ReadOnlyInInspector]
    public Texture2D activeCursor;
    public Texture2D selectCursor, panLeftCursor, panRightCursor, panUpCursor, panDownCursor, rallyPointCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    [ReadOnlyInInspector]
    public CursorState activeCursorState;
    protected int currentFrame = 0;
    [ReadOnlyInInspector]
    public CursorState previousCursorState;

    #region Unity
    public void Awake()
    {
        if (singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
        }
        if (singleton != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion Unity

    public void SetCursorState(CursorState state)
    {
        if (activeCursorState != state)
        {
            this.previousCursorState = activeCursorState;
        }

        this.activeCursorState = state;
        UpdateCursorAnimation();
    }

    protected void SetActiveCursor(ref Texture2D[] cursorAnimation)
    {
        this.currentFrame = (int)Time.time % cursorAnimation.Length;
        this.activeCursor = cursorAnimation[currentFrame];
    }

    public void UpdateCursorAnimation()
    {
        switch (activeCursorState)
        {
            case CursorState.select:
                activeCursor = selectCursor;
                break;

            case CursorState.move:
                SetActiveCursor(ref moveCursors);
                break;

            case CursorState.attack:
                SetActiveCursor(ref attackCursors);
                break;

            case CursorState.harvest:
                SetActiveCursor(ref harvestCursors);
                break;

            case CursorState.panLeft:
                activeCursor = panLeftCursor;
                break;

            case CursorState.panRight:
                activeCursor = panRightCursor;
                break;

            case CursorState.panUp:
                activeCursor = panUpCursor;
                break;

            case CursorState.panDown:
                activeCursor = panDownCursor;
                break;

            case CursorState.rallyPoint:
                activeCursor = rallyPointCursor;
                break;
        }
    }

    public Rect GetCursorDrawPosition()
    {
        // Set base position for custom cursor image
        float leftEdge = Input.mousePosition.x;
        float topEdge = Screen.height - Input.mousePosition.y;  // screen draw coordinates are inverted
        float width = activeCursor.width;
        float height = activeCursor.height;

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
                topEdge -= activeCursor.height;
                break;
        }

        Rect cursorBox = new Rect(leftEdge, topEdge, width, height);
        return cursorBox;
    }

    public bool IsMouseInPlayArea()
    {
        Vector3 position = Input.mousePosition;

        bool xInBounds = (position.x >= 0) && (position.x <= (Screen.width - Hud.ORDERS_BAR_WIDTH));
        bool yInBounds = (position.y >= 0) && (position.y <= (Screen.height - Hud.RESOURCE_BAR_HEIGHT));

        return xInBounds && yInBounds;
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
            return CursorManager.invalidPoint;
        }

        return hit.point;
    }
}