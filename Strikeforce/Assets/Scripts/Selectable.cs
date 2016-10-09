using UnityEngine;
using Strikeforce;

public class Selectable: Entity {
	public Texture2D BuildImage {
		get;
		set;
	}
	public float BuildTime {
		get;
		set;
	}
	public int Cost {
		get;
		set;
	}
	public int SellValue {
		get;
		set;
	}
	public Bounds SelectionBounds {
		get;
		protected set;
	}
	public Player Owner {
		get;
		set;
	}
	public string[] Actions {
		get;
		set;
	}
	public float AttackRange = 10f;
	private float DEFAULT_ATTACK_RANGE = 10f;
	public float WeaponAimSpeed = 1f;
	private float DEFAULT_WEAPON_AIM_SPEED = 1f;
	public float WeaponCooldown = 1f;
	private float DEFAULT_WEAPON_COOLDOWN = 1f;
	protected float currentCooldownRemaining {
		get;
		set;
	}
	protected bool isSelected {
		get;
		set;
	}
	protected bool isAttacking {
		get;
		set;
	}
	protected bool isAdvancing {
		get;
		set;
	}
	protected bool isAiming {
		get;
		set;
	}
	protected Entity actionTarget {
		get;
		set;
	}
	protected int actionTargetId {
		get;
		set;
	}
	protected struct SelectableProperties {
		public const string IS_ATTACKING = "IsAttacking";
		public const string IS_ADVANCING = "IsAdvancing";
		public const string IS_AIMING = "IsAiming";
		public const string COOLDOWN = "CurrentCooldown";
		public const string TARGET_ID = "TargetId";
	}
	private List < Material > oldMaterials {
		get;
		set;
	}

	protected override void Awake() {
		base.Awake();

		selectionBounds = ResourceManager.invalidBounds;
		UpdateBounds();
	}

	protected virtual void Start() {
		SetOwnership();

		if (isLoadedFromSave == true) {
			LoadAttackTarget(attackTargetId);
			return;
		}

		SetWeaponDefaults();
	}

	protected virtual void Update() {
		currentCooldownRemaining -= Time.deltaTime;
		currentCooldownRemaining = Mathf.Clamp(currentCooldownRemaining, 0f, float.MaxValue);

		if (isAttacking == false) {
			return;
		}
		if (isAdvancing == true) {
			return;
		}
		if (isAiming == true) {
			return;
		}

		AttackTarget(attackTarget);
	}

	protected virtual void OnGUI() {
		if (isSelected == false) {
			return;
		}
		if (GameManager.activeInstance.isMenuOpen) {
			return;
		}

		DrawSelection();
	}

	protected void LoadAttackTarget(int entityId) {
		if (entityId < 0) {
			return;
		}

		try {
			attackTarget = (EntityController) GameManager.activeInstance.GetGameEntityById(entityId);
		}
		catch {
			Debug.Log(string.Format("Failed to load Attack target"));
		}
	}

	protected void SetOwnership() {
		owner = GetComponentInParent < PlayerController > ();
		if (owner == null) {
			//Debug.Log(string.Format("{0} has no owner", entityName));
			return;
		}

		//Debug.Log(string.Format("{0} belongs to {1}", entityName, player.username));
		if (isLoadedFromSave == true) {
			return;
		}

		SetTeamColour();
	}

	protected void SetTeamColour() {
		TeamColour[] allTeamColours = GetComponentsInChildren < TeamColour > ();
		foreach(TeamColour teamColour in allTeamColours) {
			if (owner == null) {
				teamColour.GetComponent < Renderer > ().material.color = GameManager.activeInstance.defaultColour;
				continue;
			}

			teamColour.GetComponent < Renderer > ().material.color = owner.teamColour;
		}
	}

	protected virtual void SetWeaponDefaults() {
		bool readyToAttack = IsAbleToAttack();
		if (readyToAttack == false) {
			return;
		}

		SetDefaultWeaponRange();
		SetDefaultWeaponAimSpeed();
		SetDefaultWeaponCooldown();
	}

	private void SetDefaultWeaponRange() {
		if (weaponRange > 0) {
			return;
		}

		weaponRange = DEFAULT_WEAPON_RANGE;
	}

	private void SetDefaultWeaponAimSpeed() {
		if (weaponAimSpeed > 0) {
			return;
		}

		weaponAimSpeed = DEFAULT_WEAPON_AIM_SPEED;
	}

