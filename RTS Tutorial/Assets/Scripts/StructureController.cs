using UnityEngine;
using System.Collections.Generic;
using RTS;

public class StructureController : EntityController {

    protected Queue<string> buildQueue;
    private float currentBuildProgress = 0f;
    public float buildComplete;
    private Vector3 spawnPoint;
    protected Vector3 rallyPoint;
    public Texture2D rallyPointIcon;
    public Texture2D sellIcon;

    protected override void Awake()
    {
        base.Awake();
        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0f, spawnZ);
        rallyPoint = spawnPoint;
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
        if (currentBuildProgress < buildComplete)
        {
            return;
        }
        if (owner == null)
        {
            return;
        }

        string unitName = buildQueue.Dequeue();
        owner.SpawnUnit(unitName, spawnPoint, rallyPoint, transform.rotation);
        currentBuildProgress = 0f;
    }

    public string[] GetBuildQueueEntries()
    {
        string[] entries = buildQueue.ToArray();
        return entries;
    }

    public float GetBuildCompletionPercentage()
    {
        float completionPercentage = currentBuildProgress / buildComplete;
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

        bool isGround = hitEntity.CompareTag(Tags.ground);
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

    public bool HasSpawnPoint()
    {
        bool hasSpawnPoint = spawnPoint != ResourceManager.invalidPoint;
        bool hasRallyPoint = rallyPoint != ResourceManager.invalidPoint;

        return hasSpawnPoint && hasRallyPoint;
    }

    public override void SetOverState(GameObject entityUnderMouse)
    {
        base.SetOverState(entityUnderMouse);

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

        bool isGround = CompareTag(Tags.ground);
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
}