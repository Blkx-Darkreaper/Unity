using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class PlayerController : MonoBehaviour {

    public string username;
    public bool isNPC;

    [HideInInspector]
    public HudController hud;
    public Color teamColour;
    public EntityController selectedEntity { get; set; }
    public int startingMoney, startingMoneyLimit, startingPower, startingPowerLimit;
    private Dictionary<ResourceType, int> resources, resourceLimits;
    public Material notAllowedMaterial, allowedMaterial;
    private StructureController constructionSite;
    private UnitController constructor;
    public bool isSettingConstructionPoint { get; protected set; }

    private void Awake()
    {
        InitResourceLists();
    }

    private void InitResourceLists()
    {
        resources = new Dictionary<ResourceType, int>();
        resources.Add(ResourceType.money, 0);
        resources.Add(ResourceType.power, 0);

        AddResource(ResourceType.money, startingMoney);
        AddResource(ResourceType.power, startingPower);

        resourceLimits = new Dictionary<ResourceType, int>();
        resourceLimits.Add(ResourceType.money, startingMoneyLimit);
        resourceLimits.Add(ResourceType.power, startingPowerLimit);
    }

    private void Start()
    {
        hud = GetComponentInChildren<HudController>();
        isSettingConstructionPoint = false;
    }

    private void Update()
    {
        if (isNPC == true)
        {
            return;
        }

        hud.SetResourceValues(resources, resourceLimits);

        if (isSettingConstructionPoint == false)
        {
            return;
        }

        constructionSite.UpdateBounds();

        bool validConstructionSite = IsConstructionSiteValid();
        if (validConstructionSite == true)
        {
            constructionSite.SetTransparencyMaterial(allowedMaterial, false);
        }
        else
        {
            constructionSite.SetTransparencyMaterial(notAllowedMaterial, false);
        }
    }

    public bool IsConstructionSiteValid()
    {
        Bounds buildSiteBounds = constructionSite.selectionBounds;

        List<Vector3> allCorners = GameManager.GetBoundsCorners(buildSiteBounds);

        foreach (Vector3 corner in allCorners)
        {
            GameObject hitGameObject = UserInput.GetHitGameObject(corner);
            if (hitGameObject == null)
            {
                continue;
            }

            bool hitGameObjectIsGround = hitGameObject.CompareTag(Tags.ground);
            if (hitGameObjectIsGround == true)
            {
                continue;
            }

            EntityController hitEntity = hitGameObject.GetComponentInParent<EntityController>();
            if (hitEntity == null)
            {
                continue;
            }

            Bounds hitEntityBounds = hitEntity.selectionBounds;
            bool boundsIntersect = buildSiteBounds.Intersects(hitEntityBounds);
            if (boundsIntersect == false)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public void StartConstruction()
    {
        isSettingConstructionPoint = false;
        GameObject allStructures = transform.Find("Units").gameObject;
        if (allStructures == null)
        {
            return;
        }

        constructionSite.transform.parent = allStructures.transform;
        constructionSite.owner = this;
        constructionSite.SetColliders(true);
        constructor.SetSpawner(constructionSite);
        constructionSite.StartConstruction();
    }

    public void CancelConstruction()
    {
        isSettingConstructionPoint = false;
        Destroy(constructionSite.gameObject);
        constructionSite = null;
        constructor = null;
    }

    public void AddResource(ResourceType type, int amountToAdd)
    {
        resources[type] += amountToAdd;
    }

    public void SetResourceLimit(ResourceType type, int limit)
    {
        resourceLimits[type] = limit;
    }

    public void SpawnUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation)
    {
        GameObject allUnits = transform.Find("Units").gameObject;
        if (allUnits == null)
        {
            return;
        }
        //Units allUnits = GetComponentInChildren<Units>();
        GameObject unitToSpawn = (GameObject)Instantiate(GameManager.activeInstance.GetUnit(unitName), spawnPoint, startingOrientation);
        unitToSpawn.transform.parent = allUnits.transform;
        Debug.Log(string.Format("Spawned {0} for player {1}", unitName, username));

        if (rallyPoint == ResourceManager.invalidPoint)
        {
            return;
        }
        if (spawnPoint == rallyPoint)
        {
            return;
        }

        UnitController controller = unitToSpawn.GetComponent<UnitController>();
        if (controller == null)
        {
            return;
        }

        controller.SetWaypoint(rallyPoint);
    }

    public void SpawnUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation, StructureController spawner)
    {
        GameObject allUnits = transform.Find("Units").gameObject;
        if (allUnits == null)
        {
            return;
        }

        GameObject unitToSpawn = (GameObject)Instantiate(GameManager.activeInstance.GetUnit(unitName), spawnPoint, startingOrientation);
        unitToSpawn.transform.parent = allUnits.transform;
        Debug.Log(string.Format("Spawned {0} for player {1}", unitName, username));

        if (rallyPoint == ResourceManager.invalidPoint)
        {
            return;
        }

        UnitController controller = unitToSpawn.GetComponent<UnitController>();
        if (controller == null)
        {
            return;
        }

        controller.SetSpawner(spawner);

		if (spawnPoint == rallyPoint)
		{
			return;
		}

        controller.SetWaypoint(rallyPoint);
    }

    public void SpawnStructure(string structureName, Vector3 buildPoint, UnitController builder, Rect playingArea)
    {
        GameObject structureToSpawn = (GameObject)Instantiate(GameManager.activeInstance.GetStructure(structureName), 
            buildPoint, new Quaternion());

        constructionSite = structureToSpawn.GetComponent<StructureController>();
        if (constructionSite != null)
        {
            constructor = builder;
            isSettingConstructionPoint = true;
            constructionSite.SetTransparencyMaterial(notAllowedMaterial, true);
            constructionSite.SetColliders(false);
            constructionSite.playingArea = playingArea;
        }
        else
        {
            Destroy(structureToSpawn);
        }
    }

    public void SetConstructionPoint()
    {
        Vector3 constructionPosition = UserInput.GetHitPoint();
        constructionPosition.y = 0;
        constructionSite.transform.position = constructionPosition;
    }
}