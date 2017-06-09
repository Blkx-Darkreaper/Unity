using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Strikeforce
{
    public class Player : Entity
    {
        public int PlayerId { get { return this.playerControllerId; } }
        public bool IsNPC;
        public Team CurrentTeam { get; set; }
        protected Camera mainCamera;
        public Hud BuildHud;
        public Hud RaidHud;
        [HideInInspector]
        public Raider CurrentRaider;
        public string RaiderPrefabName = "Raider";
        [HideInInspector]
        public Checkpoint PreviousCheckpoint = null;
        [HideInInspector]
        public Inventory CurrentInventory;
        protected bool isInBuildMode = true;
        public GridCursor BuildCursor;
        public GridCursor BuyCursor;
        public LinkedList<Sector> Sectors { get; protected set; }
        public Selectable SelectedEntity { get; set; }
        public Color SelectionColour = Color.red;
        public bool IsSettingConstructionPoint = false;
        public bool IsSellingStructure = false;
        public Material NotAllowedMaterial, AllowedMaterial;
        public Color Colour;
        protected Color savedMaterialColour { get; set; }
        protected const string UNIT = "Unit";
        protected const string STRUCTURE = "Structure";
        protected LinkedList<Vehicle> allUnits { get; set; }
        protected LinkedList<Structure> allStructures { get; set; }
        protected float constructionProgress = 0f;

        protected override void Awake()
        {
            Profile profile = ProfileManager.Singleton.CurrentProfile;
            if (profile != null)
            {
                profile.Player = this;
            }

            this.CurrentTeam = null;

            // Get the main camera
            this.mainCamera = GameObject.FindGameObjectWithTag(Tags.MAIN_CAMERA).GetComponent<Camera>();
            this.CurrentInventory = GetComponent<Inventory>();
            this.Sectors = new LinkedList<Sector>();
            this.allUnits = new LinkedList<Vehicle>();
            this.allStructures = new LinkedList<Structure>();
            this.IsSettingConstructionPoint = false;
        }

        protected void Start()
        {
            Profile profile = ProfileManager.Singleton.CurrentProfile;
            if (profile == null)
            {
                return;
            }

            GameManager.Singleton.AddProfile(profile);
        }

        public override void OnStartLocalPlayer()
        {
            //CurrentRaider.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        public void StartGame()
        {
            this.CurrentLevel = CurrentTeam.HomeBase;

            this.BuildHud.enabled = isInBuildMode;
            this.RaidHud.enabled = !isInBuildMode;

            //SetCursors();
            SpawnRaider();  // Testing
        }

        protected void Update()
        {
            float timeElapsed = Time.deltaTime;
            this.constructionProgress += timeElapsed;
            int progress = Mathf.FloorToInt(constructionProgress);
            if (progress <= 0)
            {
                return;
            }

            constructionProgress -= progress;

            foreach (Structure structure in allStructures)
            {
                //Construct
                if (structure.IsConstructionComplete == false)
                {
                    structure.Construct(progress);
                }

                // Repair
                if (structure.IsRepairing == true)
                {
                    structure.Repair(progress);
                }
            }
        }

        protected void SetCursors()
        {
            // Get initial sector and spawnpoint
            Sector spawnSector = CurrentLevel.GetNextAvailableSector();
            Spawnpoint spawnpoint = spawnSector.Spawn;

            spawnSector.SetOwnership(this);
            SetCameraOverhead(spawnpoint.Location);

            // Get the cursors
            GridCursor[] cursors = GetComponentsInChildren<GridCursor>() as GridCursor[];
            if (cursors.Length != 2)
            {
                Debug.Log(String.Format("Failed to retrieve both cursors in player {0}", this.PlayerId));
                return;
            }

            this.BuildCursor = cursors[0];
            this.BuildCursor.Bounds = CurrentLevel.Bounds;

            this.BuyCursor = cursors[1];
            this.BuyCursor.Bounds = BuildHud.Bounds;

            BuildCursor.transform.position = new Vector3(spawnpoint.Location.x, spawnpoint.Location.y, spawnpoint.Location.z);
        }

        protected void SpawnRaider()	// Testing
        {
            this.isInBuildMode = false;
            this.BuildHud.enabled = isInBuildMode;
            this.RaidHud.enabled = !isInBuildMode;

            // Get spawn point from level
            Vector3 spawnLocation = CurrentLevel.GetRaiderSpawnLocation();

            // Spawn the raider at the spawnpoint
            GameObject raiderObject = Instantiate(
                GlobalAssets.GetVehiclePrefab(RaiderPrefabName),
                spawnLocation,
                Quaternion.identity) as GameObject;

            // Make the raider a child of the player
            raiderObject.transform.parent = gameObject.transform;

            this.CurrentRaider = raiderObject.GetComponent<Raider>();

            //NetworkServer.SpawnWithClientAuthority(raiderObject, connectionToClient);
            NetworkServer.SpawnWithClientAuthority(CurrentRaider.gameObject, gameObject);
            GameManager.Singleton.RegisterEntity(CurrentRaider);

            CurrentRaider.SetLayout(new Vector3[] {
                new Vector3(-0.25f, -0.25f, 0),
                new Vector3(-0.125f, -0.125f, 0),
                new Vector3(0, 0, 0),
                new Vector3(0.125f, 0, 0),
                new Vector3(0.25f, 0, 0)},
                new Hardpoint[] { new Hardpoint(-138, -69, 1, 1, HardpointPosition.LeftOuterWing) },
                new Hardpoint[] { new Hardpoint(-94, -16, 1, 1, HardpointPosition.LeftWing) },
                new Hardpoint[] { new Hardpoint(-22, 116, 1, 1, HardpointPosition.Center),
                    new Hardpoint(-22, 26, 1, 3, HardpointPosition.Center) },
                new Hardpoint[] { new Hardpoint(50, -16, 1, 1, HardpointPosition.RightWing) },
                new Hardpoint[] { new Hardpoint(94, -69, 1, 1, HardpointPosition.RightOuterWing) });

            GameObject basicShotPrefab = GlobalAssets.GetWeaponPrefab(Weapon.Types.BASIC_SHOT);
            Weapon basicShot1 = GameObject.Instantiate(basicShotPrefab).GetComponent<Weapon>() as Weapon;
            basicShot1.transform.parent = CurrentRaider.transform;
            GameManager.Singleton.RegisterEntity(basicShot1);

            Weapon basicShot2 = GameObject.Instantiate(basicShotPrefab).GetComponent<Weapon>() as Weapon;
            basicShot2.transform.parent = CurrentRaider.transform;
            GameManager.Singleton.RegisterEntity(basicShot2);

            GameObject boltPrefab = GlobalAssets.GetWeaponPrefab(Weapon.Types.BOLT);
            Weapon bolt = GameObject.Instantiate(boltPrefab).GetComponent<Weapon>() as Weapon;
            bolt.transform.parent = CurrentRaider.transform;
            GameManager.Singleton.RegisterEntity(bolt);

            bool equipped = CurrentRaider.EquipWeapon(basicShot1, HardpointPosition.Center, 0, 1, 0);
            equipped &= CurrentRaider.EquipWeapon(basicShot2, HardpointPosition.Center, 0, 2, 0);
            equipped &= CurrentRaider.EquipWeapon(bolt, HardpointPosition.Center, 0, 0, 0);
            if (equipped == true)
            {
                CurrentRaider.ReadyWeapons();
            }
            else
            {
                Debug.Log(String.Format("Failed to equip weapons to raider {0}", CurrentRaider.EntityId));
            }

            Vector3 raiderPosition = raiderObject.transform.position;
            SetCameraOverhead(raiderPosition);

            // Set raider and camera initial velocity
            float initialVelocity = CurrentRaider.StallSpeed;
            CurrentRaider.SetForwardVelocity(initialVelocity);

            Vector3 raiderVelocity = CurrentRaider.GetVelocity();
            SetCameraVelocity(raiderVelocity.x, raiderVelocity.y, raiderVelocity.z);

            // Make raider airbourne
            CurrentRaider.TakeOff();
            if (CurrentRaider.IsAirborne == false)
            {
                Debug.Log(String.Format("Raider {0} of player {1} failed to take off", CurrentRaider.EntityId, PlayerId));
            }
        }

        protected void SetCameraOverhead(Vector3 position)
        {
            Vector3 overheadView = new Vector3(position.x, position.y + 10, position.z);
            mainCamera.transform.position = overheadView;
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        public void LeftStick(float x, float y, float z)
        {
            MovePlayer(x, y, z);
        }

        public void RightStick(float x, float y, float z)
        {
            if (isInBuildMode == false)
            {
                return;
            }

            BuyCursor.Move(x, z);
        }

        public void DPad(float x, int y, float z)
        {
            if (isInBuildMode == false)
            {
                return;
            }

            if (z == 1)
            {
                DpadUp();
            }

            if (z == -1)
            {
                DpadDown();
            }
        }

        protected void MovePlayer(float x, float y, float z)
        {
            if (isInBuildMode == true)
            {
                BuildCursor.Move(x, z);
            }
            else
            {
                x *= 0.3f;
                z *= 0.3f;

                CurrentRaider.Move(x, z);
            }

            MoveMainCamera(x, y, z);
            //SetMainCameraPosition(cameraPosition);
        }

        protected void MoveMainCamera(float deltaX, float deltaY, float deltaZ)
        {
            Vector3 currentPosition = transform.position;

            KeepLevelInMainView(currentPosition.x, currentPosition.z, ref deltaX, ref deltaZ);

            mainCamera.transform.Translate(deltaX, deltaZ, deltaY);
        }

        protected void SetMainCameraPosition(float x, float z)
        {
            float y = mainCamera.transform.position.y;
            SetMainCameraPosition(new Vector3(x, y, z));
        }

        protected void SetMainCameraPosition(Vector3 position)
        {
            mainCamera.transform.position = new Vector3(position.x, position.y, position.z);
        }

        protected void SetMainCameraVelocity(float velocityX, float velocityY, float velocityZ)
        {
            Rigidbody cameraVelocity = mainCamera.GetComponent<Rigidbody>();
            if (cameraVelocity == null)
            {
                Debug.Log("Main camera has no rigidbody");
                return;
            }

            cameraVelocity.velocity = new Vector3(velocityX, velocityY, velocityZ);
        }

        protected RectangleF GetMainCameraViewBounds()
        {
            float x = mainCamera.transform.position.x;
            float y = mainCamera.transform.position.y;
            float height = 2 * mainCamera.orthographicSize;
            float width = height * Screen.width / Screen.height;

            RectangleF cameraBounds = new RectangleF(x, y, width, height);
            return cameraBounds;
        }

        protected void KeepLevelInMainView(float x, float y, ref float deltaX, ref float deltaY)
        {
            float finalX = x + deltaX;
            float finalY = y + deltaY;

            RectangleF mainCameraBounds = GetMainCameraViewBounds();
            float viewWidth = mainCameraBounds.Width;
            float viewHeight = mainCameraBounds.Height;

            int levelWidth = CurrentLevel.Width;
            int levelHeight = CurrentLevel.Height;

            float minX = (viewWidth - levelWidth) / 2f;
            float maxX = (levelWidth - viewWidth) / 2f;

            float minY = (viewHeight - levelHeight) / 2f;
            float maxY = (levelHeight - viewHeight) / 2f;

            deltaX = Mathf.Clamp(finalX, minX, maxX) - x;
            deltaY = Mathf.Clamp(finalY, minY, maxY) - y;
        }

        public void RespondToKeyEvent(KeyEvent keyEvent)
        {
            ActionKey key = keyEvent.Key;

            if (keyEvent.IsComplete == false)
            {
                Debug.Log(string.Format("{0} key {1} at {2}", key.ToString(), keyEvent.EventType.ToString(), Time.time.ToString()));
            }
            else
            {
                Debug.Log(string.Format("{0} key released at {1}", key.ToString(), Time.time.ToString()));
            }

            switch (key)
            {
                case ActionKey.Action1:
                    if (isInBuildMode == true)
                    {
                        Action1();
                    }
                    break;

                case ActionKey.Action2:
                    if (isInBuildMode == true)
                    {
                        Action2();
                    }
                    else
                    {
                        SetIsBoosting(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.Special1:
                    if (isInBuildMode == true)
                    {
                        Special1(keyEvent);
                    }
                    else
                    {
                        SetSpecialFiring(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.Special2:
                    if (isInBuildMode == true)
                    {
                        Special2();
                    }
                    else
                    {
                        SetEquipmentActive(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.DUp:
                    if (isInBuildMode == true)
                    {
                        DpadUp();
                    }
                    break;

                case ActionKey.DDown:
                    if (isInBuildMode == true)
                    {
                        DpadDown();
                    }
                    break;

                case ActionKey.RightTrigger:
                    if (isInBuildMode == true)
                    {
                    }
                    else
                    {
                        SetPrimaryFiring(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.LeftTrigger:
                    if (isInBuildMode == true)
                    {
                    }
                    else
                    {
                        SetSecondaryFiring(!keyEvent.IsComplete);
                    }
                    break;
            }
        }

        protected void Action1()
        {
            if (IsSellingStructure == true)
            {
                ConfirmSale();
                return;
            }

            if (SelectedEntity != null)
            {
                ShowOptions();
                return;
            }

            Select();
        }

        protected void Action2()
        {
            if (IsSellingStructure == true)
            {
                CancelSale();
                return;
            }

            if (SelectedEntity != null)
            {
                SellStructure();
                return;
            }

            ShowInfo();
        }

        protected void Special1(KeyEvent keyEvent)
        {
            if (SelectedEntity == null)
            {
                ToggleRepair(keyEvent.EventType, keyEvent.HoldDuration);
                return;
            }

            if (IsSettingConstructionPoint == true)
            {
                RotateStructure();
                return;
            }

            Scramble();
        }

        protected void Special2()
        {
            if (SelectedEntity == null)
            {
                BuyStructure();
                return;
            }

            if (IsSettingConstructionPoint == true)
            {
                ToggleContextOption();
                return;
            }

            Deselect();
        }

        protected void DpadUp()
        {
            if (SelectedEntity != null)
            {
                SetAlertStatus();
                return;
            }

            PageUp();
        }

        protected void DpadDown()
        {
            if (SelectedEntity != null)
            {
                SetPatrolStatus();
                return;
            }

            PageDown();
        }

        protected Structure GetStructureHighlightedByBuildCursor()
        {
            return GetHighlightedStructure(BuildCursor);
        }

        protected Structure GetStructureHighlightedByBuyCursor()
        {
            return GetHighlightedStructure(BuyCursor);
        }

        protected Structure GetHighlightedStructure(GridCursor cursor)
        {
            RaycastHit hit;
            int groundMask = GameManager.Singleton.GroundMask;

            if (Physics.Raycast(cursor.transform.position, Vector3.down, out hit, groundMask) == false)
            {
                return null;
            }

            Structure highlighted = hit.collider.GetComponentInParent<Structure>();
            return highlighted;
        }

        protected List<Structure> GetStructuresOnScreen()
        {
            List<Structure> allOwned = GetAllOwnedStructures();
            List<Structure> allOnScreen = new List<Structure>();

            foreach (Structure structure in allOwned)
            {
                Renderer renderer = structure.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                if (renderer.isVisible == false)
                {
                    continue;
                }

                allOnScreen.Add(structure);
            }

            return allOnScreen;
        }

        protected List<Structure> GetAllOwnedStructures()
        {
            List<Structure> allOwned = new List<Structure>(allStructures);
            return allOwned;
        }

        public void ActionFailed()
        {
            Debug.Log(string.Format("Action failed"));
        }

        protected void SetSelected(Selectable selectable)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.SetSelection(false);
                SelectedEntity.GetComponentInChildren<MeshRenderer>().material.color = savedMaterialColour;
            }

            this.SelectedEntity = selectable;
            if (selectable == null)
            {
                return;
            }

            selectable.SetSelection(true);
            Color selectedColour = selectable.GetComponentInChildren<MeshRenderer>().material.color;
            this.savedMaterialColour = selectedColour;

            selectedColour = SelectionColour;
        }

        protected void Select()
        {
            GameObject selected = GetStructureHighlightedByBuildCursor().gameObject;
            if (selected == null)
            {
                return;
            }

            Selectable selectable = selected.GetComponent<Selectable>();
            if (selectable == null)
            {
                return;
            }

            SetSelected(selectable);

            // Set build cursor size
            float width = selectable.transform.lossyScale.x;
            float length = selectable.transform.lossyScale.z;
            Vector2 size = new Vector2(width, length);
            BuildCursor.SetSize(size);
        }

        protected void Deselect()
        {
            SetSelected(null);

            BuildCursor.DefaultSize();
        }

        protected void ShowOptions()
        {
            throw new NotImplementedException();
        }

        protected void ConfirmSale()
        {
            Structure structure = SelectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            structure.Sell();
        }

        protected void ConfirmBuy()
        {
            Structure structure = SelectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            IsSettingConstructionPoint = false;

            GameManager.Singleton.RegisterEntity(structure);
            structure.Owner = this;
            structure.StartConstruction();
        }

        protected void ShowInfo()
        {

        }

        protected void SellStructure()
        {
            Structure structure = SelectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            IsSellingStructure = true;
        }

        protected void CancelSale()
        {
            IsSellingStructure = false;
        }

        protected void CancelBuy()
        {
            Structure structure = SelectedEntity.GetComponent<Structure>();
            int cost = structure.Cost;

            IsSettingConstructionPoint = false;
            CurrentInventory.UpdateResource(ResourceType.Money, cost);
            Deselect();

            structure.OnNetworkDestroy();
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

        protected void ToggleRepairing(Structure structure)
        {
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            structure.IsRepairing = !structure.IsRepairing;
        }

        protected void ToggleRepairHighlighted()
        {
            Structure highlighted = GetStructureHighlightedByBuildCursor();
            ToggleRepairing(highlighted);
        }

        protected void ToggleRepairOnScreen()
        {
            List<Structure> onScreen = GetStructuresOnScreen();
            foreach (Structure structure in onScreen)
            {
                ToggleRepairing(structure);
            }
        }

        protected void ToggleRepairAll()
        {
            List<Structure> owned = GetAllOwnedStructures();
            foreach (Structure structure in owned)
            {
                ToggleRepairing(structure);
            }
        }

        protected void Scramble()
        {
            Shelter shelter = SelectedEntity.GetComponent<Shelter>();
            if (shelter == null)
            {
                return;
            }

            shelter.ToggleScrambling();
        }

        protected void RotateStructure()
        {
            Structure structure = SelectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            structure.transform.Rotate(0, 90, 0);
        }

        protected void BuyStructure()
        {
            Structure structure = GetStructureHighlightedByBuyCursor();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            int cost = structure.Cost;

            bool hasSufficientFunds = CurrentInventory.HasSufficientResources(ResourceType.Money, cost);
            if (hasSufficientFunds == false)
            {
                // Insufficient funds
                ActionFailed();
                return;
            }

            Vector3 spawnPoint = BuildCursor.transform.position;
            GameObject gameObjectToBuild = Instantiate(structure.gameObject, spawnPoint, Quaternion.identity) as GameObject;
            if (gameObjectToBuild == null)
            {
                ActionFailed();
                return;
            }

            Selectable selectable = gameObjectToBuild.GetComponent<Selectable>();
            if (selectable == null)
            {
                ActionFailed();
                return;
            }

            SetSelected(selectable);
            IsSettingConstructionPoint = true;

            CurrentInventory.UpdateResource(ResourceType.Money, -cost);
        }

        protected void ToggleContextOption()
        {
            Structure structure = SelectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                return;
            }

            structure.ToggleContextOption();
        }

        protected void SetAlertStatus()
        {
            Shelter shelter = SelectedEntity.GetComponent<Shelter>();
            if (shelter == null)
            {
                ActionFailed();
                return;
            }

            shelter.SetPatrolStatus(false);
        }

        protected void PageUp()
        {
            throw new NotImplementedException();
            //mainCamera.transform.Translate(0, 0, screenHeight);
        }

        protected void SetPatrolStatus()
        {
            Shelter shelter = SelectedEntity.GetComponent<Shelter>();
            if (shelter == null)
            {
                ActionFailed();
                return;
            }

            shelter.SetPatrolStatus(true);
        }

        protected void PageDown()
        {
            throw new NotImplementedException();
        }

        protected void SetIsBoosting(bool isBoosting)
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

            raider.SetIsBoosting(isBoosting);
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

        protected void SetSecondaryFiring(bool isFiring)
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

            raider.SetSecondaryFire(isFiring);
        }

        protected void SetSpecialFiring(bool isFiring)
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

            raider.SetSpecialFire(isFiring);
        }

        protected void SetEquipmentActive(bool isActive)
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

            raider.SetEquipmentActive(isActive);
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

        public void SpawnUnit(string vehicleName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation)
        {
            GameObject allUnits = transform.Find(UNIT).gameObject;
            if (allUnits == null)
            {
                return;
            }
            //Units allUnits = GetComponentInChildren<Units>();
            GameObject vehicleToSpawn = (GameObject)Instantiate(GlobalAssets.GetVehiclePrefab(vehicleName), spawnPoint, startingOrientation);
            vehicleToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", vehicleName, PlayerId));

            if (rallyPoint == GlobalAssets.InvalidLocation)
            {
                return;
            }
            if (spawnPoint == rallyPoint)
            {
                return;
            }

            Selectable selectable = vehicleToSpawn.GetComponent<Selectable>();
            if (selectable == null)
            {
                return;
            }

            selectable.SetWaypoint(rallyPoint);
        }

        public void SpawnUnit(string vehicleName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion startingOrientation, Structure spawner)
        {
            GameObject allUnits = transform.Find(UNIT).gameObject;
            if (allUnits == null)
            {
                return;
            }

            GameObject vehicleToSpawn = (GameObject)Instantiate(GlobalAssets.GetVehiclePrefab(vehicleName), spawnPoint, startingOrientation);
            vehicleToSpawn.transform.parent = allUnits.transform;
            Debug.Log(string.Format("Spawned {0} for player {1}", vehicleName, PlayerId.ToString()));

            if (rallyPoint == GlobalAssets.InvalidLocation)
            {
                return;
            }

            Selectable selectable = vehicleToSpawn.GetComponent<Selectable>();
            if (selectable == null)
            {
                return;
            }

            selectable.SetSpawner(spawner);

            if (spawnPoint == rallyPoint)
            {
                return;
            }

            selectable.SetWaypoint(rallyPoint);
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