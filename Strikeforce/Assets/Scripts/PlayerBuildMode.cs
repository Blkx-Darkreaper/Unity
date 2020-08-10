using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Color = UnityEngine.Color;

namespace Strikeforce
{
    public class PlayerBuildMode : MonoBehaviour
    {
        protected Player parent { get; set; }
        public Hud hud;
        public GridCursor buildCursor;
        //public GridCursor BuyCursor;
        protected float constructionProgress = 0f;
        protected const string UNIT = "Unit";
        protected const string STRUCTURE = "Structure";
        protected LinkedList<Vehicle> allUnits { get; set; }
        protected LinkedList<Structure> allStructures { get; set; }
        [HideInInspector]
        public Inventory currentInventory;
        public LinkedList<Sector> sectors { get; protected set; }
        public Selectable selectedEntity { get; set; }
        public Color selectionColour = Color.red;
        public bool isSettingConstructionPoint = false;
        public bool isSellingStructure = false;
        public Material notAllowedMaterial, allowedMaterial;
        public Color colour;
        protected Color savedMaterialColour { get; set; }

        protected void Awake()
        {
            this.parent = GetComponent<Player>();

            this.buildCursor = Instantiate(buildCursor, this.gameObject.transform).GetComponent<GridCursor>();
            this.hud = Instantiate(hud, this.gameObject.transform).GetComponent<Hud>();

            this.sectors = new LinkedList<Sector>();
            this.allUnits = new LinkedList<Vehicle>();
            this.allStructures = new LinkedList<Structure>();
            this.isSettingConstructionPoint = false;

            this.currentInventory = ScriptableObject.CreateInstance(typeof(Inventory)) as Inventory;
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

            this.constructionProgress -= progress;

            foreach (Structure structure in allStructures)
            {
                //Construct
                if (structure.isConstructionComplete == false)
                {
                    structure.Construct(progress);
                }

                // Repair
                if (structure.isRepairing == true)
                {
                    structure.Repair(progress);
                }
            }
        }

        public void ActionFailed()
        {
            Debug.Log(string.Format("Action failed"));
        }

        public void Action1()
        {
            if (MenuManager.Singleton.IsMenuOpen == true)
            {
                HandleMenuButtonClick();
                return;
            }

            if (isSellingStructure == true)
            {
                ConfirmSale();
                return;
            }

            if (selectedEntity != null)
            {
                ShowOptions();
                return;
            }

            Select();
        }

        protected void HandleMenuButtonClick()
        {
            Menu currentMenu = MenuManager.Singleton.CurrentMenu;
            currentMenu.MenuButtonClick();
        }

        public void Action2()
        {
            if (isSellingStructure == true)
            {
                CancelSale();
                return;
            }

            if (selectedEntity != null)
            {
                SellStructure();
                return;
            }

            ShowInfo();
        }

        public void Special1(KeyEvent keyEvent)
        {
            if (selectedEntity == null)
            {
                ToggleRepair(keyEvent.EventType, keyEvent.HoldDuration);
                return;
            }

            if (isSettingConstructionPoint == true)
            {
                RotateStructure();
                return;
            }

            Scramble();
        }

        public void Special2()
        {
            if (selectedEntity == null)
            {
                //BuyStructure();
                return;
            }

            if (isSettingConstructionPoint == true)
            {
                ToggleContextOption();
                return;
            }

            Deselect();
        }

        protected Structure GetStructureHighlightedByBuildCursor()
        {
            return GetHighlightedStructure(buildCursor);
        }

        //protected Structure GetStructureHighlightedByBuyCursor()
        //{
        //    return GetHighlightedStructure(BuyCursor);
        //}

