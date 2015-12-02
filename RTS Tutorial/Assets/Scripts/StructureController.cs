using UnityEngine;
using System.Collections.Generic;
using RTS;
using Newtonsoft.Json;

public class StructureController : EntityController {

    protected Queue<string> buildQueue;
    private float currentBuildProgress = 0f;
    public float buildTime;
    public bool isConstructionComplete { get; protected set; }
    private const string CONSTRUCTION_MESSAGE = "Building...";
    private Vector3 spawnPoint;
    protected Vector3 rallyPoint;
    public Texture2D rallyPointIcon;
    public Texture2D sellIcon;
    protected struct StructureProperties
    {
        public const string IS_CONSTRUCTION_COMPLETE = "IsConstructionComplete";
        public const string SPAWN_POINT = "SpawnPoint";
        public const string RALLY_POINT = "RallyPoint";
        public const string BUILD_PROGRESS = "BuildProgress";
        public const string BUILD_QUEUE = "BuildQueue";
    }

    protected override void Awake()
    {
        base.Awake();
        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0f, spawnZ);
        rallyPoint = spawnPoint;
        isConstructionComplete = true;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        BuildUnits();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        if (isConstructionComplete == true)
        {
            return;
        }

        DrawConstructionProgress();
    }

    private void DrawConstructionProgress()
    {
        GUI.skin = ResourceManager.selectionBoxSkin;
        Rect selectionBox = GameManager.CalculateSelectionBox(selectionBounds, playingArea);

        // Draw the selection box around the currently selected object, within the bounds of the main draw area
        GUI.BeginGroup(playingArea);

        UpdateHealthPercentage(0.99f, 0.5f);
        DrawHealthBarWithLabel(selectionBox, CONSTRUCTION_MESSAGE);

        GUI.EndGroup();
    }

    protected void BuildUnit(string unitName)
    {
        buildQueue.Enqueue(unitName);
    }

    protected void BuildUnits()
    {
        if (buildQueue.Count <= 0)
        {
            return;
        }

        currentBuildProgress += Time.deltaTime * ResourceManager.buildSpeed;
        if (currentBuildProgress < buildTime)
        {
            return;
        }
        if (owner == null)
        {
            return;
        }

        string unitName = buildQueue.Dequeue();
        owner.SpawnUnit(unitName, spawnPoint, rallyPoint, transform.rotation, this);
        currentBuildProgress = 0f;
    }

    public string[] GetBuildQueueEntries()
    {
        string[] entries = buildQueue.ToArray();
        return entries;
    }

    public float GetBuildCompletionPercentage()
    {
        float completionPercentage = currentBuildProgress / buildTime;
        return completionPercentage;
    }

    public override void SetSelection(bool selected)
    {
        base.SetSelection(selected);

        if (owner == null)
        {
            return;
        }
        if (owner.isNPC == true)
        {
            return;
        }

        RallyPointController flag = GetComponentInChildren<RallyPointController>();
        if (flag == null)
        {
            return;
        }

        if (selected == true)
        {
            if (spawnPoint == ResourceManager.invalidPoint)
            {
                return;
            }
            if (rallyPoint == ResourceManager.invalidPoint)
            {
                return;
            }

            flag.transform.localPosition = rallyPoint;
            flag.transform.forward = transform.forward;
            flag.Enable();
        }
        else
        {
            flag.Disable();
        }
    }

    public override void MouseClick(GameObject hitEntity, Vector3 hitPoint, PlayerController player)
    {
        base.MouseClick(hitEntity, hitPoint, player);

        if (player == null)
        {
            return;
        }
        if (player.isNPC == true)
        {
            return;
        }
        if (isSelected == false)
        {
            return;
        }

        bool isGround = hitEntity.CompareTag(Tags.GROUND);
        if (isGround == false)
        {
            return;
        }

        if (hitPoint == ResourceManager.invalidPoint)
        {
            return;
        }

        float x = hitPoint.x;
        float y = hitPoint.y + player.selectedEntity.transform.position.y;  // Ensures rallyPoint stays on top of the surface it is on
        float z = hitPoint.z;
        Vector3 pointClicked = new Vector3(x, y, z);
        UpdateRallyPointPosition(pointClicked);
    }

    private void UpdateRallyPointPosition(Vector3 updatedPoint)
    {
        rallyPoint = updatedPoint;

        RallyPointController flag = GetComponentInChildren<RallyPointController>();
        if (flag == null)
        {
            return;
        }
        flag.transform.localPosition = updatedPoint;
        flag.transform.forward = transform.forward;
    }

    public bool HasValidSpawnPoint()
    {
        bool hasSpawnPoint = spawnPoint != ResourceManager.invalidPoint;
        bool hasRallyPoint = rallyPoint != ResourceManager.invalidPoint;

        return hasSpawnPoint && hasRallyPoint;
    }

    public override void SetHoverState(GameObject entityUnderMouse)
    {
        base.SetHoverState(entityUnderMouse);

        if (owner == null)
        {
            return;
        }
        if (owner.isNPC == true)
        {
            return;
        }
        if (isSelected == false)
        {
            return;
        }

        bool isGround = CompareTag(Tags.GROUND);
        if (isGround == false)
        {
            return;
        }

        CursorState previousState = owner.hud.previousCursorState;
        if (previousState != CursorState.rallyPoint)
        {
            return;
        }

        owner.hud.SetCursorState(CursorState.rallyPoint);
    }

    public void Sell()
    {
        if (owner == null)
        {
            return;
        }

        owner.AddResource(ResourceType.money, sellValue);

        if (isSelected == true)
        {
            SetSelection(false);
        }

        Destroy(this.gameObject);
    }

    public void StartConstruction()
    {
        UpdateBounds();
        isConstructionComplete = false;
        currentHitPoints = 0;
    }

    public void Construct(int amount)
    {
        currentHitPoints += amount;

        if (currentHitPoints < maxHitPoints)
        {
            return;
        }

        currentHitPoints = maxHitPoints;
        isConstructionComplete = true;
        RestoreMaterials();
        SetTeamColour();
    }

    protected override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);

        SaveManager.SaveBoolean(writer, StructureProperties.IS_CONSTRUCTION_COMPLETE, isConstructionComplete);
        SaveManager.SaveVector(writer, StructureProperties.SPAWN_POINT, spawnPoint);
        SaveManager.SaveVector(writer, StructureProperties.RALLY_POINT, rallyPoint);
        SaveManager.SaveFloat(writer, StructureProperties.BUILD_PROGRESS, currentBuildProgress);
        SaveManager.SaveStringArray(writer, StructureProperties.BUILD_QUEUE, buildQueue.ToArray());
    }

    protected override bool LoadDetails(JsonReader reader, string propertyName)
    {
        bool loadComplete = false;

        base.LoadDetails(reader, propertyName);

        switch (propertyName)
        {
            case StructureProperties.IS_CONSTRUCTION_COMPLETE:
                isConstructionComplete = LoadManager.LoadBoolean(reader);
                break;

            case StructureProperties.SPAWN_POINT:
                spawnPoint = LoadManager.LoadVector(reader);
                break;

            case StructureProperties.RALLY_POINT:
                rallyPoint = LoadManager.LoadVector(reader);
                break;

            case StructureProperties.BUILD_PROGRESS:
                currentBuildProgress = LoadManager.LoadFloat(reader);
                break;

            case StructureProperties.BUILD_QUEUE:
                buildQueue = new Queue<string>(LoadManager.LoadStringArray(reader));
                loadComplete = true;    // last property to load
                break;
        }

        return loadComplete;
    }
}