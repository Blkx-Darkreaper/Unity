using UnityEngine;
using Strikeforce;

public class Destructible : Selectable {
	public int MaxHitPoints { get; set; }
	public int CurrentHitPoints { get; set; }
	protected float healthPercentage { get; set; }
	protected GUIStyle healthStyle { get; set; }
	private const int HEALTH_BAR_VERTICAL_OFFSET;
	private const int HEALTH_BAR_HEIGHT;
}