	private void SetDefaultWeaponCooldown() {
		if (weaponCooldown > 0) {
			return;
		}

		weaponCooldown = DEFAULT_WEAPON_COOLDOWN;
	}

	public bool IsOwnedByPlayer(PlayerController player) {
		if (owner == null) {
			return false;
		}

		return owner.Equals(player);
	}

	public virtual void SetSelection(bool selected) {
		isSelected = selected;
		if (selected == false) {
			return;
		}

		this.playingArea = HudController.GetPlayingArea();
	}

	private void DrawSelection() {
		GUI.skin = ResourceManager.selectionBoxSkin;
		Rect selectionBox = GameManager.CalculateSelectionBox(selectionBounds, playingArea);

		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectionBox);
		GUI.EndGroup();
	}

	protected virtual void DrawSelectionBox(Rect selectionBox) {
		GUI.Box(selectionBox, string.Empty);

		UpdateHealthPercentage();
		DrawHealthBar(selectionBox);
	}

	public void UpdateBounds() {
		Bounds updatedBounds = new Bounds(transform.position, Vector3.zero);
		GameObject meshes = transform.Find(EntityProperties.MESHES).gameObject;
		Renderer[] allRenderers = meshes.GetComponentsInChildren < Renderer > ();
		foreach(Renderer renderer in allRenderers) {
			bool enabled = renderer.enabled;
			if (enabled == false) {
				continue;
			}

			updatedBounds.Encapsulate(renderer.bounds);
		}

		selectionBounds = updatedBounds;
	}

	public virtual void PerformAction(string actionToPerform) {}

	public virtual void MouseClick(GameObject hitGameObject, Vector3 hitPoint, PlayerController player) {
		if (isSelected == false) {
			return;
		}
		if (hitGameObject == null) {
			return;
		}
		bool isGround = hitGameObject.CompareTag(Tags.GROUND);
		if (isGround == true) {
			return;
		}

		Entity hitEntity = hitGameObject.GetComponentInParent < Entity > ();
		if (hitEntity == null) {
			return;
		}
		if (hitEntity == this) {
			return;
		}

		Resource hitResource = hitGameObject.GetComponentInParent < Resource > ();
		if (hitResource != null) {
			bool resourceDepleted = hitResource.isEmpty;
			if (resourceDepleted == true) {
				return;
			}
		}

		bool readyToAttack = IsAbleToAttack();
		if (readyToAttack == false) {
			ChangeSelection(hitEntity, player);
			return;
		}

		if (hitEntity.maxHitPoints == 0) {
			ChangeSelection(hitEntity, player);
			return;
		}

		PlayerController hitEntityOwner = hitEntity.owner;
		if (hitEntityOwner != null) {
			bool samePlayer = owner.username.Equals(hitEntityOwner.username);
			if (samePlayer == true) {
				ChangeSelection(hitEntity, player);
				return;
			}
		}

		SetAttackTarget(hitEntity);
	}

	private void ChangeSelection(EntityController otherEntity, PlayerController player) {
		if (otherEntity == this) {
			return;
		}

		SetSelection(false);
		if (player.selectedEntity != null) {
			player.selectedEntity.SetSelection(false);
		}

		player.selectedEntity = otherEntity;
		playingArea = HudController.GetPlayingArea();
		otherEntity.SetSelection(true);
	}

	public virtual void SetHoverState(GameObject gameObjectUnderMouse) {
		if (owner == null) {
			return;
		}
		if (owner.isNPC == true) {
			return;
		}
		if (isSelected == false) {
			return;
		}

		bool isGround = gameObjectUnderMouse.CompareTag(Tags.GROUND);
		if (isGround == true) {
			return;
		}

		owner.hud.SetCursorState(CursorState.select);

		bool canAttack = IsAbleToAttack();
		if (canAttack == false) {
			return;
		}

		EntityController entityUnderMouse = gameObjectUnderMouse.GetComponentInParent < EntityController > ();
		if (entityUnderMouse == null) {
			return;
		}

		if (entityUnderMouse.maxHitPoints == 0) {
			return;
		}

		PlayerController entityOwner = entityUnderMouse.owner;
		if (entityOwner != null) {
			bool samePlayer = owner.username.Equals(entityOwner.username);
			if (samePlayer == true) {
				return;
			}
		}

		owner.hud.SetCursorState(CursorState.attack);
	}

	protected virtual void SetAttackTarget(EntityController target) {
		attackTarget = target;

		Vector3 targetPosition = attackTarget.transform.position;
		bool targetInRange = IsTargetInRange(targetPosition);
		if (targetInRange == true) {
			isAttacking = true;
			AttackTarget(attackTarget);
		}
		else {
			AdvanceTowardsTarget(targetPosition);
		}
	}

	public virtual bool IsAbleToAttack() {
		return false;
	}

	private bool IsTargetInRange(Vector3 targetPosition) {

		Vector3 bearingToTarget = targetPosition - transform.position;
		float weaponsRangeSquared = weaponRange * weaponRange;
		if (bearingToTarget.sqrMagnitude < weaponsRangeSquared) {
			return true;
		}

		return false;
	}

	private void AttackTarget(EntityController attackTarget) {
		if (attackTarget == null) {
			isAttacking = false;
			return;
		}

		Vector3 targetPosition = attackTarget.transform.position;
		bool targetInRange = IsTargetInRange(targetPosition);
		if (targetInRange == false) {
			AdvanceTowardsTarget(targetPosition);
			return;
		}

		bool targetInSights = IsTargetInSights(targetPosition);
		if (targetInSights == false) {
			GetBearingToTarget(targetPosition);
			return;
		}

		bool readyToFire = IsReadyToFire();
		if (readyToFire == false) {
			return;
		}

		FireWeaponAtTarget(attackTarget);
	}

	private bool IsTargetInSights(Vector3 targetPosition) {
		Vector3 bearingToTarget = targetPosition - transform.position;
		if (bearingToTarget.normalized == transform.forward.normalized) {
			return true;
		}

		return false;
	}

	private bool IsReadyToFire() {
		if (currentCooldownRemaining <= 0) {
			return true;
		}

		return false;
	}

	protected virtual void FireWeaponAtTarget(EntityController attackTarget) {
		currentCooldownRemaining = weaponCooldown;

		string ownersName = "Neutral";
		string attackTargetsOwnersName = ownersName;
		if (owner != null) {
			ownersName = owner.username;
		}
		if (attackTarget.owner != null) {
			attackTargetsOwnersName = attackTarget.owner.username;
		}

		Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
	}

	protected virtual void GetBearingToTarget(Vector3 targetPosition) {
		isAiming = true;
	}

	private void AdvanceTowardsTarget(Vector3 targetPosition) {
		UnitController unit = this as UnitController;
		if (unit == null) {
			isAttacking = false;
			return;
		}

		isAdvancing = true;
		Vector3 attackPosition = GetNearestAttackPosition(targetPosition);
		unit.SetWaypoint(attackPosition);
		isAttacking = true;
	}

	private Vector3 GetNearestAttackPosition(Vector3 targetPosition) {
		Vector3 bearingToTarget = targetPosition - transform.position;
		float distanceToTarget = bearingToTarget.magnitude;
		float distanceToTravel = distanceToTarget - (0.9f * weaponRange); // Move in slightly closer than weapon's range
		Vector3 attackPosition = Vector3.Lerp(transform.position, targetPosition, distanceToTravel / distanceToTarget);
		return attackPosition;
	}

	public void SetTransparencyMaterial(Material transparencyMaterial, bool saveCurrentMaterial) {
		if (saveCurrentMaterial == true) {
			oldMaterials.Clear();
		}

		Renderer[] allRenderers = GetComponentsInChildren < Renderer > ();
		foreach(Renderer renderer in allRenderers) {
			if (saveCurrentMaterial == true) {
				oldMaterials.Add(renderer.material);
			}

			renderer.material = transparencyMaterial;
		}
	}

	public void RestoreMaterials() {
		Renderer[] allRenderers = GetComponentsInChildren < Renderer > ();
		int totalRenderers = allRenderers.Length;
		if (oldMaterials.Count != totalRenderers) {
			return;
		}

		for (int i = 0; i < totalRenderers; i++) {
			allRenderers[i].material = oldMaterials[i];
		}
	}

	protected override void SaveDetails(JsonWriter writer) {}

	protected override bool LoadDetails(JsonReader reader, string propertyName) {}

	protected virtual bool LoadDetails(JsonReader reader, string propertyName) {}

	protected override void LoadEnd(bool loadComplete) {
		base.LoadEnd(loadComplete);
		if (loadComplete == false) {
			return;
		}

		selectionBounds = ResourceManager.invalidBounds;
		UpdateBounds();
	}
}