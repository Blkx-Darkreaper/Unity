using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ForceManager))]
public class RodTipPhysics : MonoBehaviour
{
    [SerializeField] public Vector2Variable forceVector;

    [ReadOnlyInInspector] Vector2 previousVelocity;

    private Rigidbody2D rodTipBody;
    private ForceManager forces;

    void Awake()
    {
        this.rodTipBody = GetComponent<Rigidbody2D>();
        this.forces = GetComponent<ForceManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(forceVector == null)
        {
            return;
        }

        Vector2 currentVelocity = rodTipBody.velocity;

        Vector2 acceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
        Vector2 force = acceleration * rodTipBody.mass;

        forceVector.runtimeValue = force;

        forces.AddForce(Color.yellow, force, ForceMode2D.Force, true);

        this.previousVelocity = currentVelocity;
    }
}