using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Newtonsoft.Json;

public class HarvesterUnit : UnitController
{

    public float maxLoadSize;
    private bool isHarvesting = false;
    private bool isDepositing = false;
    private float currentLoad = 0f;
    private ResourceType harvestType;
    private ResourceController resourceSource;
    public StructureController resourceStore;
    public float harvestAmount, depositAmount;
    private float currentDeposit = 0f;
    private const int MAX_HEIGHT_BUFFER = 4;
    private const int LEFT_EDGE_BUFFER = 7;
    private const int TOP_EDGE_BUFFER = 2;
    protected struct HarvesterProperties
    {
        public const string IS_HARVESTING = "IsHarvesting";
        public const string IS_DEPOSITING = "IsDepositing";
        public const string CURRENT_LOAD = "CurrentLoad";
        public const string CURRENT_DEPOSIT = "CurrentDeposit";
        public const string HARVEST_TYPE = "HarvestType";
        public const string SOURCE_ID = "ResourceSourceId";
        public const string STORE_ID = "ResourceStoreId";
    }

    public override void SetSpawner(StructureController spawner)
    {
        base.SetSpawner(spawner);
        SetDepository(spawner);
    }

    public void SetDepository(StructureController depository)
    {
        resourceStore = depository;
    }

    protected override void Start()
    {
        base.Start();
        harvestType = ResourceType.unknown;
    }

    protected override void Update()
    {
        base.Update();
        if (isTurning == true)
        {
            return;
        }
        if (isMoving == true)
        {
            return;
        }

        GameObject[] arms = GetArms();

        if (isHarvesting == true)
        {
            ShowArms(arms);
            HarvestResource();
            if (currentLoad < maxLoadSize)
            {
                return;
            }
            //bool depositDepleted = resourceDeposit.isEmpty;
            //if (depositDepleted == false)
            //{
            //    return;
            //}

            currentLoad = Mathf.Floor(currentLoad); // Round down to a whole number to avoid bugs
            isHarvesting = false;
            isDepositing = true;
            HideArms(arms);
            SetWaypoint(resourceStore.transform.position, resourceStore.gameObject);

            string ownersName = "Neutral";
            if (owner != null)
            {
                ownersName = string.Format("{0}'s", owner.username);
            }

            Debug.Log(string.Format("{0} {1} is returning to depository to offload", ownersName, entityName));
        }

        if (isDepositing == true)
        {
            Offload();
            if (currentLoad > 0)
            {
                return;
            }

            isDepositing = false;
            HideArms(arms);

            bool depositDepleted = resourceSource.isEmpty;
            if (depositDepleted == true)
            {
                return;
            }

            isHarvesting = true;
            SetWaypoint(resourceSource.transform.position, resourceSource.gameObject);

            string ownersName = "Neutral";
            if (owner != null)
            {
                ownersName = string.Format("{0}'s", owner.username);
            }

            Debug.Log(string.Format("{0} {1} is harvesting", ownersName, entityName));
        }
    }

    private GameObject[] GetArms()
    {
        GameObject meshes = transform.Find("Meshes").gameObject;
        Renderer[] allShapes = meshes.GetComponentsInChildren<Renderer>();
        List<GameObject> allArms = new List<GameObject>();
        foreach (Renderer shape in allShapes)
        {
            if (!shape.name.StartsWith("Arm"))
            {
                continue;
            }

            GameObject arm = shape.gameObject;
            allArms.Add(arm);
        }
        return allArms.ToArray();
    }

