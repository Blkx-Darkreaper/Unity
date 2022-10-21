using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReel : MonoBehaviour
{
    //public static PlayerReel singleton;

    [Header("Reel")]
    [SerializeField] private float reelForceMultiplier;
    //[SerializeField] private float reelSpeed = 1f;
    [SerializeField] private float spoolingSpeed;
    [SerializeField] private float spoolThreshold;
    [SerializeField] private float reelControlDistanceThreshold;
    [SerializeField] private RectTransform reelSprite;
    [SerializeField] private Rigidbody2D reelBody;

    public enum Position { Center = 0, Edge = -1, Up = 1, Right = 2, Down = 3, Left = 4 }

    [ReadOnlyInInspector] public Vector2 reelControllerMovement;
    [ReadOnlyInInspector] public bool isReelApplyingTension = false;
    [ReadOnlyInInspector] public float reelRotationVelocity = 0f;   // deg/s
    [ReadOnlyInInspector] public float reelVelocity = 0f; // positive clockwise, negative counter-clockwise
    [ReadOnlyInInspector] public Position previousControllerPosition = Position.Center;
    [ReadOnlyInInspector] public float currentReelControllerAngle;
    private float previousReelControllerAngle;
    [ReadOnlyInInspector] public float deltaReelControllerAngle;
    //private Quaternion previousReelRotation;
    [ReadOnlyInInspector] public float currentReelAngle;
    private float previousReelAngle;
    [ReadOnlyInInspector] public float deltaReelAngle;

    [Header("Line")]
    [SerializeField] private float lineBrakeForceMultiplier;
    [SerializeField] private float lineUnspoolResistanceForceMultiplier;
    [SerializeField] private float castingForceMultiplier;
    [SerializeField] [Range(0f, 1f)] private float castingHorImportancePercent;
    [SerializeField] private float castingSpeedThreshold;
    [SerializeField] private float maxLine;
    [SerializeField] private DistanceJoint2D lineJoint;
    [SerializeField] private LineRenderer line;

    [SerializeField] public FloatVariable lineSlack;
    [SerializeField] public Vector2Variable lineTensionForce;

    public event Action<float> onSlackChanged;
    public event Action<float> onTensionChanged;

    [SerializeField] public bool resetMaxes = false;
    [ReadOnlyInInspector] public float castingSpeed = 0f;
    [ReadOnlyInInspector] public float previousCastingSpeed = 0f;
    [ReadOnlyInInspector] public float deltaCastingSpeed = 0f;  //debug
    [ReadOnlyInInspector] public float maxCastingSpeed = 0f;    //debug
    [ReadOnlyInInspector] public float castingStrength = 0f;
    [ReadOnlyInInspector] public float maxCastingStrength = 0f; //debug
    [ReadOnlyInInspector] public bool isCasting = false;
    [ReadOnlyInInspector] public bool wasCasting = false;
    [ReadOnlyInInspector] public float lureRelStrength = 0f;
    [ReadOnlyInInspector] public float reelStrength = 0f;
    [ReadOnlyInInspector] public float lineStrength = 0f;    //debug
    [ReadOnlyInInspector] public float lineBrakePressure = 0f;
    [ReadOnlyInInspector] public float lineResistanceStrength = 0f;
    [ReadOnlyInInspector] public float maxLineResistanceStrength = 0f;  //debug
    [ReadOnlyInInspector] public float netLineStrength = 0f;
    [ReadOnlyInInspector] public float previousNetLineStrength = 0f;
    [ReadOnlyInInspector] public float maxNetLineStrength = 0f; //debug
    [ReadOnlyInInspector] public float lineRemaining;
    [ReadOnlyInInspector] public float lineOut = 0f;
    [ReadOnlyInInspector] public float deltaLineOut = 0f;
    [ReadOnlyInInspector] public float slack = 0f;   //debug

    [Header("Lure")]
    [SerializeField] private LurePhysics lure;

    private Rigidbody2D lureBody;

    [ReadOnlyInInspector] public Vector2 lureVelocity;
    [ReadOnlyInInspector] public float lureSpeed = 0f;
    [ReadOnlyInInspector] public float maxLureSpeed = 0f;   //debug
    [ReadOnlyInInspector] public float maxLureStrength = 0f; //debug
    [ReadOnlyInInspector] public float rodToLureAngle;  //debug

    [Header("Rod")]
    [SerializeField] private RodTipPhysics rodTip;
    [SerializeField] private Transform rodEnd;

    private Rigidbody2D rodTipBody;

    private PlayerControls controls;

    void Awake()
    {
        this.controls = new PlayerControls();

        if (rodTip != null)
        {
            this.rodTipBody = rodTip.GetComponent<Rigidbody2D>();
        }

        if (lure != null)
        {
            this.lureBody = lure.GetComponent<Rigidbody2D>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.controls.Fishing.ReelMovement.performed += context => reelControllerMovement = context.ReadValue<Vector2>();
        this.controls.Fishing.ReelMovement.canceled += context => reelControllerMovement = Vector2.zero;

        this.controls.Fishing.LineBrake.performed += context => lineBrakePressure = context.ReadValue<float>();
        this.controls.Fishing.LineBrake.canceled += context => lineBrakePressure = 0f;

        this.lineOut = lineJoint.distance;
        this.lineRemaining = maxLine - lineOut;

        this.lureVelocity = Vector2.zero;
    }

    void OnValidate()
    {
        if (resetMaxes != true)
        {
            return;
        }

        this.maxCastingSpeed = 0f;
        this.maxCastingStrength = 0f;
        this.maxLureSpeed = 0f;
        this.maxLureStrength = 0f;
        this.maxLineResistanceStrength = 0f;
        this.maxNetLineStrength = 0f;

        this.resetMaxes = false;
    }

    void OnEnable()
    {
        controls.Fishing.ReelMovement.Enable();

        controls.Fishing.LineBrake.Enable();
    }

    void OnDisable()
    {
        controls.Fishing.ReelMovement.Disable();

        controls.Fishing.LineBrake.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (lureBody == null)
        {
            return;
        }
        if (rodTipBody == null)
        {
            return;
        }
        if (lineJoint == null)
        {
            return;
        }

        ReelInput();
    }

    #region Update

    private void ReelInput()
    {
        this.isReelApplyingTension = false;

        if (reelControllerMovement.magnitude == 0)
        {
            reelBody.angularVelocity = 0;
            return;
        }

        Position position = GetPosition(reelControllerMovement, reelControlDistanceThreshold);

        if (position == Position.Center)
        {
            reelBody.angularVelocity = 0;
            this.reelVelocity = 0f;
            this.previousControllerPosition = position;
            return;
        }

        this.currentReelControllerAngle = Mathf.Atan2(reelControllerMovement.x, reelControllerMovement.y)
            * Mathf.Rad2Deg;

        // To prevent reel from jumping to a new orientation
        if (previousControllerPosition == Position.Center)
        {
            this.previousControllerPosition = position;
            this.previousReelControllerAngle = currentReelControllerAngle;
            return;
        }

        this.previousControllerPosition = position;

        this.isReelApplyingTension = true;

        this.deltaReelControllerAngle = Mathf.DeltaAngle(previousReelControllerAngle, currentReelControllerAngle);

        this.previousReelControllerAngle = currentReelControllerAngle;
    }

    private Position GetPosition(Vector2 position, float threshold)
    {
        float y = position.y;
        float x = position.x;

        float h = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));

        if (h < 1 - threshold)
        {
            return Position.Center;
        }

        float absY = Mathf.Abs(y);
        if (1 - absY < threshold)
        {
            y /= absY;
        }
        if (absY < threshold)
        {
            y = 0;
        }

        float absX = Mathf.Abs(x);
        if (1 - absX < threshold)
        {
            x /= absX;
        }
        if (absX < threshold)
        {
            x = 0;
        }

        //if (y == 0 && x == 0)
        //{
        //    return Position.Center;
        //}

        if (y == 1 && x == 0)
        {
            return Position.Up;
        }

        if (y == -1 && x == 0)
        {
            return Position.Down;
        }

        if (y == 0 && x == 1)
        {
            return Position.Right;
        }

        if (y == 0 && x == -1)
        {
            return Position.Left;
        }

        return Position.Edge;
    }

    #endregion Update

    void FixedUpdate()
    {
        if (lureBody == null)
        {
            return;
        }
        if (rodTipBody == null)
        {
            return;
        }
        if (rodTip.forceVector == null)
        {
            return;
        }

        Casting();

        Tension();

        Slack();

        this.maxLureSpeed = Mathf.Max(lureSpeed, maxLureSpeed); //debug

        //// Update force line is applying to lure
        //if (netLineStrength <= 0)
        //{
        //    this.lineTensionForce.runtimeValue = Vector2.zero;
        //    return;
        //}

        //Vector2 rodTipDirection = (rodTipBody.transform.position - lureBody.transform.position).normalized;
        //this.lineTensionForce.runtimeValue = rodTipDirection * netLineStrength;
    }

    #region FixedUpdate

    private void Casting()
    {
        Vector2 endPoint;

        Vector2 rodDirection = (rodTipBody.transform.position - rodEnd.position).normalized;

        // Draw casting direction
        endPoint = (Vector2)rodTipBody.transform.position + rodDirection;
        Debug.DrawLine(rodTipBody.transform.position, endPoint, Color.cyan);

        Vector2 rodTipToLureDirection = (lureBody.transform.position - rodTipBody.transform.position).normalized;

        float lureDotProd = Vector2.Dot(lureVelocity, rodDirection);
        Vector2 relLureVelocity = rodDirection * lureDotProd;

        Vector2 horDirection = Vector2.right;
        if (lureVelocity.x < 0)
        {
            horDirection = Vector2.left;
        }

        float horDotProd = Vector2.Dot(relLureVelocity, horDirection);

        // Adding lure velocity at high casting force multipliers makes lure really twitchy
        this.castingSpeed = Globals.FloorFloatToPrecision(Mathf.Abs(lureDotProd * (1 - castingHorImportancePercent)
            + horDotProd * castingHorImportancePercent));

        // Draw rod tip velocity vector
        endPoint = (Vector2)lureBody.transform.position + rodTipToLureDirection * castingSpeed;
        Debug.DrawLine(lureBody.transform.position, endPoint, Color.white);

        this.deltaCastingSpeed = Globals.FloorFloatToPrecision(castingSpeed - previousCastingSpeed);   //debug
        this.maxCastingSpeed = Mathf.Max(castingSpeed, maxCastingSpeed);    //debug

        // If casting speed exceeds threshold then disable line limit
        this.isCasting = castingSpeed > castingSpeedThreshold;

        if (isCasting == true)
        {
            lineJoint.enabled = false;
        }

        if(isCasting != true && wasCasting == true)
        {
            float distance = Vector2.Distance(rodTipBody.transform.position, lureBody.transform.position);
            lineJoint.distance = distance;

            lineJoint.enabled = true;
        }

        float deltaMomentum = lureBody.mass * (castingSpeed - previousCastingSpeed);
        this.castingStrength = deltaMomentum / Time.fixedDeltaTime * castingForceMultiplier;  // Needs to always be positive or line will retract
        this.castingStrength = Globals.FloorFloatToPrecision(Mathf.Clamp(castingStrength, 0, float.MaxValue));

        this.maxCastingStrength = Mathf.Max(castingStrength, maxCastingStrength);   //debug

        this.previousCastingSpeed = castingSpeed;
        this.wasCasting = isCasting;
    }

    private void Tension()
    {
        // If net force component away from rod tip towards lure is positive then unspool line
        Vector2 lureNetForce = lure.netForce.runtimeValue;
        this.maxLureStrength = lureNetForce.magnitude;

        Vector2 rodTipLureDirection = (lureBody.transform.position - rodTipBody.transform.position).normalized;

        // Get component of lure momentum away from rod tip
        this.lureVelocity = lureBody.velocity;
        this.lureSpeed = Globals.FloorFloatToPrecision(lureVelocity.magnitude);

        float lureDotProd = Mathf.Clamp(Vector2.Dot(lureNetForce, rodTipLureDirection), 0, float.MaxValue);
        this.lureRelStrength = Globals.FloorFloatToPrecision(lureDotProd);

        // If reel is applying a greater negative force then spool line
        this.reelStrength = 0;

        // Counter-Clockwise - Reel in
        if (isReelApplyingTension == true && deltaReelControllerAngle <= 0)
        {
            reelStrength = (1 - deltaReelControllerAngle) * reelForceMultiplier;
        }

        // Clockwise - Reel out
        if (deltaReelControllerAngle > 0)
        {
            reelStrength = -deltaReelControllerAngle * reelForceMultiplier;
        }

        this.reelStrength = Globals.FloorFloatToPrecision(reelStrength);

        // Determine net strength
        //float netStrength = lineStrength - reelStrength;
        this.lineStrength = lureRelStrength + castingStrength - reelStrength;

        // If line brake is being applied then reduce net strength towards 0
        float lineBrakeResistance = lineBrakePressure * lineBrakeForceMultiplier;

        // If net strength is positive then apply unspool resistance to reduce towards 0
        this.lineResistanceStrength = lineBrakeResistance;
        if (deltaReelControllerAngle >= 0)
        {
            this.lineResistanceStrength += lineUnspoolResistanceForceMultiplier * lineRemaining;
        }

        this.lineResistanceStrength = Globals.FloorFloatToPrecision(lineResistanceStrength);
        this.maxLineResistanceStrength = Mathf.Max(lineResistanceStrength, maxLineResistanceStrength);

        this.netLineStrength = lineStrength;

        if (lineStrength > 0)
        {
            this.netLineStrength = Mathf.Clamp(lineStrength - lineResistanceStrength, 0, lineStrength);
        }
        if (lineStrength < 0)
        {
            this.netLineStrength = Mathf.Clamp(lineStrength + lineResistanceStrength, lineStrength, 0);
        }

        this.netLineStrength = Globals.FloorFloatToPrecision(netLineStrength);
        this.maxNetLineStrength = Mathf.Max(netLineStrength, maxNetLineStrength);

        if (netLineStrength != 0)
        {
            AdjustLineOut(lineStrength * spoolingSpeed);

            RotateReel(deltaLineOut / spoolingSpeed, Time.fixedDeltaTime);
        }

        if (previousNetLineStrength == netLineStrength)
        {
            return;
        }

        this.previousNetLineStrength = netLineStrength;

        if (onTensionChanged == null)
        {
            return;
        }

        onTensionChanged.Invoke(slack);
    }

    private void Slack()
    {
        float distance = Vector2.Distance(rodTipBody.transform.position, lureBody.transform.position);

        this.slack = Globals.FloorFloatToPrecision(Mathf.Clamp(lineOut - distance, 0, maxLine));
        if (slack == lineSlack.runtimeValue)
        {
            return;
        }

        this.lineSlack.runtimeValue = slack;

        if (onSlackChanged == null)
        {
            return;
        }

        onSlackChanged.Invoke(slack);
    }

    private void AdjustLineOut(float delta)
    {
        this.deltaLineOut = Globals.FloorFloatToPrecision(Mathf.Clamp(delta, -lineOut, lineRemaining));
        if (deltaLineOut == 0)
        {
            return;
        }

        // Stop reel out when no line remaining
        if (lineOut >= (maxLine - spoolThreshold) && deltaLineOut > 0)
        {
            return;
        }

        // Stop reel in when no line out remaining
        if (lineRemaining >= (maxLine - spoolThreshold) && deltaLineOut < 0)
        {
            // TODO: Player jamming sound
            return;
        }

        this.lineOut = Globals.FloorFloatToPrecision(Mathf.Clamp(lineOut + deltaLineOut, 0, maxLine));
        //this.lineRemaining = Mathf.Clamp(lineRemaining - deltaLine, 0, maxLine);
        this.lineRemaining = Globals.FloorFloatToPrecision(Mathf.Clamp(maxLine - lineOut, 0, maxLine));

        lineJoint.distance = lineOut;
    }

    private void RotateReel(float delta, float timeElapsed)
    {
        // Spin reel as it spools out
        this.deltaReelAngle = Globals.FloorFloatToPrecision(delta);

        Quaternion currentRotation = Quaternion.Euler(0f, 0f, -deltaReelAngle);
        reelSprite.rotation *= currentRotation;

        //Quaternion rotationDiff = currentRotation * Quaternion.Inverse(previousRotation);
        //Quaternion up = Quaternion.LookRotation(Vector2.up);
        //Quaternion normalizedRotation = up * rotationDiff;
        //this.deltaTurn = 

        this.reelRotationVelocity = Globals.FloorFloatToPrecision(deltaReelAngle / timeElapsed);
        this.currentReelAngle = Globals.FloorFloatToPrecision(previousReelAngle + delta);
        reelBody.rotation -= deltaReelAngle;
        //reelBody.angularVelocity = -rotationVelocity; // tends to skip
        //reelBody.AddTorque(rotationVelocity);

        //this.previousReelRotation = currentRotation;
        this.previousReelAngle = currentReelAngle;
    }

    #endregion FixedUpdate
}