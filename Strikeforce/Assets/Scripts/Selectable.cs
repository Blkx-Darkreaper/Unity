using UnityEngine;
using Strikeforce;

public class Selectable : Entity {
	public Texture2D BuildImage { get; set; }
	public float BuildTime { get; set; }
	public int Cost { get; set; }
	public int SellValue { get; set; }
	public Bounds SelectionBounds { get; set; }
	public Player Owner { get; set; }
	public string[] Actions { get; set; }
	public float AttackRange { get; set; }
	private float DEFAULT_ATTACK_RANGE { get; set; }
	public float WeaponAimSpeed { get; set; }
	private float DEFAULT_WEAPON_AIM_SPEED { get; set; }
	public float WeaponCooldown { get; set; }
	private float DEFAULT_WEAPON_COOLDOWN { get; set; }
	protected float currentCooldownRemaining { get; set; }
	protected bool isSelected { get; set; }
	protected bool isAttacking { get; set; }
	protected bool isAdvancing { get; set; }
	protected bool isAiming { get; set; }
	protected Entity actionTarget { get; set; }
	protected int actionTargetId { get; set; }
	protected struct SelectableProperties {
        	public const string NAME = "Name";
        	public const string ID = "Id";
        	public const string MESHES = "Meshes";
        	public const string HIT_POINTS = "HitPoints";
        	public const string IS_ATTACKING = "IsAttacking";
        	public const string IS_ADVANCING = "IsAdvancing";
        	public const string IS_AIMING = "IsAiming";
        	public const string COOLDOWN = "CurrentCooldown";
        	public const string TARGET_ID = "TargetId";
	}
	private List<Material> oldMaterial { get; set; }
	
}