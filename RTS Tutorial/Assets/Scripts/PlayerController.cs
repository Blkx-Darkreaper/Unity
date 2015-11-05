using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class PlayerController : MonoBehaviour {

    public string username;
    public bool isNPC;

    [HideInInspector]
    public HudController hud;
    public EntityController selectedEntity { get; set; }
    public int startingMoney, startingMoneyLimit, startingPower, startingPowerLimit;
    private Dictionary<ResourceType, int> resources, resourceLimits;

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
    }

    private void Update()
    {
        if (isNPC == true)
        {
            return;
        }

        hud.SetResourceValues(resources, resourceLimits);
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
}