        protected Structure GetHighlightedStructure(GridCursor cursor)
        {
            RaycastHit hit;
            int groundMask = GameManager.singleton.groundMask;

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

        protected void SetSelected(Selectable selectable)
        {
            if (selectedEntity != null)
            {
                selectedEntity.SetSelection(false);
                selectedEntity.GetComponentInChildren<MeshRenderer>().material.color = savedMaterialColour;
            }

            this.selectedEntity = selectable;
            if (selectable == null)
            {
                return;
            }

            selectable.SetSelection(true);
            Color selectedColour = selectable.GetComponentInChildren<MeshRenderer>().material.color;
            this.savedMaterialColour = selectedColour;

            selectedColour = selectionColour;
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
            buildCursor.ExpandAndCenterAroundSelectable(selectable);
        }

        protected void Deselect()
        {
            SetSelected(null);

            buildCursor.DefaultSize();
        }

        public void ShowOptions()
        {
            throw new NotImplementedException();
        }

        public void ConfirmSale()
        {
            Structure structure = selectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            structure.Sell();
        }

        public void ConfirmBuy()
        {
            Structure structure = selectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            isSettingConstructionPoint = false;

            GameEntityManager.singleton.RegisterEntity(structure);
            structure.currentOwner = parent;
            structure.StartConstruction();
        }

        public void ShowInfo()
        {

        }

        public void SellStructure()
        {
            Structure structure = selectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            this.isSellingStructure = true;
        }

        public void CancelSale()
        {
            this.isSellingStructure = false;
        }

        public void CancelBuy()
        {
            Structure structure = selectedEntity.GetComponent<Structure>();
            int cost = structure.cost;

            isSettingConstructionPoint = false;
            currentInventory.UpdateResource(ResourceType.Money, cost);
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

            structure.isRepairing = !structure.isRepairing;
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

        public void Scramble()
        {
            Shelter shelter = selectedEntity.GetComponent<Shelter>();
            if (shelter == null)
            {
                return;
            }

            shelter.ToggleScrambling();
        }

        public void RotateStructure()
        {
            Structure structure = selectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                ActionFailed();
                return;
            }

            structure.transform.Rotate(0, 90, 0);
        }

        //protected void BuyStructure()
        //{
        //    Structure structure = GetStructureHighlightedByBuyCursor();
        //    if (structure == null)
        //    {
        //        ActionFailed();
        //        return;
        //    }

        //    int cost = structure.Cost;

        //    bool hasSufficientFunds = CurrentInventory.HasSufficientResources(ResourceType.Money, cost);
        //    if (hasSufficientFunds == false)
        //    {
        //        // Insufficient funds
        //        ActionFailed();
        //        return;
        //    }

        //    Vector3 spawnPoint = BuildCursor.transform.position;
        //    GameObject gameObjectToBuild = Instantiate(structure.gameObject, spawnPoint, Quaternion.identity) as GameObject;
        //    if (gameObjectToBuild == null)
        //    {
        //        ActionFailed();
        //        return;
        //    }

        //    Selectable selectable = gameObjectToBuild.GetComponent<Selectable>();
        //    if (selectable == null)
        //    {
        //        ActionFailed();
        //        return;
        //    }

        //    SetSelected(selectable);
        //    IsSettingConstructionPoint = true;

        //    CurrentInventory.UpdateResource(ResourceType.Money, -cost);
        //}

        protected void ToggleContextOption()
        {
            Structure structure = selectedEntity.GetComponent<Structure>();
            if (structure == null)
            {
                return;
            }

            structure.ToggleContextOption();
        }

        public void DpadUp()
        {
            if (selectedEntity != null)
            {
                this.SetAlertStatus();
                return;
            }

            this.PageUp();
        }

        public void DpadDown()
        {
            if (selectedEntity != null)
            {
                this.SetPatrolStatus();
                return;
            }

            this.PageDown();
        }

        protected void SetAlertStatus()
        {
            Shelter shelter = selectedEntity.GetComponent<Shelter>();
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
            Shelter shelter = selectedEntity.GetComponent<Shelter>();
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

        public bool IsConstructionSiteValid(Structure constructionSite)
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

                Selectable hitEntity = hitGameObject.GetComponentInParent<Selectable>();
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
            Debug.Log(string.Format("Spawned {0} for player {1}", vehicleName, parent.playerId));

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

            _Targeting targeting = gameObject.GetComponent<_Targeting>();
            if (targeting == null)
            {
                Debug.Log(string.Format("No targeting on {0} {1}. Unable to attack", selectable.name, selectable.entityId));
                return;
            }

            targeting.SetWaypoint(rallyPoint);
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
            Debug.Log(string.Format("Spawned {0} for player {1}", vehicleName, parent.playerId.ToString()));

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

            _Targeting targeting = gameObject.GetComponent<_Targeting>();
            if (targeting == null)
            {
                Debug.Log(string.Format("No targeting on {0} {1}. Unable to attack", selectable.name, selectable.entityId));
                return;
            }

            targeting.SetWaypoint(rallyPoint);
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
                    if (structure.isConstructionComplete == false)
                    {
                        structure.SetTransparencyMaterial(allowedMaterial, true);
                    }
                    break;
            }
        }
    }
}