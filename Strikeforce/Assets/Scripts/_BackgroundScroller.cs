using UnityEngine;
using System.Collections;

public class _BackgroundScroller : MonoBehaviour {

    public float scrollSpeed;
    private Vector3 startPosition;

    public void Start()
    {
        startPosition = transform.position;
    }

	public void Update () {
        	float displacementChange = Mathf.Repeat(Time.time * -scrollSpeed, transform.localScale.y);
        	transform.position = startPosition + Vector3.forward * displacementChange;
	}
}
