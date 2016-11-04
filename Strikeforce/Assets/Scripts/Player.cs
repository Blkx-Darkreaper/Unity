using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Player : Entity
    {
        public string Username;
        public bool IsNPC;
        [HideInInspector]
        public Hud PlayerHud;
        public Raider Raider;
        public BuildCursor Cursor;
        public Selectable SelectedEntity { get; set; }
        public int StartingMoney, StartingMoneyLimit, StartingFuel, StartingFuelLimit;
        private Dictionary<ResourceType, int> resources, resourceLimits;
        public Material NotAllowedMaterial, AllowedMaterial;
        private Structure constructionSite;
        public Color Colour;
        public bool IsSettingConstructionPoint { get; protected set; }
        public bool IsDead
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
                Structure[] allStructures = GetComponentsInChildren<Structure>();
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
                Selectable[] allUnits = GetComponentsInChildren<Selectable>();
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
            public const string FACTION_COLOUR = "TeamColour";
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
            if (Username == null)
            {
                Username = "Unknown";
                return;
            }

            if (Username.Equals(string.Empty) == false)
            {
                return;
            }

            Username = "Unknown";
        }

        private void InitResourceLists()
        {
            resources = new Dictionary<ResourceType, int>();
            resources.Add(ResourceType.money, 0);
            resources.Add(ResourceType.fuel, 0);

            resourceLimits = new Dictionary<ResourceType, int>();
            resourceLimits.Add(ResourceType.money, StartingMoneyLimit);
            resourceLimits.Add(ResourceType.fuel, StartingFuelLimit);

            AddResource(ResourceType.money, StartingMoney);
            AddResource(ResourceType.fuel, StartingFuel);
        }

        private void Start()
        {
            PlayerHud = GetComponentInChildren<Hud>();
            IsSettingConstructionPoint = false;
        }

        private void Update()
        {
            if (IsNPC == true)
            {
                return;
            }

            PlayerHud.SetResourceValues(resources, resourceLimits);

            if (IsSettingConstructionPoint == false)
            {
                return;
            }

            constructionSite.UpdateBounds();

            bool validConstructionSite = IsConstructionSiteValid();
            if (validConstructionSite == true)
            {
                constructionSite.SetTransparencyMaterial(AllowedMaterial, false);
            }
            else
            {
                constructionSite.SetTransparencyMaterial(NotAllowedMaterial, false);
            }
        }

        public bool IsConstructionSiteValid()
        {
            Bounds buildSiteBounds = constructionSite.SelectionBounds;

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

                Selectable hitEntity = hitGameObject.GetComponentInParent<Selectable>();
                if (hitEntity == null)
                {
                    continue;
                }

                Bounds hitEntityBounds = hitEntity.SelectionBounds;
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
            IsSettingConstructionPoint = false;
            GameObject allStructures = transform.Find(PlayerProperties.STRUCTURES).gameObject;
            if (allStructures == null)
            {
                return;
            }

            bool sufficientFunds = HasSufficientResources(ResourceType.money, constructionSite.Cost);
            if (sufficientFunds == false)
            {
                InsufficientResources(ResourceType.money);
                return;
            }

            RemoveResource(ResourceType.money, constructionSite.Cost);
            constructionSite.transform.parent = allStructures.transform;
            constructionSite.Owner = this;
            constructionSite.SetColliders(true);
            constructionSite.StartConstruction();
        }

        public void InsufficientResources(ResourceType type)
        {
            Debug.Log(string.Format("{0} has insufficient {1}", name, type.ToString()));
        }

        public void CancelConstruction()
        {
            IsSettingConstructionPoint = false;
            Destroy(constructionSite.gameObject);
            constructionSite = null;
        }

        public void AddResource(ResourceType type, int amountToAdd)
        {
            resources[type] += amountToAdd;
            if (resourceLimits.ContainsKey(type) == false)
            {
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
            GameObject unitToSpawn = (GameObject)Instantiate(GameManager.ActiveInstance.GetUnitPrefab(unitName), spawnPoint, startingOrientation);
            unitToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", unitName, Username));

            if (rallyPoint == GlobalAssets.InvalidPoint)
            {
                return;
            }
            if (spawnPoint == rallyPoint)
            {
                return;
            }

            Selectable controller = unitToSpawn.GetComponent<Selectable>();
            if (controller == null)
            {
                return;
            }

            controller.SetWaypoint(rallyPoint);
        }

        public void SpawnUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation, Structure spawner)
        {
            GameObject allUnits = transform.Find(PlayerProperties.UNITS).gameObject;
            if (allUnits == null)
            {
                return;
            }

            GameObject unitToSpawn = (GameObject)Instantiate(GameManager.ActiveInstance.GetUnitPrefab(unitName), spawnPoint, startingOrientation);
            unitToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", unitName, Username));

            if (rallyPoint == GlobalAssets.InvalidPoint)
            {
                return;
            }

            Selectable controller = unitToSpawn.GetComponent<Selectable>();
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

        public void SpawnStructure(string structureName, Vector3 buildPoint, Selectable builder, Rect playingArea)
        {
            GameObject structureToSpawn = (GameObject)Instantiate(GameManager.ActiveInstance.GetStructurePrefab(structureName),
                buildPoint, new Quaternion());

            constructionSite = structureToSpawn.GetComponent<Structure>();
            if (constructionSite != null)
            {
                IsSettingConstructionPoint = true;
                constructionSite.SetTransparencyMaterial(NotAllowedMaterial, true);
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

            SaveManager.SaveString(writer, PlayerProperties.USERNAME, Username);
            SaveManager.SaveBoolean(writer, PlayerProperties.IS_NPC, IsNPC);
            SaveManager.SaveColour(writer, PlayerProperties.FACTION_COLOUR, Colour);
            //SaveCamera(writer);
            SaveResources(writer);
            SaveStructures(writer);
            SaveUnits(writer);
        }

        private void SaveCamera(JsonWriter writer)
        {
            Entity camera = GetCamera();
            if (camera == null)
            {
                return;
            }

            writer.WritePropertyName(JsonProperties.CAMERA);
            camera.Save(writer);
        }

        private Entity GetCamera()
        {
            foreach (Entity entity in GetComponentsInChildren<Entity>())
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
                KeyValuePair<string, string> properties = GlobalAssets.PlayerResourceProperties[index];

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
            Structure[] allStructures = GetComponentsInChildren<Structure>();
            if (allStructures.Length == 0)
            {
                return;
            }

            bool firstEntry = true;
            foreach (Structure structure in allStructures)
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
            Selectable[] allUnits = GetComponentsInChildren<Selectable>();
            if (allUnits.Length == 0)
            {
                return;
            }

            bool firstEntry = true;
            foreach (Selectable unit in allUnits)
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
                    Username = LoadManager.LoadString(reader);
                    break;

                case PlayerProperties.IS_NPC:
                    IsNPC = LoadManager.LoadBoolean(reader);
                    break;

                case PlayerProperties.FACTION_COLOUR:
                    Colour = LoadManager.LoadColour(reader);
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

        protected override void LoadEnd(bool loadingComplete)
        {
            if (loadingComplete == false)
            {
                Debug.Log(string.Format("Failed to load {0} {1}", name, Username));
                GameObject[] allChildren = GetComponentsInChildren<GameObject>();
                foreach (GameObject child in allChildren)
                {
                    GameManager.ActiveInstance.DestroyGameEntity(child);
                }

                GameManager.ActiveInstance.DestroyGameEntity(gameObject);
                return;
            }

            isLoadedFromSave = true;
            Debug.Log(string.Format("Loaded {0} {1}", name, Username));
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
                        StartingMoney = LoadManager.LoadInt(reader);
                        break;

                    case ResourceProperties.MONEY_LIMIT:
                        StartingMoneyLimit = LoadManager.LoadInt(reader);
                        break;

                    case ResourceProperties.FUEL:
                        StartingFuel = LoadManager.LoadInt(reader);
                        break;

                    case ResourceProperties.FUEL_LIMIT:
                        StartingFuelLimit = LoadManager.LoadInt(reader);
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

                bool isNameProperty = propertyName.Equals(Entity.nameProperty);
                if (isNameProperty == false)
                {
                    continue;
                }

                entityName = LoadManager.LoadString(reader);
                GameObject prefab = null;

                switch (entityType)
                {
                    case PlayerProperties.STRUCTURES:
                        prefab = GameManager.ActiveInstance.GetStructurePrefab(entityName);
                        break;

                    case PlayerProperties.UNITS:
                        prefab = GameManager.ActiveInstance.GetUnitPrefab(entityName);
                        break;
                }

                if (prefab == null)
                {
                    Debug.Log(string.Format("{0} prefab could not be found", entityName));
                    continue;
                }

                GameObject clone = (GameObject)GameObject.Instantiate(prefab);
                Entity entity = clone.GetComponent<Entity>();
                if (entity == null)
                {
                    continue;
                }

                entity.Load(reader);
                TakeOwnershipOfEntity(entity, entityType);
                OtherEntitySetup(entity, entityType);
            }
        }

        protected void TakeOwnershipOfEntity(Entity entity, string entityType)
        {
            GameObject group = transform.Find(entityType).gameObject;
            entity.transform.parent = group.transform;
        }

        protected void OtherEntitySetup(Entity entity, string entityType)
        {
            switch (entityType)
            {
                case PlayerProperties.STRUCTURES:
                    Structure structure = (Structure)entity;
                    if (structure.IsConstructionComplete == false)
                    {
                        structure.SetTransparencyMaterial(AllowedMaterial, true);
                    }
                    break;
            }
        }
    }
}