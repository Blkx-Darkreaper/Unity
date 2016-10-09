using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Strikeforce;

public class Hud : MonoBehaviour
{

    public GUISkin resourceSkin, ordersSkin, selectionBoxSkin, playerDetailsSkin;
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
    private const int BUTTON_PADDING = 7;
    private const int SCROLL_BAR_WIDTH = 22;

    private PlayerController player;

    [HideInInspector]
    public Texture2D activeCursor;
    public Texture2D selectCursor, panLeftCursor, panRightCursor, panUpCursor, panDownCursor, rallyPointCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    private CursorState activeCursorState;
    private int currentFrame = 0;
    public CursorState previousCursorState { get; private set; }

    public Texture2D[] resourceIcons;
    private Dictionary<ResourceType, Texture2D> allResources;
    private Dictionary<ResourceType, int> resourceValues, resourceLimits;

    private EntityController previousSelection = null;
    private float sliderValue;
    public Texture2D buttonHover, buttonClick;
    private int buildAreaHeight;
    public Texture2D buildFrame, buildMask;
    public Texture2D smallButtonHover, smallButtonClick;

    public Texture2D healthyTexture, damagedTexture, criticalTexture;
    public Texture2D[] resourceHealthBars;


    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, 
        Dictionary<ResourceType, int> resourceLimits)
    {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
        StoreSelectionBoxTextures(selectionBoxSkin, healthyTexture, damagedTexture, criticalTexture);
        StoreResourceHealthBarTextures();
        SetCursorState(CursorState.select);
        resourceValues = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();
        InitResources();
        buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_PADDING;
        SetPlayerUsername();
    }

    private void SetPlayerUsername()
    {
        string username = GameManager.activeInstance.currentPlayerAccount.username;
        if (player.isNPC == true)
        {
            return;
        }

        player.username = username;
    }

    private void StoreSelectionBoxTextures(GUISkin skin, Texture2D healthyTexture, Texture2D damagedTexture, Texture2D criticalTexture)
    {
        ResourceManager.selectionBoxSkin = skin;
        ResourceManager.HealthBarTextures.healthy = healthyTexture;
        ResourceManager.HealthBarTextures.damaged = damagedTexture;
        ResourceManager.HealthBarTextures.critical = criticalTexture;
    }

    private void StoreResourceHealthBarTextures()
    {
        ResourceManager.SetResourceBarTextures(resourceHealthBars);
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
                    break;

                case "Power":
                    allResources.Add(ResourceType.power, resource);
                    break;

                case "Ore":
                    break;

                case "Unknown":
                    break;

                default:
                    break;
            }
        }

        ResourceType[] allTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        foreach (ResourceType type in allTypes)
        {
            resourceValues.Add(type, 0);
            resourceLimits.Add(type, 0);
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
            //Debug.Log(string.Format("Non-Human player"));
            return;
        }

        DrawPlayerDetails();
        DrawOrdersBar();
        DrawResourceBar();
        DrawMouseCursor();
    }

    private void DrawPlayerDetails()
    {
        GUI.skin = playerDetailsSkin;

        float x = 0;
        float y = 0;
        float width = Screen.width;
        float height = Screen.height;
        GUI.BeginGroup(new Rect(x, y, width, height));

        height = Menu.textHeight;
        x = Menu.textHeight;
        y = Screen.height - x - Menu.padding;

        x = DrawPlayerAvatar(x, y, height);

        float minWidth = 0, maxWidth = 0;

        string username = player.username;
        //string currentPlayerUsername = GameManager.activeInstance.currentPlayerAccount.username;
        //if (currentPlayerUsername.Equals(username) == false)
        //{
        //    Debug.Log(string.Format("Usernames do not match"));
        //}

        playerDetailsSkin.GetStyle("label").CalcMinMaxWidth(new GUIContent(username), out minWidth, out maxWidth);
        GUI.Label(new Rect(x, y, maxWidth, height), username);

        GUI.EndGroup();
    }

    private float DrawPlayerAvatar(float x, float y, float height)
    {
        int avatarId = GameManager.activeInstance.currentPlayerAccount.avatarId;
        Texture2D avatar = ResourceManager.GetAvatar(avatarId);
        if (avatar == null)
        {
            return x;
        }

        GUI.DrawTexture(new Rect(x, y, height, height), avatar);

        x += height + Menu.padding;
        return x;
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
        if (player.isSettingConstructionPoint == true)
        {
            Cursor.visible = false;
            return;
        }

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
        if (activeCursorState != state)
        {
            previousCursorState = activeCursorState;
        }

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

            case CursorState.rallyPoint:
                activeCursor = rallyPointCursor;
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

            case CursorState.rallyPoint:
                topEdge -= activeCursor.height;
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

        DrawAvailableActions();
        StructureController selectedStructure = GetSelectedStructure();
        DrawBuildQueue(selectedStructure);
        DrawStandardStructureOptions(selectedStructure);
        DrawSelectedNameInOrdersBar();

        GUI.EndGroup();
    }

    private void DrawSelectedNameInOrdersBar()
    {
        if (player.selectedEntity == null)
        {
            return;
        }

        string selectionName = player.selectedEntity.name;
        if (selectionName.Equals(string.Empty))
        {
            Debug.Log("Selected entity has no name");
            return;
        }

        int x = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + 2 * BUILD_IMAGE_PADDING;
        int y = buildAreaHeight + BUTTON_PADDING;
        int width = ORDERS_BAR_WIDTH;
        int height = SELECTION_NAME_HEIGHT;
        GUI.Label(new Rect(x, y, width, height), selectionName);
        //Debug.Log(string.Format("Selected entity is named {0}", selectionName));
    }

    private void DrawAvailableActions()
    {
        if (player == null)
        {
            return;
        }
        if (player.selectedEntity == null)
        {
            return;
        }

        bool ownedByPlayer = player.selectedEntity.IsOwnedByPlayer(player);
        if (ownedByPlayer == false)
        {
            return;
        }

        ResetSlider();
        string[] allActions = player.selectedEntity.actions;
        DrawActions(allActions);
        previousSelection = player.selectedEntity;
    }

    private void DrawActions(string[] actionsToDraw)
    {
        if (actionsToDraw == null)
        {
            return;
        }

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

            bool buttonClicked = GUI.Button(buttonArea, actionIcon);
            if (buttonClicked == false)
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
        int height = groupHeight - 2 * BUTTON_PADDING;
        Rect scrollArea = new Rect(BUTTON_PADDING, BUTTON_PADDING, SCROLL_BAR_WIDTH, height);
        return scrollArea;
    }

    private StructureController GetSelectedStructure()
    {
        if (previousSelection == null)
        {
            return null;
        }

        StructureController selectedStructure = previousSelection.GetComponent<StructureController>();
        return selectedStructure;
    }

    private void DrawBuildQueue(StructureController selectedStructure)
    {
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

    private void DrawStandardStructureOptions(StructureController structure)
    {
        if (player == null)
        {
            return;
        }
        if (player.selectedEntity == null)
        {
            return;
        }
        if (structure == null)
        {
            return;
        }

        GUIStyle smallButtons = new GUIStyle();
        smallButtons.hover.background = smallButtonHover;
        smallButtons.active.background = smallButtonClick;
        GUI.skin.button = smallButtons;

        int leftEdge = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_PADDING;
        int topEdge = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
        int width = BUILD_IMAGE_WIDTH / 2;
        int height = BUILD_IMAGE_HEIGHT / 2;

        bool hasSpawnPoint = structure.HasValidSpawnPoint();
        if (hasSpawnPoint == false)
        {
            return;
        }

        SellButtonHandler(structure, leftEdge, topEdge, width, height);
        RallyPointButtonHandler(structure, leftEdge, topEdge, width, height);
    }

    private void SellButtonHandler(StructureController structure, int leftEdge, int topEdge, int width, int height)
    {
        Texture2D sellIcon = structure.sellIcon;
        leftEdge += width + BUTTON_PADDING;

        bool buttonPressed = GUI.Button(new Rect(leftEdge, topEdge, width, height), sellIcon);
        if (buttonPressed == false)
        {
            return;
        }

        structure.Sell();
    }

    private void RallyPointButtonHandler(StructureController structure, int leftEdge, int topEdge, int width, int height)
    {
        Texture2D rallyPointIcon = structure.rallyPointIcon;
        bool buttonPressed = GUI.Button(new Rect(leftEdge, topEdge, width, height), rallyPointIcon);
        if (buttonPressed == false)
        {
            return;
        }

        if (activeCursorState != CursorState.rallyPoint && previousCursorState != CursorState.rallyPoint)
        {
            SetCursorState(CursorState.rallyPoint);
        }
        else
        {
            SetCursorState(CursorState.panRight);
            SetCursorState(CursorState.select);
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
        ResourceType[] typesToDisplay = new ResourceType[] { ResourceType.money, ResourceType.power };
        foreach (ResourceType resource in typesToDisplay)
        {
            DrawResourceIcon(resource, iconLeft, textLeft, topEdge);
            iconLeft += TEXT_WIDTH;
            textLeft += TEXT_WIDTH;
        }

        DrawMenuButton();

        GUI.EndGroup();
    }

    private void DrawMenuButton()
    {
        int width = ORDERS_BAR_WIDTH - 2 * BUTTON_PADDING - SCROLL_BAR_WIDTH;
        int height = RESOURCE_BAR_HEIGHT - 2 * BUTTON_PADDING;
        int x = Screen.width - ORDERS_BAR_WIDTH / 2 - width / 2 + SCROLL_BAR_WIDTH / 2;
        int y = BUTTON_PADDING;
        Rect menuButton = new Rect(x, y, width, height);

        bool buttonPressed = GUI.Button(menuButton, "Menu");
        if (buttonPressed == false)
        {
            return;
        }

        PauseMenu();
    }

    private void PauseMenu()
    {
        Time.timeScale = 0f;
        PauseMenu pauseMenu = GetComponent<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.enabled = true;
        }
        UserInput userInput = player.GetComponent<UserInput>();
        if (userInput != null)
        {
            userInput.enabled = false;
        }
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

    public static Rect GetPlayingArea()
    {
        int x = 0;
        int y = RESOURCE_BAR_HEIGHT;
        int width = Screen.width - ORDERS_BAR_WIDTH;
        int height = Screen.height - RESOURCE_BAR_HEIGHT;

        Rect playingArea = new Rect(x, y, width, height);
        return playingArea;
    }
}
