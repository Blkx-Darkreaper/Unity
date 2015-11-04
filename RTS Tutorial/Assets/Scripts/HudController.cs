using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class HudController : MonoBehaviour {

    public GUISkin resourceSkin, ordersSkin, selectBoxSkin;
    [HideInInspector]
    public GUISkin mouseCursorSkin;
    private const int ORDERS_BAR_WIDTH = 150;
    private const int RESOURCE_BAR_HEIGHT = 40;
    private const int SELECTION_NAME_HEIGHT = 20;
    private const int ICON_WIDTH = 32, ICON_HEIGHT = 32;
    private const int TEXT_WIDTH = 128;
    private const int TEXT_HEIGHT = 32;
    private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
    private const int BUILD_IMAGE_PADDING = 8;
    private const int BUTTON_SPACING = 7;
    private const int SCROLL_BAR_WIDTH = 22;

    private PlayerController player;

    [HideInInspector]
    public Texture2D activeCursor;
    public Texture2D selectCursor, panLeftCursor, panRightCursor, panUpCursor, panDownCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    private CursorState activeCursorState;
    private int currentFrame = 0;

    public Texture2D[] resourceIcons;
    private Dictionary<ResourceType, Texture2D> allResources;
    private Dictionary<ResourceType, int> resourceValues, resourceLimits;

    private EntityController previousSelection = null;
    private float sliderValue;
    public Texture2D buttonHover, buttonClick;
    private int buildAreaHeight;
    public Texture2D buildFrame, buildMask;


    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits)
    {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
        ResourceManager.selectBoxSkin = this.selectBoxSkin;
        SetCursorState(CursorState.select);
        resourceValues = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();
        InitResources();
        buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
    }

    private void InitResources()
    {
        allResources = new Dictionary<ResourceType, Texture2D>();
        foreach (Texture2D resource in resourceIcons)
        {
            string resourceName = resource.name;
            switch (resourceName)
            {
                case "Money":
                    allResources.Add(ResourceType.money, resource);
                    resourceValues.Add(ResourceType.money, 0);
                    resourceLimits.Add(ResourceType.money, 0);
                    break;

                case "Power":
                    allResources.Add(ResourceType.power, resource);
                    resourceValues.Add(ResourceType.power, 0);
                    resourceLimits.Add(ResourceType.power, 0);
                    break;
            }
        }
    }

    private void OnGUI()
    {
        if (player == null)
        {
            Debug.Log(string.Format("Player is null"));
            return;
        }
        if (player.isNPC == true)
        {
            Debug.Log(string.Format("Non-Human player"));
            return;
        }

        DrawOrdersBar();
        DrawResourceBar();
        DrawMouseCursor();
    }

    private void DrawMouseCursor()
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

        if (drawCustomCursor == false)
        {
            Cursor.visible = true;
            return;
        }

        Cursor.visible = false;
        GUI.skin = mouseCursorSkin;

        int x = 0;
        int y = 0;
        int width = Screen.width;
        int height = Screen.height;
        GUI.BeginGroup(new Rect(x, y, width, height));
        UpdateCursorAnimation();
        Rect cursorPosition = GetCursorDrawPosition();
        GUI.Label(cursorPosition, activeCursor);
        GUI.EndGroup();
    }

    public void SetCursorState(CursorState state)
    {
        activeCursorState = state;
        UpdateCursorAnimation();
    }

    private void UpdateCursorAnimation()
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
        }
    }

    private void SetActiveCursor(ref Texture2D[] cursorAnimation)
    {
        currentFrame = (int)Time.time % cursorAnimation.Length;
        activeCursor = cursorAnimation[currentFrame];
    }

    private Rect GetCursorDrawPosition()
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
        }

        Rect cursorBox = new Rect(leftEdge, topEdge, width, height);
        return cursorBox;
    }

    private void DrawOrdersBar()
    {
        GUI.skin = ordersSkin;

        int x = Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH;
        int y = RESOURCE_BAR_HEIGHT;
        int width = ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH;
        int height = Screen.height - RESOURCE_BAR_HEIGHT;
        GUI.BeginGroup(new Rect(x, y, width, height));

        x = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH;
        y = 0;
        width = ORDERS_BAR_WIDTH;
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        DisplayAvailableActions();
        DisplayBuildQueue();
        DisplaySelectedNameInOrdersBar();

        GUI.EndGroup();
    }

    private void DisplaySelectedNameInOrdersBar()
    {
        if (player.selectedEntity == null)
        {
            return;
        }

        string selectionName = player.selectedEntity.entityName;
        if (selectionName.Equals(string.Empty))
        {
            Debug.Log("Selected entity has no name");
            return;
        }

        int x = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + 2 * BUILD_IMAGE_PADDING;
        int y = buildAreaHeight + BUTTON_SPACING;
        int width = ORDERS_BAR_WIDTH;
        int height = SELECTION_NAME_HEIGHT;
        GUI.Label(new Rect(x, y, width, height), selectionName);
        //Debug.Log(string.Format("Selected entity is named {0}", selectionName));
    }

    private void DisplayAvailableActions()
    {
        if (player == null)
        {
            return;
        }
        if (player.selectedEntity == null)
        {
            return;
        }

        bool ownedByPlayer = player.selectedEntity.OwnedByPlayer(player);
        if (ownedByPlayer == false)
        {
            return;
        }

        ResetSlider();
        string[] allActions = player.selectedEntity.GetActions();
        DrawActions(allActions);
        previousSelection = player.selectedEntity;
    }

    private void DrawActions(string[] actionsToDraw)
    {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;
        int totalActions = actionsToDraw.Length;

        int x = BUILD_IMAGE_WIDTH;
        int y = 0;
        int width = ORDERS_BAR_WIDTH;
        int height = buildAreaHeight;
        GUI.BeginGroup(new Rect(x, y, width, height));

        if (totalActions >= MaxRowsInArea(buildAreaHeight))
        {
            float sliderSize = totalActions / 2f;
            DrawSlider(buildAreaHeight, sliderSize);
        }

        for (int i = 0; i < totalActions; i++)
        {
            int column = i % 2;
            int row = i / 2;
            Rect buttonArea = GetButtonArea(row, column);
            
            string action = actionsToDraw[i];
			Texture2D actionIcon = GameManager.activeInstance.GetBuildIcon(action);
            if (actionIcon == null)
            {
                continue;
            }

            bool buttonCreated = GUI.Button(buttonArea, actionIcon);
            if (buttonCreated == false)
            {
                continue;
            }

            if (player.selectedEntity == null)
            {
                continue;
            }

            player.selectedEntity.PerformAction(action);
        }

        GUI.EndGroup();
    }

    private void ResetSlider()
    {
        if (previousSelection == null)
        {
            return;
        }
        if (previousSelection == player.selectedEntity)
        {
            return;
        }

        sliderValue = 0f;
    }

    private int MaxRowsInArea(int areaHeight)
    {
        int maxRows = areaHeight / BUILD_IMAGE_HEIGHT;
        return maxRows;
    }

    private Rect GetButtonArea(int row, int column)
    {
        int leftEdge = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
        float topEdge = BUILD_IMAGE_HEIGHT * (row - sliderValue);
        Rect buttonArea = new Rect(leftEdge, topEdge, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
        return buttonArea;
    }

    private void DrawSlider(int groupHeight, float totalRows)
    {
        Rect scrollArea = GetScrollArea(groupHeight);
        float topValue = 0f;
        float bottomValue = totalRows - MaxRowsInArea(groupHeight);
        sliderValue = GUI.VerticalSlider(scrollArea, sliderValue, topValue, bottomValue);
    }

    private Rect GetScrollArea(int groupHeight)
    {
        int height = groupHeight - 2 * BUTTON_SPACING;
        Rect scrollArea = new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, height);
        return scrollArea;
    }

    private void DisplayBuildQueue()
    {
        if (previousSelection == null)
        {
            return;
        }
        StructureController selectedStructure = previousSelection.GetComponent<StructureController>();
        if (selectedStructure == null)
        {
            return;
        }

        string[] buildQueue = selectedStructure.GetBuildQueueEntries();
        float buildPercentage = selectedStructure.GetBuildCompletionPercentage();

        for (int i = 0; i < buildQueue.Length; i++)
        {
            float topEdge = i * BUILD_IMAGE_HEIGHT - (i + 1) * BUILD_IMAGE_PADDING;
            Rect buildArea = new Rect(BUILD_IMAGE_PADDING, topEdge, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
            string unitName = buildQueue[i];
            Texture2D unitIcon = GameManager.activeInstance.GetBuildIcon(unitName);
            GUI.DrawTexture(buildArea, unitIcon);
            GUI.DrawTexture(buildArea, buildFrame);
            topEdge += BUILD_IMAGE_PADDING;
            float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
            float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
            if (i == 0)
            {
                // Shrink the build mask on the time currently being built to give an idea of progress
                topEdge += height * buildPercentage;
                height *= (1 - buildPercentage);
            }
            int leftEdge = 2 * BUILD_IMAGE_PADDING;
            GUI.DrawTexture(new Rect(leftEdge, topEdge, width, height), buildMask);
        }
    }

    private void DrawResourceBar()
    {
        GUI.skin = resourceSkin;

        int x = 0;
        int y = 0;
        int width = Screen.width;
        int height = RESOURCE_BAR_HEIGHT;
        GUI.BeginGroup(new Rect(x, y, width, height));
        
        GUI.Box(new Rect(x, y, width, height), string.Empty);

        int topEdge = 4;
        int iconLeft = 4;
        int textLeft = 20;
        ResourceType[] allResources = (ResourceType[]) Enum.GetValues(typeof(ResourceType));
        foreach(ResourceType resource in allResources) {
            DrawResourceIcon(resource, iconLeft, textLeft, topEdge);
            iconLeft += TEXT_WIDTH;
            textLeft += TEXT_WIDTH;
        }

        GUI.EndGroup();
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topEdge)
    {
        Texture2D icon = allResources[type];
        int value = resourceValues[type];
        int maxValue = resourceLimits[type];
        string text = string.Format("{0} / {1}", value, maxValue);
        GUI.DrawTexture(new Rect(iconLeft, topEdge, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topEdge, TEXT_WIDTH, TEXT_HEIGHT), text);
    }

    public bool MouseInPlayArea()
    {
        Vector3 position = Input.mousePosition;

        bool xInBounds = (position.x >= 0) && (position.x <= (Screen.width - ORDERS_BAR_WIDTH));
        bool yInBounds = (position.y >= 0) && (position.y <= (Screen.height - RESOURCE_BAR_HEIGHT));

        return xInBounds && yInBounds;
    }

    public bool MouseOnScreen()
    {
        Vector3 position = Input.mousePosition;

        bool xInBounds = (position.x >= 0) && (position.x <= Screen.width);
        bool yInBounds = (position.y >= 0) && (position.y <= Screen.height);

        return xInBounds && yInBounds;
    }

    public Rect GetPlayingArea()
    {
        int x = 0;
        int y = RESOURCE_BAR_HEIGHT;
        int width = Screen.width - ORDERS_BAR_WIDTH;
        int height = Screen.height - RESOURCE_BAR_HEIGHT;

        Rect playingArea = new Rect(x, y, width, height);
        return playingArea;
    }
}
