using UnityEngine;

public class Hud : MonoBehaviour
{
    public static Hud singleton;

    public GUISkin resourceSkin, ordersSkin, selectionBoxSkin, playerDetailsSkin;

    public const int ORDERS_BAR_WIDTH = 150;
    public const int RESOURCE_BAR_HEIGHT = 40;
    protected const int SELECTION_NAME_HEIGHT = 20;
    protected const int ICON_WIDTH = 32, ICON_HEIGHT = 32;
    protected const int TEXT_WIDTH = 128;
    protected const int TEXT_HEIGHT = 32;
    protected const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
    protected const int BUILD_IMAGE_PADDING = 8;
    protected const int BUTTON_PADDING = 7;
    protected const int SCROLL_BAR_WIDTH = 22;

    //public Texture2D[] resourceIcons;
    //protected Dictionary<ResourceType, Texture2D> allResources;
    //protected Dictionary<ResourceType, int> resourceValues, resourceLimits;

    protected Squad previousSelection = null;
    protected float sliderValue;
    public Texture2D buttonHover, buttonClick;
    protected int buildAreaHeight;
    public Texture2D buildFrame, buildMask;
    public Texture2D smallButtonHover, smallButtonClick;

    public Texture2D healthyTexture, damagedTexture, criticalTexture;
    public Texture2D[] resourceHealthBars;

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

    public void Start()
    {
        this.buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_PADDING;
    }

    public void OnGUI()
    {
        if (Player.singleton == null)
        {
            Debug.Log(string.Format("Player is null"));
            return;
        }
        if (Player.singleton.isNPC == true)
        {
            //Debug.Log(string.Format("Non-Human player"));
            return;
        }

        DrawMouseCursor();
    }
    #endregion Unity

    #region Draw HUD
    protected void DrawMouseCursor()
    {
        bool drawCustomCursor = CursorManager.singleton.IsMouseInPlayArea();
        if (CursorManager.singleton.activeCursorState == CursorManager.CursorState.panRight)
        {
            drawCustomCursor = true;
        }
        if (CursorManager.singleton.activeCursorState == CursorManager.CursorState.panUp)
        {
            drawCustomCursor = true;
        }
        //drawCustomCursor = drawCustomCursor && MouseOnScreen();

        if (drawCustomCursor == false)
        {
            Cursor.visible = true;
            return;
        }

        Cursor.visible = false;
        GUI.skin = CursorManager.singleton.mouseCursorSkin;

        int x = 0;
        int y = 0;
        int width = Screen.width;
        int height = Screen.height;
        GUI.BeginGroup(new Rect(x, y, width, height));
        CursorManager.singleton.UpdateCursorAnimation();
        Rect cursorPosition = CursorManager.singleton.GetCursorDrawPosition();
        GUI.Label(cursorPosition, CursorManager.singleton.activeCursor);
        GUI.EndGroup();
    }

    #endregion DrawHUD
}