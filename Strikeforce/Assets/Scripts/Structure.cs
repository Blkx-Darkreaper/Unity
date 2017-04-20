using UnityEngine;
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
        public float ConstructionTime;
        protected float currentConstructionProgress = 0f;
        public bool IsConstructionComplete { get; protected set; }
        protected const string CONSTRUCTION_MESSAGE = "Building...";
        public bool IsRepairing = false;
        public int RepairCost;
        public bool IsDamaged { get; protected set; }
        protected Vector3 spawnPoint;
        protected Vector3 rallyPoint;
        public Texture2D RallyPointIcon;
        public Texture2D SellIcon;
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
            float spawnX = SelectionBounds.center.x + transform.forward.x * SelectionBounds.extents.x + transform.forward.x * 10;
            float spawnZ = SelectionBounds.center.z + transform.forward.z * SelectionBounds.extents.z + transform.forward.z * 10;
            this.spawnPoint = new Vector3(spawnX, 0f, spawnZ);
            this.rallyPoint = spawnPoint;
            this.IsConstructionComplete = false;
            this.IsDamaged = false;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
            Build();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (IsConstructionComplete == true)
            {
                return;
            }

            DrawConstructionProgress();
        }

        private void DrawConstructionProgress()
        {
            GUI.skin = GlobalAssets.SelectionBoxSkin;
            Rect selectionBox = GameManager.CalculateSelectionBox(SelectionBounds, playingArea);

            // Draw the selection box around the currently selected object, within the bounds of the main draw area
            GUI.BeginGroup(playingArea);

            UpdateHealthPercentage(0.99f, 0.5f);
            DrawHealthBarWithLabel(selectionBox, CONSTRUCTION_MESSAGE);

            GUI.EndGroup();
        }

        public int GetTotalRepairCost()
        {
            int damage = MaxHitPoints - CurrentHitPoints;

            int repairCost = Math.Max(1, Cost / MaxHitPoints);

            int totalCost = repairCost * damage;
            return totalCost;
        }

        protected void AddVehicleToBuildQueue(string unitName)
        {
            GameObject unitGameObject = GlobalAssets.GetVehiclePrefab(unitName);
            Selectable unit = unitGameObject.GetComponent<Selectable>();
            if (Owner != null)
            {
                if (unit == null)
                {
                    return;
                }

                bool sufficientFunds = Owner.CurrentInventory.HasSufficientResources(ResourceType.Money, unit.Cost);
                if (sufficientFunds == false)
                {
                    Owner.CurrentInventory.InsufficientResources(ResourceType.Money);
                    return;
                }

                Owner.CurrentInventory.UpdateResource(ResourceType.Money, -unit.Cost);
            }

            buildQueue.Enqueue(unitName);
        }

        protected void Build()
        {
            if (buildQueue.Count <= 0)
            {
                return;
            }

            currentBuildProgress += Time.deltaTime * GlobalAssets.BuildSpeed;
            if (currentBuildProgress < BuildTime)
            {
                return;
            }
            if (Owner == null)
            {
                return;
            }

            string unitName = buildQueue.Dequeue();
            Owner.SpawnUnit(unitName, spawnPoint, rallyPoint, transform.rotation, this);
            currentBuildProgress = 0f;
        }

        public string[] GetBuildQueueEntries()
        {
            string[] entries = buildQueue.ToArray();
            return entries;
        }

        public float GetBuildCompletionPercentage()
        {
            float completionPercentage = currentBuildProgress / BuildTime;
            return completionPercentage;
        }

        public override void SetSelection(bool selected)
        {
            base.SetSelection(selected);

            if (Owner == null)
            {
                return;
            }
            if (Owner.IsNPC == true)
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
                if (spawnPoint == GlobalAssets.InvalidPoint)
                {
                    return;
                }
                if (rallyPoint == GlobalAssets.InvalidPoint)
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
            if (player.IsNPC == true)
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

            if (hitPoint == GlobalAssets.InvalidPoint)
            {
                return;
            }

            float x = hitPoint.x;
            float y = hitPoint.y + player.SelectedEntity.transform.position.y;  // Ensures rallyPoint stays on top of the surface it is on
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
            bool hasSpawnPoint = spawnPoint != GlobalAssets.InvalidPoint;
            bool hasRallyPoint = rallyPoint != GlobalAssets.InvalidPoint;

            return hasSpawnPoint && hasRallyPoint;
        }

        public override void SetHoverState(GameObject entityUnderMouse)
        {
            base.SetHoverState(entityUnderMouse);

            if (Owner == null)
            {
                return;
            }
            if (Owner.IsNPC == true)
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

            CursorState previousState = Owner.PlayerHud.PreviousCursorState;
            if (previousState != CursorState.rallyPoint)
            {
                return;
            }

            Owner.PlayerHud.SetCursorState(CursorState.rallyPoint);
        }

        public void Sell()
        {
            if (Owner == null)
            {
                return;
            }

            Owner.CurrentInventory.UpdateResource(ResourceType.Money, SellValue);

            if (isSelected == true)
            {
                SetSelection(false);
            }

            GameManager.Singleton.RemoveEntity(this);
        }

        public virtual void ToggleContextOption()
        {
            contextOptionSelected = !contextOptionSelected;
        }

        public void StartConstruction()
        {
            UpdateBounds();
            IsConstructionComplete = false;
            CurrentHitPoints = 0;
        }

        public void Construct(int amount)
        {
            CurrentHitPoints += amount;

            if (CurrentHitPoints < MaxHitPoints)
            {
                return;
            }

            CurrentHitPoints = MaxHitPoints;
            IsConstructionComplete = true;
            RestoreMaterials();
            SetTeamColour();
        }

        public void Repair(int amount)
        {
            if (IsRepairing == false)
            {
                return;
            }

            // Check if there are sufficient funds
            int fundsRequired = amount / 2 * RepairCost;
            bool sufficientFunds = Owner.CurrentInventory.HasSufficientResources(ResourceType.Money, fundsRequired);
            if (sufficientFunds == false)
            {
                IsRepairing = false;
                return;
            }

            Owner.CurrentInventory.UpdateResource(ResourceType.Money, -fundsRequired);

            CurrentHitPoints += amount;

            SetIsDamaged();

            if (CurrentHitPoints < MaxHitPoints)
            {
                return;
            }

            CurrentHitPoints = MaxHitPoints;
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            SetIsDamaged();
        }

        public void SetIsDamaged()
        {
            int damageThreshold = 2 * MaxHitPoints / 3;

            this.IsDamaged = CurrentHitPoints > damageThreshold;
        }
    }
}