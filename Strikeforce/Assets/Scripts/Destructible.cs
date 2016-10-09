using UnityEngine;
using Strikeforce;

public class Destructible: Selectable {
	public int MaxHitPoints {
		get;
		set;
	} [HideInInspector]
	public int CurrentHitPoints {
		get;
		set;
	}
	protected float healthPercentage {
		get;
		set;
	}
	protected GUIStyle healthStyle {
		get;
		set;
	}
	private const int HEALTH_BAR_VERTICAL_OFFSET;
	private const int HEALTH_BAR_HEIGHT;
	protected struct DestructibleProperties {
		public const string HIT_POINTS = "HitPoints";
	}

	protected override Awake() {
		base.Awake();

		CurrentHitPoints = MaxHitPoints;
		UpdateHealthPercentage();
	}

	protected void DrawHealthBar(Rect selectionBox) {
		DrawHealthBarWithLabel(selectionBox, string.Empty);
	}

	protected void DrawHealthBarWithLabel(Rect selectionBox, string label) {
		healthStyle.padding.top = -20;
		healthStyle.fontStyle = FontStyle.Bold;

		float x = selectionBox.x;
		float y = selectionBox.y - HEALTH_BAR_VERTICAL_OFFSET;
		float width = selectionBox.width * healthPercentage;
		float height = HEALTH_BAR_HEIGHT;
		GUI.Label(new Rect(x, y, width, height), label, healthStyle);
	}

	protected virtual void UpdateHealthPercentage() {
		UpdateHealthPercentage(0.65f, 0.35f);
	}

	protected virtual void UpdateHealthPercentage(float healthyThreshold, float damagedThreshold) {
		if (maxHitPoints == 0) {
			healthPercentage = 0f;
			return;
		}

		healthPercentage = (float) currentHitPoints / (float) maxHitPoints;

		if (healthPercentage > healthyThreshold) {
			healthStyle.normal.background = ResourceManager.HealthBarTextures.healthy;
			return;
		}
		if (healthPercentage > damagedThreshold) {
			healthStyle.normal.background = ResourceManager.HealthBarTextures.damaged;
			return;
		}

		healthStyle.normal.background = ResourceManager.HealthBarTextures.critical;
	}

	public void TakeDamage(int damage) {
		currentHitPoints -= damage;
		if (currentHitPoints > 0) {
			return;
		}

		DestroyEntity();
	}

	protected void DestroyEntity() {
		GameManager.activeInstance.DestroyGameEntity(this);

		string ownersName = "Neutral";
		if (owner != null) {
			ownersName = string.Format("{0}'s", owner.username);
		}

		Debug.Log(string.Format("{0} {1} has been destroyed", ownersName, name));
	}
}