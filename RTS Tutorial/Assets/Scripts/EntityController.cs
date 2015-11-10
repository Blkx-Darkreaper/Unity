using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class EntityController : MonoBehaviour
{

    public string entityName;
    public Texture2D buildImage;
    public int cost, sellValue, maxHitPoints;
    [HideInInspector]
    public int currentHitPoints;
    protected float healthPercentage;
    private const int HEALTH_BAR_VERTICAL_OFFSET = 7;
    private const int HEALTH_BAR_HEIGHT = 5;
    protected GUIStyle healthStyle = new GUIStyle();
    public Bounds selectionBounds { get; protected set; }
    public Rect playingArea { get; set; }
    private List<Material> oldMaterials = new List<Material>();
    public PlayerController owner { get; set; }
    public string[] actions { get; protected set; }
    protected bool isSelected = false;
    protected bool isAttacking = false;
    protected bool isAdvancing = false;
    protected bool isAiming = false;
    protected EntityController attackTarget = null;
    public float weaponRange = 10f;
    private const float DEFAULT_WEAPON_RANGE = 10f;
    public float weaponAimSpeed = 1f;
    private const float DEFAULT_WEAPON_AIM_SPEED = 1f;
    public float weaponCooldown = 1f;
    private const float DEFAULT_WEAPON_COOLDOWN = 1f;
    private float currentCooldownRemaining;

    protected virtual void Awake()
    {
        selectionBounds = ResourceManager.invalidBounds;
        UpdateBounds();

        currentHitPoints = maxHitPoints;
        UpdateHealthPercentage();

        playingArea = new Rect(0f, 0f, 0f, 0f);
    }

    protected virtual void Start()
    {
        owner = GetComponentInParent<PlayerController>();
        if (owner == null)
        {
            //Debug.Log(string.Format("{0} has no owner", entityName));
            return;
        }

        //Debug.Log(string.Format("{0} belongs to {1}", entityName, player.username));
        SetTeamColour();
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

        AttackTarget(attackTarget);
    }

    protected virtual void OnGUI()
    {
        if (isSelected == false)
        {
            return;
        }

        DrawSelection();
    }

    protected void SetTeamColour()
    {
        TeamColour[] allTeamColours = GetComponentsInChildren<TeamColour>();
        foreach (TeamColour teamColour in allTeamColours)
        {
            teamColour.GetComponent<Renderer>().material.color = owner.teamColour;
        }
    }

    private void SetWeaponDefaults()
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
        if (weaponRange > 0)
        {
            return;
        }

        weaponRange = DEFAULT_WEAPON_RANGE;
    }

    private void SetDefaultWeaponAimSpeed()
    {
        if (weaponAimSpeed > 0)
        {
            return;
        }

        weaponAimSpeed = DEFAULT_WEAPON_AIM_SPEED;
    }

    private void SetDefaultWeaponCooldown()
    {
        if (weaponCooldown > 0)
        {
            return;
        }

        weaponCooldown = DEFAULT_WEAPON_COOLDOWN;
    }

    public bool IsOwnedByPlayer(PlayerController player)
    {
        if (owner == null)
        {
            return false;
        }

        return owner.Equals(player);
    }

    public virtual void SetSelection(bool selected)
    {
        isSelected = selected;
        if (selected == false)
        {
            return;
        }

        this.playingArea = HudController.GetPlayingArea();
    }

    private void DrawSelection()
    {
        GUI.skin = ResourceManager.selectionBoxSkin;
        Rect selectionBox = GameManager.CalculateSelectionBox(selectionBounds, playingArea);

        GUI.BeginGroup(playingArea);
        DrawSelectionBox(selectionBox);
        GUI.EndGroup();
    }

    protected virtual void DrawSelectionBox(Rect selectionBox)
    {
        GUI.Box(selectionBox, string.Empty);

        UpdateHealthPercentage();
        DrawHealthBar(selectionBox);
    }

    protected void DrawHealthBar(Rect selectionBox)
    {
        DrawHealthBarWithLabel(selectionBox, string.Empty);
    }

    protected void DrawHealthBarWithLabel(Rect selectionBox, string label)
    {
        healthStyle.padding.top = -20;
        healthStyle.fontStyle = FontStyle.Bold;

        float x = selectionBox.x;
        float y = selectionBox.y - HEALTH_BAR_VERTICAL_OFFSET;
        float width = selectionBox.width * healthPercentage;
        float height = HEALTH_BAR_HEIGHT;
        GUI.Label(new Rect(x, y, width, height), label, healthStyle);
    }

    protected virtual void UpdateHealthPercentage()
    {
        UpdateHealthPercentage(0.65f, 0.35f);
    }

    protected virtual void UpdateHealthPercentage(float healthyThreshold, float damagedThreshold)
    {
        if (maxHitPoints == 0)
        {
            healthPercentage = 0f;
            return;
        }

        healthPercentage = (float)currentHitPoints / (float)maxHitPoints;

        if (healthPercentage > healthyThreshold)
        {
            healthStyle.normal.background = ResourceManager.HealthBarTextures.healthy;
            return;
        }
        if (healthPercentage > damagedThreshold)
        {
            healthStyle.normal.background = ResourceManager.HealthBarTextures.damaged;
            return;
        }

        healthStyle.normal.background = ResourceManager.HealthBarTextures.critical;
    }

    public void UpdateBounds()
    {
        Bounds updatedBounds = new Bounds(transform.position, Vector3.zero);
        GameObject meshes = transform.Find("Meshes").gameObject;
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

        selectionBounds = updatedBounds;
    }

    public virtual void PerformAction(string actionToPerform)
    {
    }

    public virtual void MouseClick(GameObject hitGameObject, Vector3 hitPoint, PlayerController player)
    {
        if (isSelected == false)
        {
            return;
        }
        if (hitGameObject == null)
        {
            return;
        }
        bool isGround = hitGameObject.CompareTag(Tags.ground);
        if (isGround == true)
        {
            return;
        }

        EntityController hitEntity = hitGameObject.GetComponentInParent<EntityController>();
        if (hitEntity == null)
        {
            return;
        }
		if (hitEntity == this) {
			return;
		}

        ResourceController hitResource = hitGameObject.GetComponentInParent<ResourceController>();
        if (hitResource != null)
        {
            bool resourceDepleted = hitResource.isEmpty;
            if (resourceDepleted == true)
            {
                return;
            }
        }

        bool readyToAttack = IsAbleToAttack();
        if (readyToAttack == false)
        {
            ChangeSelection(hitEntity, player);
            return;
        }

        if (hitEntity.maxHitPoints == 0)
        {
            ChangeSelection(hitEntity, player);
            return;
        }

        PlayerController hitEntityOwner = hitEntity.owner;
        if (hitEntityOwner != null)
        {
			bool samePlayer = owner.username.Equals(hitEntityOwner.username);
			if (samePlayer == true)
			{
				ChangeSelection(hitEntity, player);
				return;
			}
        }

        SetAttackTarget(hitEntity);
    }

    private void ChangeSelection(EntityController otherEntity, PlayerController player)
    {
        if (otherEntity == this)
        {
            return;
        }

        SetSelection(false);
        if (player.selectedEntity != null)
        {
            player.selectedEntity.SetSelection(false);
        }

        player.selectedEntity = otherEntity;
        playingArea = HudController.GetPlayingArea();
        otherEntity.SetSelection(true);
    }

    public virtual void SetHoverState(GameObject gameObjectUnderMouse)
    {
        if (owner == null)
        {
            return;
        }
        if (owner.isNPC == true)
        {
            return;
        }
        if (isSelected == false)
        {
            return;
        }

        bool isGround = gameObjectUnderMouse.CompareTag(Tags.ground);
        if (isGround == true)
        {
            return;
        }

        owner.hud.SetCursorState(CursorState.select);

        bool canAttack = IsAbleToAttack();
        if (canAttack == false)
        {
            return;
        }

        EntityController entityUnderMouse = gameObjectUnderMouse.GetComponentInParent<EntityController>();
        if (entityUnderMouse == null)
        {
            return;
        }

        if (entityUnderMouse.maxHitPoints == 0)
        {
            return;
        }

        PlayerController entityOwner = entityUnderMouse.owner;
        if (entityOwner != null)
        {
            bool samePlayer = owner.username.Equals(entityOwner.username);
            if (samePlayer == true)
            {
                return;
            }
        }

        owner.hud.SetCursorState(CursorState.attack);
    }

    public void TakeDamage(int damage)
    {
        currentHitPoints -= damage;
        if (currentHitPoints > 0)
        {
            return;
        }

        DestroyEntity();
    }

    protected void DestroyEntity()
    {
        Destroy(gameObject);

        string ownersName = "Neutral";
        if (owner != null)
        {
            ownersName = string.Format("{0}'s", owner.username);
        }

        Debug.Log(string.Format("{0} {1} has been destroyed", ownersName, entityName));
    }

    protected virtual void SetAttackTarget(EntityController target)
    {
        attackTarget = target;

        Vector3 targetPosition = attackTarget.transform.position;
        bool targetInRange = IsTargetInRange(targetPosition);
        if (targetInRange == true)
        {
            isAttacking = true;
            AttackTarget(attackTarget);
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
        float weaponsRangeSquared = weaponRange * weaponRange;
        if (bearingToTarget.sqrMagnitude < weaponsRangeSquared)
        {
            return true;
        }

        return false;
    }

    private void AttackTarget(EntityController attackTarget)
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
            AimTowardsTarget(targetPosition);
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

    protected virtual void FireWeaponAtTarget(EntityController attackTarget)
    {
        currentCooldownRemaining = weaponCooldown;

        string ownersName = "Neutral";
        string attackTargetsOwnersName = ownersName;
        if (owner != null)
        {
            ownersName = owner.username;
        }
        if (attackTarget.owner != null)
        {
            attackTargetsOwnersName = attackTarget.owner.username;
        }

        Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, entityName, 
            attackTargetsOwnersName, attackTarget.entityName));
    }

    protected virtual void AimTowardsTarget(Vector3 targetPosition)
    {
        isAiming = true;
    }

    private void AdvanceTowardsTarget(Vector3 targetPosition)
    {
        UnitController unit = this as UnitController;
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
        float distanceToTravel = distanceToTarget - (0.9f * weaponRange);   // Move in slightly closer than weapon's range
        Vector3 attackPosition = Vector3.Lerp(transform.position, targetPosition, distanceToTravel / distanceToTarget);
        return attackPosition;
    }

    public void SetColliders(bool enabled)
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            collider.enabled = enabled;
        }
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