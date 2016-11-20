using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Selectable : Entity
    {
        public Texture2D BuildImage { get; set; }
        public float BuildTime { get; set; }
        public int Cost { get; set; }
        public int SellValue { get; set; }
        public Bounds SelectionBounds { get; protected set; }
        public Player Owner { get; set; }
        public string[] Actions { get; set; }
        public float WeaponRange = 10f;
        private const float DEFAULT_WEAPON_RANGE = 10f;
        public float WeaponAimSpeed = 1f;
        private const float DEFAULT_WEAPON_AIM_SPEED = 1f;
        public float WeaponCooldown = 1f;
        private const float DEFAULT_WEAPON_COOLDOWN = 1f;
        protected float currentCooldownRemaining { get; set; }
        protected bool isSelected { get; set; }
        protected bool isAttacking { get; set; }
        protected bool isAdvancing { get; set; }
        protected bool isAiming { get; set; }
        protected Entity actionTarget { get; set; }
        protected int actionTargetId { get; set; }
        protected struct SelectableProperties
        {
            public const string IS_ATTACKING = "IsAttacking";
            public const string IS_ADVANCING = "IsAdvancing";
            public const string IS_AIMING = "IsAiming";
            public const string COOLDOWN = "CurrentCooldown";
            public const string TARGET_ID = "TargetId";
        }
        private List<Material> oldMaterials { get; set; }

        protected override void Awake()
        {
            base.Awake();

            SelectionBounds = GlobalAssets.InvalidBounds;
            //UpdateBounds();
        }

        protected virtual void Start()
        {
            SetOwnership();

            if (isLoadedFromSave == true)
            {
                LoadAttackTarget(actionTargetId);
                return;
            }

            SetWeaponDefaults();
        }

        protected virtual void Update()
        {
            currentCooldownRemaining -= Time.deltaTime;
            currentCooldownRemaining = Mathf.Clamp(currentCooldownRemaining, 0f, float.MaxValue);

            if (isAttacking == false)
            {
                return;
            }
            if (isAdvancing == true)
            {
                return;
            }
            if (isAiming == true)
            {
                return;
            }

            AttackTarget(actionTarget);
        }

        protected virtual void OnGUI()
        {
            if (isSelected == false)
            {
                return;
            }
            if (GameManager.Singleton.IsMenuOpen)
            {
                return;
            }

            DrawSelection();
        }

        public virtual void SetSpawner(Structure spawner)
        {
        }

        protected void LoadAttackTarget(int entityId)
        {
            if (entityId < 0)
            {
                return;
            }

            try
            {
                actionTarget = (Entity)GameManager.Singleton.GetGameEntityById(entityId);
            }
            catch
            {
                Debug.Log(string.Format("Failed to load Attack target"));
            }
        }

        protected void SetOwnership()
        {
            Owner = GetComponentInParent<Player>();
            if (Owner == null)
            {
                //Debug.Log(string.Format("{0} has no owner", entityName));
                return;
            }

            //Debug.Log(string.Format("{0} belongs to {1}", entityName, player.username));
            if (isLoadedFromSave == true)
            {
                return;
            }

            SetTeamColour();
        }

        protected void SetTeamColour()
        {
            FactionColour[] allTeamColours = GetComponentsInChildren<FactionColour>();
            foreach (FactionColour teamColour in allTeamColours)
            {
                if (Owner == null)
                {
                    teamColour.GetComponent<Renderer>().material.color = GameManager.Singleton.DefaultColour;
                    continue;
                }

                teamColour.GetComponent<Renderer>().material.color = Owner.Colour;
            }
        }

        protected virtual void SetWeaponDefaults()
        {
            bool readyToAttack = IsAbleToAttack();
            if (readyToAttack == false)
            {
                return;
            }

            SetDefaultWeaponRange();
            SetDefaultWeaponAimSpeed();
            SetDefaultWeaponCooldown();
        }

        private void SetDefaultWeaponRange()
        {
            if (WeaponRange > 0)
            {
                return;
            }

            WeaponRange = DEFAULT_WEAPON_RANGE;
        }

        private void SetDefaultWeaponAimSpeed()
        {
            if (WeaponAimSpeed > 0)
            {
                return;
            }

            WeaponAimSpeed = DEFAULT_WEAPON_AIM_SPEED;
        }

        private void SetDefaultWeaponCooldown()
        {
            if (WeaponCooldown > 0)
            {
                return;
            }

            WeaponCooldown = DEFAULT_WEAPON_COOLDOWN;
        }

        public bool IsOwnedByPlayer(Player player)
        {
            if (Owner == null)
            {
                return false;
            }

            return Owner.Equals(player);
        }

        public virtual void SetSelection(bool selected)
        {
            isSelected = selected;
            if (selected == false)
            {
                return;
            }

            this.playingArea = Hud.GetPlayingArea();
        }

        private void DrawSelection()
        {
            GUI.skin = GlobalAssets.SelectionBoxSkin;
            Rect selectionBox = GameManager.CalculateSelectionBox(SelectionBounds, playingArea);

            GUI.BeginGroup(playingArea);
            DrawSelectionBox(selectionBox);
            GUI.EndGroup();
        }

        protected virtual void DrawSelectionBox(Rect selectionBox)
        {
            GUI.Box(selectionBox, string.Empty);
        }

        public void UpdateBounds()
        {
            Bounds updatedBounds = new Bounds(transform.position, Vector3.zero);
            GameObject meshes = transform.Find(EntityProperties.MESHES).gameObject;
            Renderer[] allRenderers = meshes.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                bool enabled = renderer.enabled;
                if (enabled == false)
                {
                    continue;
                }

                updatedBounds.Encapsulate(renderer.bounds);
            }

            SelectionBounds = updatedBounds;
        }

        public virtual void PerformAction(string actionToPerform) { }

        public virtual void MouseClick(GameObject hitGameObject, Vector3 hitPoint, Player player)
        {
            if (isSelected == false)
            {
                return;
            }
            if (hitGameObject == null)
            {
                return;
            }
            bool isGround = hitGameObject.CompareTag(Tags.GROUND);
            if (isGround == true)
            {
                return;
            }

            Destructible hitEntity = hitGameObject.GetComponentInParent<Destructible>();
            if (hitEntity == null)
            {
                return;
            }
            if (hitEntity == this)
            {
                return;
            }

            bool readyToAttack = IsAbleToAttack();
            if (readyToAttack == false)
            {
                ChangeSelection(hitEntity, player);
                return;
            }

            if (hitEntity.MaxHitPoints == 0)
            {
                ChangeSelection(hitEntity, player);
                return;
            }

            Player hitEntityOwner = hitEntity.Owner;
            if (hitEntityOwner != null)
            {
                bool samePlayer = Owner.PlayerId == hitEntityOwner.PlayerId;
                if (samePlayer == true)
                {
                    ChangeSelection(hitEntity, player);
                    return;
                }
            }

            SetAttackTarget(hitEntity);
        }

        private void ChangeSelection(Selectable otherEntity, Player player)
        {
            if (otherEntity == this)
            {
                return;
            }

            SetSelection(false);
            if (player.SelectedEntity != null)
            {
                player.SelectedEntity.SetSelection(false);
            }

            player.SelectedEntity = otherEntity;
            playingArea = Hud.GetPlayingArea();
            otherEntity.SetSelection(true);
        }

        public virtual void SetHoverState(GameObject gameObjectUnderMouse)
        {
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

            bool isGround = gameObjectUnderMouse.CompareTag(Tags.GROUND);
            if (isGround == true)
            {
                return;
            }

            Owner.PlayerHud.SetCursorState(CursorState.select);

            bool canAttack = IsAbleToAttack();
            if (canAttack == false)
            {
                return;
            }

            Destructible entityUnderMouse = gameObjectUnderMouse.GetComponentInParent<Destructible>();
            if (entityUnderMouse == null)
            {
                return;
            }

            if (entityUnderMouse.MaxHitPoints == 0)
            {
                return;
            }

            Player entityOwner = entityUnderMouse.Owner;
            if (entityOwner != null)
            {
                bool samePlayer = Owner.PlayerId == entityOwner.PlayerId;
                if (samePlayer == true)
                {
                    return;
                }
            }

            Owner.PlayerHud.SetCursorState(CursorState.attack);
        }

        protected virtual void SetAttackTarget(Entity target)
        {
            actionTarget = target;

            Vector3 targetPosition = actionTarget.transform.position;
            bool targetInRange = IsTargetInRange(targetPosition);
            if (targetInRange == true)
            {
                isAttacking = true;
                AttackTarget(actionTarget);
            }
            else
            {
                AdvanceTowardsTarget(targetPosition);
            }
        }

        public virtual bool IsAbleToAttack()
        {
            return false;
        }

        private bool IsTargetInRange(Vector3 targetPosition)
        {

            Vector3 bearingToTarget = targetPosition - transform.position;
            float weaponsRangeSquared = WeaponRange * WeaponRange;
            if (bearingToTarget.sqrMagnitude < weaponsRangeSquared)
            {
                return true;
            }

            return false;
        }

        private void AttackTarget(Entity attackTarget)
        {
            if (attackTarget == null)
            {
                isAttacking = false;
                return;
            }

            Vector3 targetPosition = attackTarget.transform.position;
            bool targetInRange = IsTargetInRange(targetPosition);
            if (targetInRange == false)
            {
                AdvanceTowardsTarget(targetPosition);
                return;
            }

            bool targetInSights = IsTargetInSights(targetPosition);
            if (targetInSights == false)
            {
                GetBearingToTarget(targetPosition);
                return;
            }

            bool readyToFire = IsReadyToFire();
            if (readyToFire == false)
            {
                return;
            }

            FireWeaponAtTarget(attackTarget);
        }

        private bool IsTargetInSights(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            if (bearingToTarget.normalized == transform.forward.normalized)
            {
                return true;
            }

            return false;
        }

        private bool IsReadyToFire()
        {
            if (currentCooldownRemaining <= 0)
            {
                return true;
            }

            return false;
        }

        protected virtual void FireWeaponAtTarget(Entity target)
        {
            currentCooldownRemaining = WeaponCooldown;

            string ownersName = "Neutral";
            string attackTargetsOwnersName = ownersName;
            if (Owner != null)
            {
                ownersName = Owner.PlayerId.ToString();
            }

            Selectable attackTarget = target as Selectable;
            if (attackTarget == null)
            {
                Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
                return;
            }

            if (attackTarget.Owner != null)
            {
                attackTargetsOwnersName = attackTarget.Owner.PlayerId.ToString();
            }

            Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
        }

        protected virtual void GetBearingToTarget(Vector3 targetPosition)
        {
            isAiming = true;
        }

        private void AdvanceTowardsTarget(Vector3 targetPosition)
        {
            Selectable unit = this as Selectable;
            if (unit == null)
            {
                isAttacking = false;
                return;
            }

            isAdvancing = true;
            Vector3 attackPosition = GetNearestAttackPosition(targetPosition);
            unit.SetWaypoint(attackPosition);
            isAttacking = true;
        }

        private Vector3 GetNearestAttackPosition(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            float distanceToTarget = bearingToTarget.magnitude;
            float distanceToTravel = distanceToTarget - (0.9f * WeaponRange); // Move in slightly closer than weapon's range
            Vector3 attackPosition = Vector3.Lerp(transform.position, targetPosition, distanceToTravel / distanceToTarget);
            return attackPosition;
        }

        public void SetTransparencyMaterial(Material transparencyMaterial, bool saveCurrentMaterial)
        {
            if (saveCurrentMaterial == true)
            {
                oldMaterials.Clear();
            }

            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                if (saveCurrentMaterial == true)
                {
                    oldMaterials.Add(renderer.material);
                }

                renderer.material = transparencyMaterial;
            }
        }

        public void RestoreMaterials()
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            int totalRenderers = allRenderers.Length;
            if (oldMaterials.Count != totalRenderers)
            {
                return;
            }

            for (int i = 0; i < totalRenderers; i++)
            {
                allRenderers[i].material = oldMaterials[i];
            }
        }
    }
}