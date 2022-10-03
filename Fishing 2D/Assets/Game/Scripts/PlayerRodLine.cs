using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRodLine : MonoBehaviour
{
    [SerializeField] private float brakePressureThreshold;
    [SerializeField] private float forceMultiplier = 2f;
    [SerializeField] private Rigidbody2D lureBody;
    [SerializeField] private Rigidbody2D rodTipBody;
    [SerializeField] private DistanceJoint2D line;

    [ReadOnlyInInspector] public float brakePressure = 0f;
    [ReadOnlyInInspector] public Vector2 rodTipVelocity;
    [ReadOnlyInInspector] public Vector2 lureVelocity;

    private PlayerControls controls;

    void Awake()
    {
        this.controls = new PlayerControls();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.controls.Fishing.LineBrake.performed += context => brakePressure = context.ReadValue<float>();
        this.controls.Fishing.LineBrake.canceled += context => brakePressure = 0f;
    }

    void FixedUpdate()
    {
        if(rodTipBody == null)
        {
            return;
        }
        if(lureBody == null)
        {
            return;
        }
        if(line == null)
        {
            return;
        }

        this.rodTipVelocity = rodTipBody.velocity;

        //float rodtipSpeed = rodTipVelocity.magnitude;
        //if (rodtipSpeed < 0.01f)
        //{
        //    return;
        //}

        //line.enabled = brakePressure > 1 - brakePressureThreshold;

        float distance = Vector2.Distance(lureBody.transform.position, transform.position);

        Vector2 appliedVelocity = rodTipVelocity * brakePressure;
        Vector2 appliedForce = appliedVelocity * forceMultiplier;
        //if (distance > 0)
        //{
        //    lureForce /= distance;
        //}

        ForceManager forces = lureBody.GetComponent<ForceManager>();
        if (forces == null)
        {
            return;
        }

        forces.AddForce(appliedForce);

        //lureBody.AddForce(appliedForce);
        lureBody.gravityScale = 1 - brakePressure;

        this.lureVelocity = lureBody.velocity;
    }

    void OnEnable()
    {
        controls.Fishing.LineBrake.Enable();
    }

    void OnDisable()
    {
        controls.Fishing.LineBrake.Disable();
    }
}