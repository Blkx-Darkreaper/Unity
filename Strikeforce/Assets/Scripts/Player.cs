using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Player : Entity
    {
        public int PlayerId { get { return this.playerControllerId; } }
        public bool IsNPC;
        [HideInInspector]  protected Camera mainCamera;
        [HideInInspector] public Hud PlayerHud;
        [HideInInspector] public BuildCursor Cursor;
        [HideInInspector] public Raider CurrentRaider;
        [HideInInspector] protected bool isInBuildMode = true;
        [HideInInspector] public Inventory CurrentInventory;
        public Selectable SelectedEntity { get; set; }
        public bool IsSettingConstructionPoint;
        public Material NotAllowedMaterial, AllowedMaterial;
        public Color Colour;

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
            Profile profile = ProfileManager.Singleton.CurrentProfile;
            if (profile != null)
            {
                profile.Player = this;
            }

            // Get the main camera
            mainCamera = GameObject.FindGameObjectWithTag(Tags.MAIN_CAMERA).GetComponent<Camera>();

            SpawnRaider();
            CurrentInventory = GetComponent<Inventory>();
        }

        protected void SpawnRaider()
        {
            GameObject raiderObject = Instantiate(
                NetworkManager.singleton.spawnPrefabs[3], 
                Vector3.zero, 
                Quaternion.identity) as GameObject;

            CurrentRaider = raiderObject.GetComponent<Raider>();
            //NetworkServer.SpawnWithClientAuthority(raiderObject, connectionToClient);

            Vector3 raiderPosition = raiderObject.transform.position;

            Vector3 overheadView = new Vector3(raiderPosition.x, raiderPosition.y + 20, raiderPosition.z);
            mainCamera.transform.position = overheadView;
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        public override void OnStartLocalPlayer()
        {
            //CurrentRaider.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        protected void Start()
        {
            PlayerHud = GetComponentInChildren<Hud>();
            IsSettingConstructionPoint = false;
        }

        public void MovePlayer(float x, float y, float z)
        {
            if (isInBuildMode == true)
            {
                Cursor.transform.Translate(x, 0, z);
            }
            else
            {
                CurrentRaider.transform.Translate(x, 0, z);
            }
        }

        public bool IsConstructionSiteValid(Structure constructionSite)
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

        //public void StartConstruction()
        //{
        //    IsSettingConstructionPoint = false;
        //    GameObject allStructures = transform.Find(PlayerProperties.STRUCTURES).gameObject;
        //    if (allStructures == null)
        //    {
        //        return;
        //    }

        //    bool sufficientFunds = HasSufficientResources(ResourceType.money, constructionSite.Cost);
        //    if (sufficientFunds == false)
        //    {
        //        InsufficientResources(ResourceType.money);
        //        return;
        //    }

        //    RemoveResource(ResourceType.money, constructionSite.Cost);
        //    constructionSite.transform.parent = allStructures.transform;
        //    constructionSite.Owner = this;
        //    constructionSite.SetColliders(true);
        //    constructionSite.StartConstruction();
        //}

        //public void CancelConstruction()
        //{
        //    IsSettingConstructionPoint = false;
        //    Destroy(constructionSite.gameObject);
        //    constructionSite = null;
        //}

        public void SpawnUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation)
        {
            GameObject allUnits = transform.Find(PlayerProperties.UNITS).gameObject;
            if (allUnits == null)
            {
                return;
            }
            //Units allUnits = GetComponentInChildren<Units>();
            GameObject unitToSpawn = (GameObject)Instantiate(GameManager.Singleton.GetUnitPrefab(unitName), spawnPoint, startingOrientation);
            unitToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", unitName, PlayerId));

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

            GameObject unitToSpawn = (GameObject)Instantiate(GameManager.Singleton.GetUnitPrefab(unitName), spawnPoint, startingOrientation);
            unitToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", unitName, PlayerId.ToString()));

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

        //public void SpawnStructure(string structureName, Vector3 buildPoint, Selectable builder, Rect playingArea)
        //{
        //    GameObject structureToSpawn = (GameObject)Instantiate(GameManager.ActiveInstance.GetStructurePrefab(structureName),
        //        buildPoint, new Quaternion());

        //    constructionSite = structureToSpawn.GetComponent<Structure>();
        //    if (constructionSite != null)
        //    {
        //        IsSettingConstructionPoint = true;
        //        constructionSite.SetTransparencyMaterial(NotAllowedMaterial, true);
        //        constructionSite.SetColliders(false);
        //        constructionSite.playingArea = playingArea;
        //    }
        //    else
        //    {
        //        Destroy(structureToSpawn);
        //    }
        //}

        //public void SetConstructionPoint()
        //{
        //    Vector3 constructionPosition = UserInput.GetHitPoint();
        //    constructionPosition.y = 0;
        //    constructionSite.transform.position = constructionPosition;
        //}

        protected void TakeOwnershipOfEntity(Selectable selectable, string entityType)
        {
            GameObject group = transform.Find(entityType).gameObject;
            selectable.transform.parent = group.transform;
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