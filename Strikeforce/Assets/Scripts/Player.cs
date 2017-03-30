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
        [HideInInspector]
        protected Camera mainCamera;
        [HideInInspector]
        public Hud PlayerHud;
        [HideInInspector]
        public bool IsMenuOpen { get; set; }
        [HideInInspector]
        public Raider CurrentRaider;
        public string RaiderPrefabName = "Raider";
        [HideInInspector]
        public Inventory CurrentInventory;
        [HideInInspector]
        protected bool isInBuildMode = true;
        [HideInInspector]
        public BuildCursor Cursor;
        public LinkedList<Sector> Sectors { get; protected set; }
        public Selectable SelectedEntity { get; set; }
        public bool IsSettingConstructionPoint;
        public Material NotAllowedMaterial, AllowedMaterial;
        public Color Colour;
        protected const string UNIT = "Unit";
        protected const string STRUCTURE = "Structure";

        protected override void Awake()
        {
            Profile profile = ProfileManager.Singleton.CurrentProfile;
            if (profile != null)
            {
                profile.Player = this;
            }

            // Get the main camera
            this.mainCamera = GameObject.FindGameObjectWithTag(Tags.MAIN_CAMERA).GetComponent<Camera>();
            this.CurrentInventory = GetComponent<Inventory>();
            this.Sectors = new LinkedList<Sector>();
            this.CurrentLevel = GameManager.Singleton.CurrentLevels[0];
        }

        public override void OnStartLocalPlayer()
        {
            //CurrentRaider.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        protected void Start()
        {
            PlayerHud = GetComponentInChildren<Hud>();
            IsSettingConstructionPoint = false;
            SpawnRaider();
        }

        protected void SpawnRaider()	// Testing
        {
            // Get spawn point from level
            Vector3 spawn = CurrentLevel.RaiderSpawn.Location;
            GameObject raiderObject = Instantiate(
                GlobalAssets.GetVehiclePrefab(RaiderPrefabName),
                spawn,
                Quaternion.identity) as GameObject;
            raiderObject.transform.parent = gameObject.transform;

            CurrentRaider = raiderObject.GetComponent<Raider>();
            //NetworkServer.SpawnWithClientAuthority(raiderObject, connectionToClient);

            this.CurrentLevel = GameObject.FindGameObjectWithTag(Tags.LEVEL).GetComponent<Level>();

            Vector3 raiderPosition = raiderObject.transform.position;

            CurrentRaider.SetLayout(new Vector3[] {
            	new Vector3(-10, 0, 1),
            	new Vector3(-5, 0, 1),
            	new Vector3(0, 0, 1),
            	new Vector3(5, 0, 1),
            	new Vector3(10, 0, 1)},
                new Hardpoint[] { new Hardpoint(-138, -69, 1, 1, HardpointPosition.LeftOuterWing) },
                new Hardpoint[] { new Hardpoint(-94, -16, 1, 1, HardpointPosition.LeftWing) },
                new Hardpoint[] { new Hardpoint(-22, 116, 1, 1, HardpointPosition.Center),
                	new Hardpoint(-22, 26, 1, 3, HardpointPosition.Center) },
                new Hardpoint[] { new Hardpoint(50, -16, 1, 1, HardpointPosition.RightWing) },
                new Hardpoint[] { new Hardpoint(94, -69, 1, 1, HardpointPosition.RightOuterWing) });

            GameObject basicShotPrefab = GlobalAssets.GetWeaponPrefab(Weapon.Types.BASIC_SHOT);
            Weapon basicShot = GameObject.Instantiate(basicShotPrefab).GetComponent<Weapon>() as Weapon;
            basicShot.transform.parent = CurrentRaider.transform;

            bool equipped = CurrentRaider.EquipWeapon(basicShot, HardpointPosition.Center, 0, 0, 0);
            if (equipped == true)
            {
                CurrentRaider.ReadyWeapons();
            }

            NetworkServer.Spawn(CurrentRaider.gameObject);

            // Set camera overhead
            Vector3 overheadView = new Vector3(raiderPosition.x, raiderPosition.y + 10, raiderPosition.z);
            mainCamera.transform.position = overheadView;
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        public void LeftStick(float x, float y, float z)
        {
            CmdMovePlayer(x, y, z);
        }

        public void RightStick(float x, float y, float z) { }

        [Command]
        protected void CmdMovePlayer(float x, float y, float z)
        {
            isInBuildMode = false;    //testing
            Vector3 cameraPosition = Vector3.zero;
            if (isInBuildMode == true)
            {
                Cursor.Move(x, z);
            }
            else
            {
                CurrentRaider.Move(x, z);
                cameraPosition = CurrentRaider.transform.position;
            }

            //MoveCamera(x, y, z);
            SetCameraPosition(cameraPosition);
        }

        protected void MoveCamera(float x, float y, float z)
        {
            Vector3 currentPosition = transform.position;

            CurrentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            mainCamera.transform.Translate(x, z, y);
        }

        protected void SetCameraPosition(Vector3 position)
        {
            float y = mainCamera.transform.position.y;
            mainCamera.transform.position = new Vector3(position.x, y, position.z);
        }

        public void RespondToKeyEvent(KeyEvent keyEvent)
        {
            ActionKey key = keyEvent.Key;

            switch (key)
            {
                case ActionKey.Action1:
                    if (isInBuildMode == true)
                    {
                    }
                    else
                    {
                    }
                    break;

                case ActionKey.RightTrigger:
                    if (isInBuildMode == true)
                    {
                    }
                    else
                    {
                        KeyEvent.Type eventType = keyEvent.EventType;
                        if (eventType == KeyEvent.Type.Released)
                        {
                            CmdSetPrimaryFiring(false);
                            break;
                        }

                        CmdSetPrimaryFiring(true);
                    }
                    break;
            }
        }

        [Command]
        protected void CmdSetPrimaryFiring(bool isFiring)
        {
            // Command function is called from the client, but invoked on the server
            Raider raider = CurrentRaider;
            if (raider == null)
            {
                return;
            }

            raider.SetPrimaryFire(isFiring);
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
        //	IsSettingConstructionPoint = false;
        //	GameObject allStructures = transform.Find(PlayerProperties.STRUCTURES).gameObject;
        //	if (allStructures == null)
        //	{
        //    	return;
        //	}

        //	bool sufficientFunds = HasSufficientResources(ResourceType.Money, constructionSite.Cost);
        //	if (sufficientFunds == false)
        //	{
        //    	InsufficientResources(ResourceType.Money);
        //    	return;
        //	}

        //	RemoveResource(ResourceType.Money, constructionSite.Cost);
        //	constructionSite.transform.parent = allStructures.transform;
        //	constructionSite.Owner = this;
        //	constructionSite.SetColliders(true);
        //	constructionSite.StartConstruction();
        //}

        //public void CancelConstruction()
        //{
        //	IsSettingConstructionPoint = false;
        //	Destroy(constructionSite.gameObject);
        //	constructionSite = null;
        //}

        public void SpawnUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation)
        {
            GameObject allUnits = transform.Find(UNIT).gameObject;
            if (allUnits == null)
            {
                return;
            }
            //Units allUnits = GetComponentInChildren<Units>();
            GameObject unitToSpawn = (GameObject)Instantiate(GlobalAssets.GetVehiclePrefab(unitName), spawnPoint, startingOrientation);
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

        public void SpawnUnit(string vehicleName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation, Structure spawner)
        {
            GameObject allUnits = transform.Find(UNIT).gameObject;
            if (allUnits == null)
            {
                return;
            }

            GameObject unitToSpawn = (GameObject)Instantiate(GlobalAssets.GetVehiclePrefab(vehicleName), spawnPoint, startingOrientation);
            unitToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", vehicleName, PlayerId.ToString()));

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
        //	GameObject structureToSpawn = (GameObject)Instantiate(GameManager.ActiveInstance.GetStructurePrefab(structureName),
        //    	buildPoint, new Quaternion());

        //	constructionSite = structureToSpawn.GetComponent<Structure>();
        //	if (constructionSite != null)
        //	{
        //    	IsSettingConstructionPoint = true;
        //    	constructionSite.SetTransparencyMaterial(NotAllowedMaterial, true);
        //    	constructionSite.SetColliders(false);
        //    	constructionSite.playingArea = playingArea;
        //	}
        //	else
        //	{
        //    	Destroy(structureToSpawn);
        //	}
        //}

        //public void SetConstructionPoint()
        //{
        //	Vector3 constructionPosition = UserInput.GetHitPoint();
        //	constructionPosition.y = 0;
        //	constructionSite.transform.position = constructionPosition;
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
                case STRUCTURE:
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