    private static void ShowArms(GameObject[] arms)
    {
        foreach (GameObject arm in arms)
        {
            //Collider collider = arm.GetComponent<Collider>();
            //if (collider != null)
            //{
            //    collider.enabled = true;
            //}

            Renderer renderer = arm.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    private static void HideArms(GameObject[] arms)
    {
        foreach (GameObject arm in arms)
        {
            //Collider collider = arm.GetComponent<Collider>();
            //if (collider != null)
            //{
            //    collider.enabled = false;
            //}

            Renderer renderer = arm.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
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

        bool isGround = entityUnderMouse.CompareTag(Tags.GROUND);
        if (isGround == true)
        {
            return;
        }

        ResourceController resource = entityUnderMouse.GetComponentInParent<ResourceController>();
        if (resource == null)
        {
            return;
        }

        bool notEmpty = !resource.isEmpty;
        if (notEmpty == false)
        {
            return;
        }

        owner.hud.SetCursorState(CursorState.harvest);
    }

    public override void MouseClick(GameObject hitEntity, Vector3 hitPoint, PlayerController player)
    {
        base.MouseClick(hitEntity, hitPoint, player);

        if (owner == null)
        {
            return;
        }
        if (owner.isNPC == true)
        {
            return;
        }

        bool isGround = hitEntity.CompareTag(Tags.GROUND);
        if (isGround == true)
        {
            return;
        }

        ResourceController resource = hitEntity.GetComponentInParent<ResourceController>();
        if (resource == null)
        {
            return;
        }

        bool notEmpty = !resource.isEmpty;
        if (notEmpty == true)
        {
            if (owner.selectedEntity != null)
            {
                owner.selectedEntity.SetSelection(false);
            }

            SetSelection(true);
            owner.selectedEntity = this;
            StartHarvesting(resource);
        }
        else
        {
            StopHarvesting();
        }
    }

    private void StartHarvesting(ResourceController resource)
    {
        resourceSource = resource;
        SetWaypoint(resource.transform.position, resource.gameObject);
        ResourceType depositType = resource.type;
        if (harvestType != depositType)
        {
            harvestType = depositType;
            currentLoad = 0f;
        }
        isHarvesting = true;
        isDepositing = false;
    }

    private void StopHarvesting()
    {
        string ownersName = "Neutral";
        if (owner != null)
        {
            ownersName = string.Format("{0}'s", owner.username);
        }

        Debug.Log(string.Format("{0} {1} has stopped harvesting", ownersName, entityName));
    }

    private void HarvestResource()
    {
        if (resourceSource == null)
        {
            return;
        }

        bool depositDepleted = resourceSource.isEmpty;
        if (depositDepleted == true)
        {
            //Destroy(resourceDeposit);
            return;
        }

        float amountHarvested = harvestAmount * Time.deltaTime;
        amountHarvested = Mathf.Clamp(amountHarvested, 0f, maxLoadSize - currentLoad);
        resourceSource.Harvest(amountHarvested);
        currentLoad += amountHarvested;

        string ownersName = "Neutral";
        if (owner != null)
        {
            ownersName = string.Format("{0}'s", owner.username);
        }

        //Debug.Log(string.Format("{0} {1} harvested {2} {3}", ownersName, entityName, amountHarvested, harvestType.ToString()));
    }

    private void Offload()
    {
        currentDeposit += depositAmount * Time.deltaTime;
        int deposit = Mathf.FloorToInt(currentDeposit);
        if (deposit < 0)
        {
            return;
        }

        deposit = Mathf.Clamp(deposit, 0, Mathf.FloorToInt(currentLoad));
        currentDeposit -= deposit;
        currentLoad -= deposit;
        ResourceType depositType = harvestType;

        if (harvestType == ResourceType.ore)
        {
            depositType = ResourceType.money;
        }

        owner.AddResource(depositType, deposit);

        string ownersName = "Neutral";
        if (owner != null)
        {
            ownersName = string.Format("{0}'s", owner.username);
        }

        //Debug.Log(string.Format("{0} {1} offloaded {2} {3}", ownersName, entityName, deposit, harvestType.ToString()));
    }

    protected override void DrawSelectionBox(Rect selectionBox)
    {
        base.DrawSelectionBox(selectionBox);

        float percentFull = currentLoad / maxLoadSize;
        float maxHeight = selectionBox.height - MAX_HEIGHT_BUFFER;
        float height = maxHeight * percentFull;
        float leftEdge = selectionBox.x + selectionBox.width - LEFT_EDGE_BUFFER;
        float topEdge = selectionBox.y + TOP_EDGE_BUFFER + (maxHeight - height);
        float width = 5;
        Texture2D resourceBar = ResourceManager.GetResourceBarTexture(harvestType);

        if (resourceBar == null)
        {
            return;
        }

        GUI.DrawTexture(new Rect(leftEdge, topEdge, width, height), resourceBar);
    }

    protected override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);

        SaveManager.SaveBoolean(writer, HarvesterProperties.IS_HARVESTING, isHarvesting);
        SaveManager.SaveBoolean(writer, HarvesterProperties.IS_DEPOSITING, isDepositing);
        SaveManager.SaveFloat(writer, HarvesterProperties.CURRENT_LOAD, currentLoad);
        SaveManager.SaveFloat(writer, HarvesterProperties.CURRENT_DEPOSIT, currentDeposit);
        SaveManager.SaveString(writer, HarvesterProperties.HARVEST_TYPE, harvestType.ToString());
        if (resourceSource != null)
        {
            SaveManager.SaveInt(writer, HarvesterProperties.SOURCE_ID, resourceSource.entityId);
        }
        if (resourceStore != null)
        {
            SaveManager.SaveInt(writer, HarvesterProperties.STORE_ID, resourceStore.entityId);
        }
    }
}