using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

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
        public GridCursor BuildCursor;
        public GridCursor BuyCursor;
        public LinkedList<Sector> Sectors { get; protected set; }
        public Selectable SelectedEntity { get; set; }
        public bool IsSettingConstructionPoint;
        public Material NotAllowedMaterial, AllowedMaterial;
        public Color Colour;
        protected Color savedMaterialColour { get; set; }
        protected const string UNIT = "Unit";
        protected const string STRUCTURE = "Structure";
        protected LinkedList<Vehicle> allUnits { get; set; }
        protected LinkedList<Structure> allStructures { get; set; }

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
            	new Vector3(-2, 0, 0),
            	new Vector3(-1, 0, 0),
            	new Vector3(0, 0, 0),
            	new Vector3(1, 0, 0),
            	new Vector3(2, 0, 0)},
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

            NetworkServer.SpawnWithClientAuthority(CurrentRaider.gameObject, gameObject);

            // Set camera overhead
            Vector3 overheadView = new Vector3(raiderPosition.x, raiderPosition.y + 10, raiderPosition.z);
            mainCamera.transform.position = overheadView;
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        public void LeftStick(float x, float y, float z)
        {
            MovePlayer(x, y, z);
        }

        public void RightStick(float x, float y, float z) {
            if(isInBuildMode == false)
            {
                return;
            }

            BuyCursor.Move(x, z);
        }

        protected void MovePlayer(float x, float y, float z)
        {
            isInBuildMode = false;    //testing
            Vector3 cameraPosition = Vector3.zero;
            if (isInBuildMode == true)
            {
                BuildCursor.Move(x, z);
            }
            else
            {
                x *= 0.3f;
                z *= 0.3f;

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
                        Select();
                    }
                    break;

                case ActionKey.Action2:
                    if(isInBuildMode == true)
                    {
                        ShowInfo();
                    }
                    break;

                case ActionKey.Special1:
                    if(isInBuildMode == true)
                    {
                        ToggleRepair(keyEvent.EventType, keyEvent.HoldDuration);
                    }
                    break;

                case ActionKey.Special2:
                    if(isInBuildMode == true)
                    {
                        Purchase();
                    }
                    break;

                case ActionKey.DUp:
                    if(isInBuildMode == true)
                    {
                        PageUp();
                    }
                    break;

                case ActionKey.DDown:
                    if (isInBuildMode == true)
                    {
                        PageDown();
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
                            SetPrimaryFiring(false);
                            break;
                        }

                        SetPrimaryFiring(true);
                    }
                    break;
            }
        }

        protected GameObject GetHighlightedByBuildCursor()
        {
            return GetHighlighted(BuildCursor);
        }

        protected GameObject GetHighlightedByBuyCursor()
        {
            return GetHighlighted(BuyCursor);
        }

        protected GameObject GetHighlighted(GridCursor cursor)
        {
            Ray ray = camera.main.ScreenPointToRay(cursor.transform);
            RaycastHit hit;

            if(Physics.Raycast(ray.origin, ray.direction, hit, 100) == false)
            {
                return null;
            }

            if(hit.collider.tag != "Object")
            {
                return null;
            }

            GameObject gameObject = hit.collider.gameObject;
            return gameObject;
        }

        protected List<GameObject> GetOnScreen()
        {

        }

        protected List<GameObject> GetAllOwned()
        {

        }

        public void ActionFailed()
        {

        }

        protected void SetSelected(Selectable selectable)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.SetSelection(false);
                SelectedEntity.GetComponentInChildren<MeshRenderer>().material.color = savedMaterialColour;
            }

            this.SelectedEntity = selectable;
            if(selectable == null)
            {
                return;
            }

            selectable.SetSelection(true);
            Color selectedColour = selectable.GetComponentInChildren<MeshRenderer>().material.color;
            this.savedMaterialColour = selectedColour;

            selectedColour = Color.red;
        }

        protected void Select()
        {
            GameObject selected = GetHighlightedByBuildCursor();
            if (selected == null)
            {
                return;
            }

            Selectable selectable = selected.getComponent<Selectable>();
            if (selectable == null)
            {
                return;
            }

            SetSelected(selectable);
        }

        protected void ShowInfo()
        {

        }

        protected void ToggleRepair(KeyEvent.Type eventType, float holdDuration)
        {
            switch (eventType)
            {
                case KeyEvent.Type.Pressed:
                    ToggleRepairHighlighted();
                    break;

                case KeyEvent.Type.DoubleTapped:
                    ToggleRepairOnScreen();
                    break;

                case KeyEvent.Type.Held:
                    ToggleRepairAll();
                    break;
            }
        }

        protected void ToggleRepairing(GameObject gameObject)
        {
            if (gameObject == null)
            {
                ActionFailed();
                return;
            }

            Structure structure = gameObject.getComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            structure.IsRepairing = !structure.IsRepairing;
        }

        protected void ToggleRepairHighlighted()
        {
            GameObject highlighted = GetHighlightedByBuildCursor();
            ToggleRepairing(highlighted);
        }

        protected void ToggleRepairOnScreen()
        {
            List<GameObject> onScreen = GetOnScreen();
            foreach(GameObject gameObject in onScreen)
            {
                ToggleRepairing(gameObject);
            }
        }

        protected void ToggleRepairAll()
        {
            List<GameObject> owned = GetAllOwned();
            foreach (GameObject gameObject in owned)
            {
                ToggleRepairing(gameObject);
            }
        }

        protected void Purchase()
        {
            GameObject gameObject = GetHighlightedByBuyCursor();
            if(gameObject == null)
            {
                ActionFailed();
                return;
            }

            Structure structure = gameObject.GetComponent<Structure>();
            if(structure == null)
            {
                ActionFailed();
                return;
            }

            int cost = structure.Cost;

            bool hasSufficientFunds = CurrentInventory.HasSufficientResources(ResourceType.Money, cost);
            if(hasSufficientFunds == false)
            {
                ActionFailed();
                return;
            }

            Vector3 spawnPoint = BuildCursor.transform;
            GameObject gameObjectToBuild = Instantiate(gameObject, spawnPoint, Quaternion.Identity) as GameObject;
            if(gameObjectToBuild == null)
            {
                ActionFailed();
                return;
            }

            Selectable selectable = gameObjectToBuild.GetComponent<Selectable>();
            if(selectable == null)
            {
                ActionFailed();
                return;
            }

            SetSelected(selectable);
            IsSettingConstructionPoint = true;
        }

        protected void PageUp()
        {

        }

        protected void PageDown()
        {

        }

        protected void SetPrimaryFiring(bool isFiring)
        {
            if (isLocalPlayer == false)
            {
                return;
            }

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