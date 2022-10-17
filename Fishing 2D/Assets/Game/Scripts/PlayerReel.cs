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
    private Quaternion previousReelRotation;
    [ReadOnlyInInspector] public float currentReelAngle;
    private float previousReelAngle;
    [ReadOnlyInInspector] public float deltaReelAngle;

    [Header("Line")]
    [SerializeField] private float lineBrakeForceMultiplier;
    [SerializeField] private float lineUnspoolResistanceForceMultiplier;
    [SerializeField] private float lineLureMomentumMultiplier;
    [SerializeField] private float maxLine;
    [SerializeField] private DistanceJoint2D lineJoint;
    [SerializeField] private LineRenderer line;

    [SerializeField] public FloatVariable lineSlack;
    [SerializeField] public Vector2Variable lineTensionForce;

    public event Action<float> onSlackChanged;
    public event Action<float> onTensionChanged;

    [ReadOnlyInInspector] public float lineStrength = 0f;
    [ReadOnlyInInspector] public float reelStrength = 0f;
    [ReadOnlyInInspector] public float lineResistanceStrength = 0f;
    [ReadOnlyInInspector] public float lineTensionStrength = 0f;
    [ReadOnlyInInspector] public float slack;   //testing
    [ReadOnlyInInspector] public float lineBrakePressure = 0f;
    [ReadOnlyInInspector] public float lineRemaining;
    [ReadOnlyInInspector] public float lineOut = 0f;
    [ReadOnlyInInspector] public float deltaLine = 0f;
    [ReadOnlyInInspector] public Vector2 previousNetMomentum;
    [ReadOnlyInInspector] public Vector2 motionForce;

    [Header("Lure")]
    [SerializeField] private LurePhysics lure;

    private Rigidbody2D lureBody;

    [ReadOnlyInInspector] public Vector2 lureVelocity;
    [ReadOnlyInInspector] public Vector2 lurePreviousVelocity;
    [ReadOnlyInInspector] public float lureSpeed = 0f;
    [ReadOnlyInInspector] public float lureMomentum = 0f;
    [ReadOnlyInInspector] public float lureSpeedDiff;
    [ReadOnlyInInspector] public float lureMaxSpeed = 0f;
    [ReadOnlyInInspector] public float lureStrength = 0f; //testing
    [ReadOnlyInInspector] public float rodToLureAngle;  //testing

    [Header("Rod")]
    [SerializeField] private RodTipPhysics rodTip;

    private Rigidbody2D rodTipBody;

    [ReadOnlyInInspector] public Vector2 rodTipVelocity;
    [ReadOnlyInInspector] public Vector2 rodTipPreviousVelocity;

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

        this.rodTipVelocity = Vector2.zero;
        //this.rodTipPreviousVelocity = Vector2.zero;

        this.lureVelocity = Vector2.zero;
        //this.lurePreviousVelocity = Vector2.zero;

        this.previousNetMomentum = Vector2.zero;

        this.motionForce = Vector2.zero;
    }

    void OnValidate()
    {

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

        Momentum();

        Tension();

        Slack();

        this.lureSpeedDiff = lureMomentum - lureSpeed;
        this.lureMaxSpeed = Mathf.Max(lureSpeed, lureMaxSpeed); //debug

        // Update force line is applying to lure
        if (lineTensionStrength <= 0)
        {
            this.lineTensionForce.runtimeValue = Vector2.zero;
            return;
        }

        Vector2 rodTipDirection = (rodTipBody.transform.position - lureBody.transform.position).normalized;
        this.lineTensionForce.runtimeValue = rodTipDirection * lineTensionStrength;
    }

    #region FixedUpdate

    private void Momentum()
    {
        Vector2 endPoint;

        Vector2 rodTipToLureDirection = lureBody.transform.position - rodTipBody.transform.position;
        Vector2 rodTipToLureNormalDir = rodTipToLureDirection.normalized;

        // Draw rod tip to lure direction
        endPoint = (Vector2)lureBody.transform.position + rodTipToLureNormalDir * 0.5f;
        Debug.DrawLine(lureBody.transform.position, endPoint, Color.cyan);

        //float rodTipToLureDirAngle = Vector2.SignedAngle(Vector2.right, rodTipToLureDirection);

        this.rodTipVelocity = rodTipBody.velocity;
        //Vector2 rotatedRodTipVelocity = Quaternion.Euler(0, 0, -rodTipToLureDirAngle) * rodTipVelocity;

        float rodDotProd = Vector2.Dot(rodTipVelocity, -rodTipToLureNormalDir);
        Vector2 relRodTipVelocity = -rodTipToLureNormalDir * rodDotProd;

        // Draw rod tip velocity vector
        endPoint = (Vector2)rodTipBody.transform.position + relRodTipVelocity;
        //Vector2 endPoint = (Vector2)rodTipBody.transform.position + rodTipToLureNormalDir;
        Debug.DrawLine(rodTipBody.transform.position, endPoint, Color.red);

        this.lureVelocity = lureBody.velocity;
        //Vector2 rotatedLureVelocity = Quaternion.Euler(0, 0, -rodTipToLureDirAngle) * lureVelocity;

        //Vector2 relLureVelocity3 = Quaternion.Euler(0, 0, rodTipToLureDirAngle)
        //    * new Vector2(rotatedLureVelocity.x, 0);    // method 3

        //float lureVelocityAngle = Vector2.Angle(rodTipToLureDirection, lureVelocity);
        //float lureVelocityAngle2 = Vector2.Angle(rodTipToLureNormalDir, lureVelocity);
        //float lureVelocityAngle3 = Vector2.Angle(Vector2.right, rotatedLureVelocity);

        //Vector2 relLureVelocity2 = rodTipToLureNormalDir * rotatedLureVelocity.magnitude
        //    * MathF.Cos(lureVelocityAngle * Mathf.Deg2Rad); // method 2

        float lureDotProd = Vector2.Dot(lureVelocity, rodTipToLureNormalDir);
        Vector2 relLureVelocity = rodTipToLureNormalDir * lureDotProd;

        // Draw lure velocity vector
        endPoint = (Vector2)lureBody.transform.position + relLureVelocity;
        Debug.DrawLine(lureBody.transform.position, endPoint, Color.white);

        Vector2 netMomentum = lureBody.mass * (relRodTipVelocity + relLureVelocity);

        Vector2 deltaMomentum = netMomentum - previousNetMomentum;
        this.motionForce = deltaMomentum / Time.fixedDeltaTime;

        this.lureMomentum = motionForce.magnitude;
        //this.lureMomentum = 0;  //testing

        this.previousNetMomentum = netMomentum;
    }

    private void Tension()
    {
        // If net force component away from rod tip towards lure is positive then unspool line
        Vector2 lureNetForce = lure.netForce.runtimeValue;
        this.lureStrength = lureNetForce.magnitude;

        Vector2 lureDirection = (lureBody.transform.position - rodTipBody.transform.position).normalized;

        // Get component of lure momentum away from rod tip
        this.lureVelocity = lureBody.velocity;
        this.lureSpeed = lureVelocity.magnitude;

        //Vector2 castingDirection = (Vector2)lureBody.transform.position + lureDirection;
        ////Vector2 lureLineVelocity = ((Vector2)lureBody.transform.position - lureVelocity) * rodTipDirection * lureSpeed;
        //Vector2 lureLineVelocity = castingDirection + lureVelocity;

        //// Lure velocity away from rod tip
        //////Vector2 endPoint = castingDirection * lureSpeed;    //debug
        ////Vector2 endPoint = lureLineVelocity;    //debug
        ////Debug.DrawLine(lureBody.transform.position, endPoint, Color.red);   // debug

        //// TOFIX
        //// LureLine speed should usually be less than lure speed
        ////this.lureMomentum = lureLineVelocity.magnitude * lureSpeed * lineLureMomentumMultiplier;

        ////float angle = Vector2.Angle(lureDirection, lureNetForce);
        ////this.rodToLureAngle = angle;    //testing

        ////float positiveStrength = 0f;
        ////if (angle > -90 && angle < 90)
        ////{
        ////    positiveStrength = lureNetForce.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad);
        ////}

        //float rodToLureAngle = Vector2.Angle(rodTipBody.transform.position, lureBody.transform.position);
        //float lureMovementAngle = Mathf.Atan2(lureVelocity.y, lureVelocity.x) * Mathf.Rad2Deg;
        //float deltaAngle = Mathf.DeltaAngle(lureMovementAngle, rodToLureAngle);
        //float castingSpeed = lureSpeed * (90f - deltaAngle) / 90f;
        //castingSpeed = Mathf.Clamp(castingSpeed, 0, castingSpeed);

        //this.lureMomentum = castingSpeed * lineLureMomentumMultiplier;

        //// Lure casting velocity indicator
        //Vector2 endPoint = (Vector2)rodTipBody.transform.position + rodTipBody.velocity;    //debug
        //Debug.DrawLine(rodTipBody.transform.position, endPoint, Color.red);   // debug

        this.lineStrength = Mathf.Clamp(Vector2.Dot(lureNetForce, lureDirection), 0, float.MaxValue);

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

        // Determine net strength
        //float netStrength = lineStrength - reelStrength;
        float netStrength = lineStrength + lureMomentum - reelStrength;

        // If line brake is being applied then reduce net strength towards 0
        float lineBrakeResistance = lineBrakePressure * lineBrakeForceMultiplier;

        // If net strength is positive then apply unspool resistance to reduce towards 0
        this.lineResistanceStrength = lineBrakeResistance;
        if (deltaReelControllerAngle >= 0)
        {
            this.lineResistanceStrength += lineUnspoolResistanceForceMultiplier * lineRemaining;
        }

        if (netStrength == 0f)
        {
            this.lineTensionStrength = 0f;
            return;
        }

        if (netStrength > 0)
        {
            netStrength = Mathf.Clamp(netStrength - lineResistanceStrength, 0, netStrength);
        }
        if (netStrength < 0)
        {
            netStrength = Mathf.Clamp(netStrength + lineResistanceStrength, netStrength, 0);
        }

        if (netStrength != 0)
        {
            AdjustLineOut(netStrength * spoolingSpeed);

            RotateReel(deltaLine / spoolingSpeed, Time.fixedDeltaTime);
        }

        if (netStrength == lineTensionStrength)
        {
            return;
        }

        this.lineTensionStrength = netStrength;

        if (onTensionChanged == null)
        {
            return;
        }

        onTensionChanged.Invoke(slack);
    }

    private void Slack()
    {
        float distance = Vector2.Distance(rodTipBody.transform.position, lureBody.transform.position);

        float slack = Mathf.Clamp(lineOut - distance, 0, maxLine);

        if (slack == lineSlack.runtimeValue)
        {
            return;
        }

        this.lineSlack.runtimeValue = slack;
        this.slack = slack; //testing

        if (onSlackChanged == null)
        {
            return;
        }

        onSlackChanged.Invoke(slack);
    }

    private void AdjustLineOut(float delta)
    {
        this.deltaLine = Mathf.Clamp(delta, -lineOut, lineRemaining);
        if (deltaLine == 0)
        {
            return;
        }

        // Stop reel out when no line remaining
        if (lineOut >= (maxLine - spoolThreshold) && deltaLine > 0)
        {
            return;
        }

        // Stop reel in when no line out remaining
        if (lineRemaining >= (maxLine - spoolThreshold) && deltaLine < 0)
        {
            // TODO: Player jamming sound
            return;
        }

        this.lineOut = Mathf.Clamp(lineOut + deltaLine, 0, maxLine);
        //this.lineRemaining = Mathf.Clamp(lineRemaining - deltaLine, 0, maxLine);
        this.lineRemaining = Mathf.Clamp(maxLine - lineOut, 0, maxLine);

        lineJoint.distance = lineOut;
    }

    private void RotateReel(float delta, float timeElapsed)
    {
        // Spin reel as it spools out
        this.deltaReelAngle = delta;

        Quaternion currentRotation = Quaternion.Euler(0f, 0f, -deltaReelAngle);
        reelSprite.rotation *= currentRotation;

        //Quaternion rotationDiff = currentRotation * Quaternion.Inverse(previousRotation);
        //Quaternion up = Quaternion.LookRotation(Vector2.up);
        //Quaternion normalizedRotation = up * rotationDiff;
        //this.deltaTurn = 

        this.reelRotationVelocity = deltaReelAngle / timeElapsed;
        this.currentReelAngle = previousReelAngle + delta;
        reelBody.rotation -= deltaReelAngle;
        //reelBody.angularVelocity = -rotationVelocity; // tends to skip
        //reelBody.AddTorque(rotationVelocity);

        this.previousReelRotation = currentRotation;
        this.previousReelAngle = currentReelAngle;
    }

    #endregion FixedUpdate
}