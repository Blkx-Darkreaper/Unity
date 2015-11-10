using UnityEngine;
using System.Collections;
using RTS;

public class ConstructorUnit : UnitController {

    public int constructionSpeed;
    private StructureController currentProject;
    private bool isConstructing = false;
    private float constructionProgress = 0f;

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Refinery", "Factory" };
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

        bool constructionComplete = currentProject.constructionComplete;
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

        constructionComplete = currentProject.constructionComplete;
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

    private void TryToSetConstructionProject(GameObject hitGameObject, PlayerController player, out bool succeeded)
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

        bool isGround = hitGameObject.CompareTag(Tags.ground);
        if (isGround == true)
        {
            return;
        }

        StructureController structure = hitGameObject.GetComponentInParent<StructureController>();
        if (structure == null)
        {
            return;
        }

        bool underConstruction = !structure.constructionComplete;
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
}