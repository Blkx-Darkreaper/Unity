using UnityEngine;
using System.Collections;

[System.Serializable]
public struct MinMax {
	public float min;
	public float max;
}

public class EvasiveManeuver: MonoBehaviour {
	public Rigidbody craft;
	public MinMax startDelay;
	private float strafeTarget;
	public float dodge;
	public MinMax strafeTime;
	public MinMax strafeWait;
	public float smoothing;
	private float currentVelocity;
	public Boundary bounds;
	public float bank;

	public void Start() {
		craft = GetComponent < Rigidbody > ();
		currentVelocity = craft.velocity.z;
		StartCoroutine(Evade());
	}

	public void FixedUpdate() {
		float movement = Mathf.MoveTowards(craft.velocity.x, strafeTarget, Time.deltaTime * smoothing);
		craft.velocity = new Vector3(movement, 0f, currentVelocity);
		craft.position = new Vector3(
		Mathf.Clamp(craft.position.x, bounds.xMin, bounds.xMax), 0f, Mathf.Clamp(craft.position.z, bounds.zMin, bounds.zMax));
		craft.rotation = Quaternion.Euler(0f, 0f, craft.velocity.x * -bank);
	}

	private IEnumerator Evade() {
		yield
		return new WaitForSeconds(Random.Range(startDelay.min, startDelay.max));

		while (true) {
			strafeTarget = Random.Range(1, dodge) * -Mathf.Sign(transform.position.x);
			yield
			return new WaitForSeconds(Random.Range(strafeTime.min, strafeTime.max));
			strafeTarget = 0;
			yield
			return new WaitForSeconds(Random.Range(strafeWait.min, strafeWait.max));
		}
	}
}