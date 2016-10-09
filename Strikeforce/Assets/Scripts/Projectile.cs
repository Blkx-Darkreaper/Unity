using UnityEngine;
using Strikeforce;

public class Projectile : Destructible {
	public float Velocity { get; set; }
	public int Damage { get; set; }
	public float CurrentRangeToTarget { get; set; }
	public Entity Target { get; set; }

	protected void Update() { }

	protected bool HasHitTarget(Entity target) { }

	private void DealDamageToTarget(Entity target) { }
}