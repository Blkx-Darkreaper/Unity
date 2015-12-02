using UnityEngine;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class ConstructorUnit : UnitController {

    public int constructionSpeed;
    protected StructureController currentProject;
    protected int projectId = -1;
    private bool isConstructing = false;
    private float constructionProgress = 0f;
    protected struct ConstructorProperties
    {
        public const string IS_CONSTRUCTING = "IsConstructing";
        public const string CONSTRUCTION_PROGRESS = "ConstructionProgress";
        public const string PROJECT_ID = "ProjectId";
    }

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Refinery", "Factory" };

        if (isLoadedFromSave == false)
        {
            return;
        }

        LoadCurrentProject(projectId);
    }

    protected override void Update()
    {
        base.Update();

        if (isMoving == true)
        {
            return;
        }
        if (isTurning == true)
        {
            return;
        }
        if (isConstructing == false)
        {
            return;
        }
        if (currentProject == null)
        {
            return;
        }

        bool constructionComplete = currentProject.isConstructionComplete;
        if (constructionComplete == true)
        {
            return;
        }

        constructionProgress += constructionSpeed * Time.deltaTime;
        int progress = Mathf.FloorToInt(constructionProgress);
        if (progress <= 0)
        {
            return;
        }

        constructionProgress -= progress;
        currentProject.Construct(progress);

        constructionComplete = currentProject.isConstructionComplete;
        if (constructionComplete == false)
        {
            return;
        }

        isConstructing = false;
    }

    public override void MouseClick(GameObject hitGameObject, Vector3 hitPoint, PlayerController player)
    {
        bool constructionProjectSet = false;
        TryToSetConstructionProject(hitGameObject, player, out constructionProjectSet);

        if (constructionProjectSet == true)
        {
            return;
        }

        base.MouseClick(hitGameObject, hitPoint, player);
    }

    protected void LoadCurrentProject(int entityId)
    {
        if (entityId < 0)
        {
            return;
        }

        try
        {
            currentProject = (StructureController)GameManager.activeInstance.GetGameEntityById(entityId);
        }
        catch
        {
            Debug.Log(string.Format("Failed to load current Project"));
        }
    }

    protected void TryToSetConstructionProject(GameObject hitGameObject, PlayerController player, out bool succeeded)
    {
        succeeded = false;

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
        if (hitGameObject == null)
        {
            return;
        }

        bool isGround = hitGameObject.CompareTag(Tags.GROUND);
        if (isGround == true)
        {
            return;
        }

        StructureController structure = hitGameObject.GetComponentInParent<StructureController>();
        if (structure == null)
        {
            return;
        }

        bool underConstruction = !structure.isConstructionComplete;
        if (underConstruction == false)
        {
            return;
        }

        SetConstructionProject(structure);
        succeeded = true;
    }

    public override void SetSpawner(StructureController spawner)
    {
        base.SetSpawner(spawner);
        SetConstructionProject(spawner);
    }

    public void SetConstructionProject(StructureController project)
    {
        currentProject = project;
        SetWaypoint(currentProject.transform.position, currentProject.gameObject);
        isConstructing = true;
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        ConstructStructure(actionToPerform);
    }

    public override void SetWaypoint(Vector3 destination)
    {
        base.SetWaypoint(destination);
        constructionProgress = 0f;
        isConstructing = false;
    }

    private void ConstructStructure(string structureName)
    {
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z + 10;
        Vector3 buildPoint = new Vector3(x, y, z);

        if (owner == null)
        {
            return;
        }

        owner.SpawnStructure(structureName, buildPoint, this, playingArea);
    }

    protected override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);

        SaveManager.SaveBoolean(writer, ConstructorProperties.IS_CONSTRUCTING, isConstructing);
        SaveManager.SaveFloat(writer, ConstructorProperties.CONSTRUCTION_PROGRESS, constructionProgress);
        if (currentProject != null)
        {
            SaveManager.SaveInt(writer, ConstructorProperties.PROJECT_ID, currentProject.entityId);
        }
    }

    protected override bool LoadDetails(JsonReader reader, string propertyName)
    {
        base.LoadDetails(reader, propertyName);

        bool loadComplete = false;

        switch (propertyName)
        {
            case ConstructorProperties.IS_CONSTRUCTING:
                isConstructing = LoadManager.LoadBoolean(reader);
                break;

            case ConstructorProperties.CONSTRUCTION_PROGRESS:
                constructionProgress = LoadManager.LoadFloat(reader);
                loadComplete = true;
                break;

            case ConstructorProperties.PROJECT_ID:
                projectId = LoadManager.LoadInt(reader);
                loadComplete = true;
                break;
        }

        return loadComplete;
    }
}