using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Newtonsoft.Json;

public class PlayerController : PersistentEntity {

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
    public bool isDead
    {
        get
        {
            bool noStructuresLeft = hasNoStructures;
            if (noStructuresLeft == false)
            {
                return false;
            }

            bool noUnitsLeft = hasNoUnits;
            return noUnitsLeft;
        }
    }
    public bool hasNoStructures
    {
        get
        {
            StructureController[] allStructures = GetComponentsInChildren<StructureController>();
            if (allStructures == null)
            {
                return true;
            }

            if (allStructures.Length == 0)
            {
                return true;
            }

            return false;
        }
    }
    public bool hasNoUnits
    {
        get
        {
            UnitController[] allUnits = GetComponentsInChildren<UnitController>();
            if (allUnits == null)
            {
                return true;
            }

            if (allUnits.Length == 0)
            {
                return true;
            }

            return false;
        }
    }
    protected struct PlayerProperties
    {
        public const string USERNAME = "Username";
        public const string IS_NPC = "IsNPC";
        public const string TEAM_COLOUR = "TeamColour";
        public const string RESOURCES = "Resources";
        public const string STRUCTURES = "Structures";
        public const string UNITS = "Units";
    }

    protected override void Awake()
    {
        InitUsername();
        InitResourceLists();
    }

    private void InitUsername()
    {
        if (username == null)
        {
            username = "Unknown";
            return;
        }

        if (username.Equals(string.Empty) == false)
        {
            return;
        }

        username = "Unknown";
    }

    private void InitResourceLists()
    {
        resources = new Dictionary<ResourceType, int>();
        resources.Add(ResourceType.money, 0);
        resources.Add(ResourceType.power, 0);

		resourceLimits = new Dictionary<ResourceType, int>();
		resourceLimits.Add(ResourceType.money, startingMoneyLimit);
		resourceLimits.Add(ResourceType.power, startingPowerLimit);

        AddResource(ResourceType.money, startingMoney);
        AddResource(ResourceType.power, startingPower);
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

            bool hitGameObjectIsGround = hitGameObject.CompareTag(Tags.GROUND);
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
        GameObject allStructures = transform.Find(PlayerProperties.STRUCTURES).gameObject;
        if (allStructures == null)
        {
            return;
        }

        bool sufficientFunds = HasSufficientResources(ResourceType.money, constructionSite.cost);
        if (sufficientFunds == false)
        {
            InsufficientResources(ResourceType.money);
            return;
        }

        RemoveResource(ResourceType.money, constructionSite.cost);
        constructionSite.transform.parent = allStructures.transform;
        constructionSite.owner = this;
        constructionSite.SetColliders(true);
        constructor.SetSpawner(constructionSite);
        constructionSite.StartConstruction();
    }

    public void InsufficientResources(ResourceType type)
    {
        Debug.Log(string.Format("{0} has insufficient {1}", name, type.ToString()));
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
		if (resourceLimits [type] == null) {
			return;
		}

        resources[type] = Mathf.Clamp(resources[type], 0, resourceLimits[type]);
    }

    public bool HasSufficientResources(ResourceType type, int amountRequired)
    {
        int currentAmount = resources[type];

        bool sufficientResources = amountRequired <= currentAmount;
        return sufficientResources;
    }

    public void RemoveResource(ResourceType type, int amountToRemove)
    {
        resources[type] -= amountToRemove;
        resources[type] = Mathf.Clamp(resources[type], 0, resourceLimits[type]);
    }

    public void SetResourceLimit(ResourceType type, int limit)
    {
        resourceLimits[type] = limit;
    }

    public int GetResourceAmount(ResourceType type)
    {
        int amount = resources[type];
        return amount;
    }

