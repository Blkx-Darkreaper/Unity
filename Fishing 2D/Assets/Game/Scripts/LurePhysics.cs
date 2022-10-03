using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Force = ForceManager.Force;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ForceManager))]
public class LurePhysics : MonoBehaviour
{
    [Header("Reel")]
    [SerializeField] private PlayerReel reel;

    [Header("Tension")]
    //[SerializeField] private Vector2Variable lineTensionVector;

    [Header("Gravity")]


    [Header("Buoyancy")]
    [SerializeField] private float lureRadius;

    [ReadOnlyInInspector] public float depthSubmerged = 0f;
    [ReadOnlyInInspector] public Force buoyancy;

    [Header("Fish")]
    [ReadOnlyInInspector] public Force fish;

    [Header("Unspooling Resistance")]
    //[SerializeField] private Vector2Variable unspoolingResistance;

    [Header("Net Force")]
    [SerializeField] public Vector2Variable netForce;

    private Rigidbody2D body;
    private ForceManager forces;

    void Awake()
    {
        this.body = GetComponent<Rigidbody2D>();
        this.forces = GetComponent<ForceManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.buoyancy = new Force(Vector2.zero, ForceMode2D.Force);
        this.fish = new Force(Vector2.zero, ForceMode2D.Force);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        //if (reel == null)
        //{
        //    return;
        //}
        //if (lineTensionVector == null)
        if(reel.lineTension == null)
        {
            return;
        }

        //Vector2 lineTension = lineTensionVector.runtimeValue;
        Vector2 lineTension = reel.lineTension.runtimeValue;
        forces.AddForce(lineTension, ForceMode2D.Force, true);

        Vector2 netForce = lineTension;

        // Apply unspooling resistance

        // Apply buoyancy if in water
        Buoyancy();
        if(buoyancy != null && buoyancy.mode == ForceMode2D.Force && buoyancy.isDrawingOnly != true)
        {
            netForce += buoyancy.vector;
            forces.AddForce(buoyancy.vector, buoyancy.mode, true);
        }

        // Apply force of fish pulling on hook
        if(fish != null && fish.mode == ForceMode2D.Force && fish.isDrawingOnly != true)
        {
            netForce += fish.vector;
            forces.AddForce(fish.vector, fish.mode, true);
        }

        forces.AddForce(netForce);

        // Add gravity component to net force
        this.netForce.runtimeValue = netForce + Vector2.up * body.mass * Physics2D.gravity;
    }

    #region FixedUpdate

    private void Buoyancy()
    {

    }

    #endregion FixedUpdate
}