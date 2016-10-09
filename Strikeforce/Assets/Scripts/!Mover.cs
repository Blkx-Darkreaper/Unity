using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

    public float speed;

    public void Start()
    {
        var bolt = GetComponent<Rigidbody>();
        bolt.velocity = transform.forward * speed;
    }
}
