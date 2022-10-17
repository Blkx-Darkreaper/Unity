using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Force = ForceManager.Force;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ForceManager))]
public class LurePhysics : MonoBehaviour
{
    [Header("Reel")]
    [SerializeField] private PlayerReel reel;

    [Header("Tension")]
    //[SerializeField] private Vector2Variable lineTensionVector;

    [Header("Gravity")]

    [Header("Normal")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Transform groundCheck;

    //private const string GROUND_TAG = "Ground";

    [ReadOnlyInInspector] public bool isOnGround = false;
    [ReadOnlyInInspector] public Force normalForce;

    [Header("Buoyancy")]
    [SerializeField] private bool isBuoyancyActive = true;
    [SerializeField] private float buoyancyForceModifier;
    [SerializeField] private BuoyancyEffector2D buoyancy;
    [SerializeField] private float waterSurfaceOffset;

    [ReadOnlyInInspector] public bool isFloating = false;
    [ReadOnlyInInspector] public float lureRadius;
    [ReadOnlyInInspector] public float depthSubmerged = 0f;
    [ReadOnlyInInspector] public Force buoyancyForce;

    [Header("Fish")]
    [ReadOnlyInInspector] public Force fish;

    [Header("Unspooling Resistance")]
    //[SerializeField] private Vector2Variable unspoolingResistance;

    [Header("Net Force")]
    [SerializeField] public Vector2Variable netForce;

    //private Collider2D collider;
    private Rigidbody2D body;
    private ForceManager forces;

    void Awake()
    {
        //this.collider = GetComponent<Collider2D>();
        this.body = GetComponent<Rigidbody2D>();
        this.forces = GetComponent<ForceManager>();

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            return;
        }

        this.lureRadius = collider.radius;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.buoyancyForce = Force.zero;
        this.fish = Force.zero;
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground collider gizmo
        Color gizmoColour = Color.red;
        if (isOnGround == true)
        {
            gizmoColour = Color.green;
        }

        Gizmos.color = gizmoColour;
        Gizmos.DrawWireSphere((Vector2)groundCheck.position, groundCheckRadius);

        if(body == null)
        {
            return;
        }

        // Draw velocity vector
        Vector2 endPoint = (Vector2)(transform.position) + body.velocity;
        //Debug.DrawLine(transform.position, endPoint, Color.white);
        Globals.Debug.DrawCustomLine(transform.position, endPoint, 3, Color.white);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (reel == null)
        {
            return;
        }
        //if (lineTensionVector == null)
        if (reel.lineTensionForce == null)
        {
            return;
        }

        Vector2 netForce = Vector2.zero;

        // Note: makes lure respond strangely when reeling in
        ////Vector2 lineTension = lineTensionVector.runtimeValue;
        //Vector2 lineTension = reel.lineTensionForce.runtimeValue;
        //forces.AddForce(lineTension, ForceMode2D.Force, true);

        //netForce += lineTension;

        // Apply normal force if on ground
        Normal();
        if (normalForce != null && normalForce.mode == ForceMode2D.Force && normalForce.isDrawingOnly != true)
        {
            netForce += normalForce.vector;
            forces.AddForce(normalForce.vector, normalForce.mode, true);
        }

        // Apply buoyancy if in water
        Buoyancy();
        if (buoyancyForce != null && buoyancyForce.mode == ForceMode2D.Force && buoyancyForce.isDrawingOnly != true)
        {
            netForce += buoyancyForce.vector;
            forces.AddForce(buoyancyForce.vector, buoyancyForce.mode, true);
        }

        // Apply force of fish pulling on hook
        if (fish != null && fish.mode == ForceMode2D.Force && fish.isDrawingOnly != true)
        {
            netForce += fish.vector;
            forces.AddForce(fish.vector, fish.mode, true);
        }

        forces.AddForce(netForce);

        // Add gravity component to net force
        this.netForce.runtimeValue = netForce + forces.GetWeight().vector;
    }

    #region FixedUpdate

    private void Buoyancy()
    {
        float surface = buoyancy.surfaceLevel + waterSurfaceOffset;

        this.depthSubmerged = Mathf.Clamp(-(transform.position.y - lureRadius - surface),
            0, float.MaxValue);

        this.isFloating = false;

        if (depthSubmerged <= 0)
        {
            this.buoyancyForce = Force.zero;
            return;
        }

        this.isFloating = true;

        if(isBuoyancyActive == false)
        {
            this.buoyancyForce = Force.zero;
            return;
        }

        float volumeSubmerged = Mathf.PI * Mathf.Pow(depthSubmerged, 2) / 3
            * (3 * lureRadius - depthSubmerged);

        if (depthSubmerged >= 2 * lureRadius)
        {
            volumeSubmerged = 4 / 3 * Mathf.PI * Mathf.Pow(lureRadius, 3);
        }

        //float weightStrength = forces.GetWeight().vector.magnitude; //testing

        float buoyacyStrength = volumeSubmerged * buoyancy.density * buoyancyForceModifier;

        this.buoyancyForce = new Force(Color.blue, Vector2.up * buoyacyStrength);
    }

    private void Normal()
    {
        this.isOnGround = false;
        this.normalForce = Force.zero;

        //Collider2D[] allColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundMask);
        //foreach (Collider2D collider in allColliders)
        //{
        //    if (collider.gameObject == gameObject)
        //    {
        //        continue;
        //    }

        //    this.isOnGround = true;
        //}

        //if(isOnGround == false)
        //{
        //    return;
        //}

        //this.normal = new Force(Vector2.up * body.mass * -Physics2D.gravity);

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundMask);
        if (hit.collider == null)
        {
            return;
        }

        Debug.DrawRay(hit.point, hit.normal, Color.green);

        this.isOnGround = true;

        Vector2 gravity = -forces.GetWeight().vector;

        float normalStrength = Vector2.Dot(gravity, hit.normal);
        Vector2 normal = hit.normal.normalized * normalStrength;
        this.normalForce = new Force(Color.green, normal);
    }

    #endregion FixedUpdate
}