    public void SpawnUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation)
    {
        GameObject allUnits = transform.Find(PlayerProperties.UNITS).gameObject;
        if (allUnits == null)
        {
            return;
        }
        //Units allUnits = GetComponentInChildren<Units>();
        GameObject unitToSpawn = (GameObject)Instantiate(GameManager.activeInstance.GetUnitPrefab(unitName), spawnPoint, startingOrientation);
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
        GameObject allUnits = transform.Find(PlayerProperties.UNITS).gameObject;
        if (allUnits == null)
        {
            return;
        }

        GameObject unitToSpawn = (GameObject)Instantiate(GameManager.activeInstance.GetUnitPrefab(unitName), spawnPoint, startingOrientation);
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
        GameObject structureToSpawn = (GameObject)Instantiate(GameManager.activeInstance.GetStructurePrefab(structureName), 
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

    protected override void SaveDetails(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        SaveManager.SaveString(writer, PlayerProperties.USERNAME, username);
        SaveManager.SaveBoolean(writer, PlayerProperties.IS_NPC, isNPC);
        SaveManager.SaveColour(writer, PlayerProperties.TEAM_COLOUR, teamColour);
        //SaveCamera(writer);
        SaveResources(writer);
        SaveStructures(writer);
        SaveUnits(writer);
    }

    private void SaveCamera(JsonWriter writer)
    {
        PersistentEntity camera = GetCamera();
        if (camera == null)
        {
            return;
        }

        writer.WritePropertyName(JsonProperties.CAMERA);
        camera.Save(writer);
    }

    private PersistentEntity GetCamera()
    {
        foreach (PersistentEntity entity in GetComponentsInChildren<PersistentEntity>())
        {
            bool tagMatch = entity.CompareTag(Tags.MAIN_CAMERA);
            if (tagMatch == false)
            {
                continue;
            }

            return entity;
        }

        return null;
    }

    private void SaveResources(JsonWriter writer)
    {
        if (resources.Count == 0)
        {
            return;
        }

        writer.WritePropertyName(PlayerProperties.RESOURCES);
        
        writer.WriteStartArray();
        foreach (KeyValuePair<ResourceType, int> entry in resources)
        {
            writer.WriteStartObject();
            int index = (int)entry.Key;
            KeyValuePair<string, string> properties = ResourceManager.PlayerResourceProperties[index];
            
            string propertyName = properties.Key;
			int value = entry.Value;
            SaveManager.SaveInt(writer, propertyName, value);

            propertyName = properties.Value;
            value = resourceLimits[entry.Key];
            SaveManager.SaveInt(writer, propertyName, value);

            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private void SaveStructures(JsonWriter writer)
    {
        StructureController[] allStructures = GetComponentsInChildren<StructureController>();
        if (allStructures.Length == 0)
        {
            return;
        }

        bool firstEntry = true;
        foreach (StructureController structure in allStructures)
        {
            if (firstEntry == true)
            {
                writer.WritePropertyName(PlayerProperties.STRUCTURES);
                writer.WriteStartArray();
                firstEntry = false;
            }

            structure.Save(writer);
        }

        writer.WriteEndArray();
    }

    private void SaveUnits(JsonWriter writer)
    {
        UnitController[] allUnits = GetComponentsInChildren<UnitController>();
        if (allUnits.Length == 0)
        {
            return;
        }

        bool firstEntry = true;
        foreach (UnitController unit in allUnits)
        {
            if (firstEntry == true)
            {
                writer.WritePropertyName(PlayerProperties.UNITS);
                writer.WriteStartArray();
                firstEntry = false;
            }

            unit.Save(writer);
        }

        writer.WriteEndArray();
    }

    protected override bool LoadDetails(JsonReader reader, string propertyName)
    {
        // Properties must be loaded in the order they were saved for loadCompleted to work properly
        bool loadCompleted = false;

        switch (propertyName)
        {
            case PlayerProperties.USERNAME:
				username = LoadManager.LoadString(reader);
                break;

            case PlayerProperties.IS_NPC:
                isNPC = LoadManager.LoadBoolean(reader);
                break;

            case PlayerProperties.TEAM_COLOUR:
                teamColour = LoadManager.LoadColour(reader);
                break;

            case PlayerProperties.RESOURCES:
                LoadResources(reader);
                break;

            case PlayerProperties.STRUCTURES:
                LoadStructures(reader);
                break;

            case PlayerProperties.UNITS:
                LoadUnits(reader);
                loadCompleted = true;   // Last property to load
                break;
        }

        return loadCompleted;
    }

	protected override void LoadEnd (bool loadingComplete)
	{
		if (loadingComplete == false)
		{
			Debug.Log(string.Format("Failed to load {0} {1}", name, username));
			GameObject[] allChildren = GetComponentsInChildren<GameObject>();
			foreach(GameObject child in allChildren) {
				GameManager.activeInstance.DestroyGameEntity(child);
			}

			GameManager.activeInstance.DestroyGameEntity(gameObject);
			return;
		}
		
		isLoadedFromSave = true;
		Debug.Log(string.Format("Loaded {0} {1}", name, username));
	}

    private void LoadResources(JsonReader reader)
    {
        if (reader == null)
        {
            return;
        }

        string propertyName = string.Empty;
        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndArray)
                {
                    continue;
                }

                return;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = LoadManager.LoadString(reader);
                continue;
            }

            switch (propertyName)
            {
                case ResourceProperties.MONEY:
                    startingMoney = LoadManager.LoadInt(reader);
                    break;

                case ResourceProperties.MONEY_LIMIT:
                    startingMoneyLimit = LoadManager.LoadInt(reader);
                    break;

                case ResourceProperties.POWER:
                    startingPower = LoadManager.LoadInt(reader);
                    break;

                case ResourceProperties.POWER_LIMIT:
                    startingPowerLimit = LoadManager.LoadInt(reader);
                    break;
            }
        }
    }

    private void LoadStructures(JsonReader reader)
    {
        LoadPlayerOwnedEntities(reader, PlayerProperties.STRUCTURES);
    }

    private void LoadUnits(JsonReader reader)
    {
        LoadPlayerOwnedEntities(reader, PlayerProperties.UNITS);
    }

    private void LoadPlayerOwnedEntities(JsonReader reader, string entityType)
    {
        if (reader == null)
        {
            return;
        }

        string propertyName = string.Empty;
        string entityName = string.Empty;

        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndArray)
                {
                    continue;
                }

                return;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = LoadManager.LoadString(reader);
                continue;
            }

            bool isNameProperty = propertyName.Equals(EntityController.nameProperty);
            if (isNameProperty == false)
            {
                continue;
            }

            entityName = LoadManager.LoadString(reader);
			GameObject prefab = null;

			switch(entityType) {
				case PlayerProperties.STRUCTURES:
            	prefab = GameManager.activeInstance.GetStructurePrefab(entityName);
				break;

				case PlayerProperties.UNITS:
				prefab = GameManager.activeInstance.GetUnitPrefab(entityName);
				break;
			}

			if(prefab == null) {
				Debug.Log(string.Format("{0} prefab could not be found", entityName));
				continue;
			}

            GameObject clone = (GameObject)GameObject.Instantiate(prefab);
            EntityController entity = clone.GetComponent<EntityController>();
			if(entity == null) {
				continue;
			}

            entity.Load(reader);
            TakeOwnershipOfEntity(entity, entityType);
            OtherEntitySetup(entity, entityType);
        }
    }

    protected void TakeOwnershipOfEntity(EntityController entity, string entityType)
    {
        GameObject group = transform.Find(entityType).gameObject;
        entity.transform.parent = group.transform;
    }

    protected void OtherEntitySetup(EntityController entity, string entityType)
    {
        switch (entityType)
        {
            case PlayerProperties.STRUCTURES:
                StructureController structure = (StructureController)entity;
                if (structure.isConstructionComplete == false)
                {
                    structure.SetTransparencyMaterial(allowedMaterial, true);
                }
                break;
        }
    }
}