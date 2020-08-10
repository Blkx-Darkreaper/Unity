using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Structure : Destructible
    {
        protected Queue<string> buildQueue;
        protected float currentBuildProgress = 0f;
        protected bool contextOptionSelected = false;
        public float constructionTime;
        protected float currentConstructionProgress = 0f;
        public bool isConstructionComplete { get; protected set; }
        protected const string CONSTRUCTION_MESSAGE = "Building...";
        public bool isRepairing = false;
        public int repairCost;
        public bool isDamaged { get{
                int damageThreshold = 2 * maxHitPoints / 3;
                return currentHitPoints > damageThreshold;
        } }
        protected Vector3 spawnPoint;
        public Vector3 rallyPoint { get; protected set; }
        public Texture2D rallyPointIcon;
        public Texture2D sellIcon;
        protected struct StructureProperties
        {
            public const string IS_CONSTRUCTION_COMPLETE = "IsConstructionComplete";
            public const string SPAWN_POINT = "SpawnPoint";
            public const string RALLY_POINT = "RallyPoint";
            public const string BUILD_PROGRESS = "BuildProgress";
            public const string BUILD_QUEUE = "BuildQueue";
        }

        protected override void Awake()
        {
            base.Awake();
            this.buildQueue = new Queue<string>();
            float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
            float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
            this.spawnPoint = new Vector3(spawnX, 0f, spawnZ);
            this.rallyPoint = spawnPoint;
            this.isConstructionComplete = false;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected virtual void Update()
        {
            Build();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (isConstructionComplete == true)
            {
                return;
            }

            DrawConstructionProgress();
        }

        [Client]
        private void DrawConstructionProgress()
        {
            GUI.skin = GlobalAssets.SelectionBoxSkin;
            Rect selectionBox = GameManager.CalculateSelectionBox(selectionBounds, playingArea);

            // Draw the selection box around the currently selected object, within the bounds of the main draw area
            GUI.BeginGroup(playingArea);

            UpdateHealthPercentage(0.99f, 0.5f);
            DrawHealthBarWithLabel(selectionBox, CONSTRUCTION_MESSAGE);

            GUI.EndGroup();
        }

        public int GetTotalRepairCost()
        {
            int damage = maxHitPoints - currentHitPoints;

            int repairCost = Math.Max(1, cost / maxHitPoints);

            int totalCost = repairCost * damage;
            return totalCost;
        }

        [Server]
        protected void AddVehicleToBuildQueue(string unitName)
        {
            GameObject unitGameObject = GlobalAssets.GetVehiclePrefab(unitName);
            Vehicle vehicle = unitGameObject.GetComponent<Vehicle>();
            if (vehicle == null)
            {
                return;
            }

            if (currentOwner != null)
            {
                bool sufficientFunds = currentOwner.buildMode.currentInventory.HasSufficientResources(ResourceType.Money, vehicle.cost);
                if (sufficientFunds == false)
                {
                    currentOwner.buildMode.currentInventory.InsufficientResources(ResourceType.Money);
                    return;
                }

                currentOwner.buildMode.currentInventory.UpdateResource(ResourceType.Money, -vehicle.cost);
            }

            buildQueue.Enqueue(unitName);
            this.buildTime = vehicle.buildTime;
        }

        [Server]
        protected void Build()
        {
            if (buildQueue.Count <= 0)
            {
                return;
            }

            currentBuildProgress += Time.deltaTime;
            if (currentBuildProgress < buildTime)
            {
                return;
            }
            if (currentOwner == null)
            {
                return;
            }

            string unitName = buildQueue.Dequeue();
            currentOwner.buildMode.SpawnUnit(unitName, spawnPoint, rallyPoint, transform.rotation, this);
            currentBuildProgress = 0f;
        }

        public string[] GetBuildQueueEntries()
        {
            string[] entries = buildQueue.ToArray();
            return entries;
        }

        public float GetBuildCompletionPercentage()
        {
            if(buildQueue.Count == 0)
            {
                return 0f;
            }

            float completionPercentage = currentBuildProgress / buildTime;
            return completionPercentage;
        }

        public override void SetSelection(bool selected)
        {
            base.SetSelection(selected);

            if (currentOwner == null)
            {
                return;
            }
            if (currentOwner.isNPC == true)
            {
                return;
            }

            RallyPoint flag = GetComponentInChildren<RallyPoint>();
            if (flag == null)
            {
                return;
            }

            if (selected == true)
            {
                if (spawnPoint == GlobalAssets.InvalidLocation)
                {
                    return;
                }
                if (rallyPoint == GlobalAssets.InvalidLocation)
                {
                    return;
                }

                flag.transform.localPosition = rallyPoint;
                flag.transform.forward = transform.forward;
                flag.Enable();
            }
            else
            {
                flag.Disable();
            }
        }

        public override void MouseClick(GameObject hitEntity, Vector3 hitPoint, Player player)
        {
            base.MouseClick(hitEntity, hitPoint, player);

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

            bool isGround = hitEntity.CompareTag(Tags.GROUND);
            if (isGround == false)
            {
                return;
            }

            if (hitPoint == GlobalAssets.InvalidLocation)
            {
                return;
            }

            float x = hitPoint.x;
            float y = hitPoint.y + player.buildMode.selectedEntity.transform.position.y;  // Ensures rallyPoint stays on top of the surface it is on
            float z = hitPoint.z;
            Vector3 pointClicked = new Vector3(x, y, z);
            UpdateRallyPointPosition(pointClicked);
        }

        private void UpdateRallyPointPosition(Vector3 updatedPoint)
        {
            rallyPoint = updatedPoint;

            RallyPoint flag = GetComponentInChildren<RallyPoint>();
            if (flag == null)
            {
                return;
            }
            flag.transform.localPosition = updatedPoint;
            flag.transform.forward = transform.forward;
        }

        public bool HasValidSpawnPoint()
        {
            bool hasSpawnPoint = spawnPoint != GlobalAssets.InvalidLocation;
            bool hasRallyPoint = rallyPoint != GlobalAssets.InvalidLocation;

            return hasSpawnPoint && hasRallyPoint;
        }

        public override void SetHoverState(GameObject entityUnderMouse)
        {
            base.SetHoverState(entityUnderMouse);

            if (currentOwner == null)
            {
                return;
            }
            if (currentOwner.isNPC == true)
            {
                return;
            }
            if (isSelected == false)
            {
                return;
            }

            bool isGround = CompareTag(Tags.GROUND);
            if (isGround == false)
            {
                return;
            }

            CursorState previousState = currentOwner.buildMode.hud.PreviousCursorState;
            if (previousState != CursorState.rallyPoint)
            {
                return;
            }

            currentOwner.buildMode.hud.SetCursorState(CursorState.rallyPoint);
        }

        [Server]
        public void Sell()
        {
            if (currentOwner == null)
            {
                return;
            }

            currentOwner.buildMode.currentInventory.UpdateResource(ResourceType.Money, sellValue);

            if (isSelected == true)
            {
                SetSelection(false);
            }

            GameEntityManager.singleton.RemoveEntity(this);
        }

        public virtual void ToggleContextOption()
        {
            contextOptionSelected = !contextOptionSelected;
        }

        [Server]
        public void StartConstruction()
        {
            UpdateBounds();
            this.isConstructionComplete = false;
            this.currentHitPoints = 0;
        }

        [Server]
        public void CancelConstruction()
        {
            if (currentOwner == null)
            {
                return;
            }

            if (isSelected == true)
            {
                SetSelection(false);
            }

            GameEntityManager.singleton.RemoveEntity(this);
        }

        [Server]
        public void Construct(int amount)
        {
            this.currentHitPoints += amount;

            if (currentHitPoints < maxHitPoints)
            {
                return;
            }

            this.currentHitPoints = maxHitPoints;
            this.isConstructionComplete = true;
            RestoreMaterials();
            SetTeamColour();
        }

        [Server]
        public void Repair(int amount)
        {
            if (isRepairing == false)
            {
                return;
            }

            // Check if there are sufficient funds
            int fundsRequired = amount / 2 * repairCost;
            bool sufficientFunds = currentOwner.buildMode.currentInventory.HasSufficientResources(ResourceType.Money, fundsRequired);
            if (sufficientFunds == false)
            {
                this.isRepairing = false;
                return;
            }

            currentOwner.buildMode.currentInventory.UpdateResource(ResourceType.Money, -fundsRequired);

            this.currentHitPoints += amount;

            if (currentHitPoints < maxHitPoints)
            {
                return;
            }

            this.currentHitPoints = maxHitPoints;
            this.isRepairing = false;
        }

        [Server]
        public override void TakeDamage(int amount, RaycastHit hit)
        {
            base.TakeDamage(amount, hit);
        }
    }
}