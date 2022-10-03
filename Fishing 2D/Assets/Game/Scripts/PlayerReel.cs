using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReel : MonoBehaviour
{
    //public static PlayerReel singleton;

    [Header("Reel")]
    //[SerializeField] private int maxVelocitySamples = 20;
    [SerializeField] private float reelSpeed = 1f;
    [SerializeField] private float spoolingSpeed;
    [SerializeField] private float spoolThreshold;
    [SerializeField] private float minReelRadius;
    [SerializeField] private float maxReelRadius;
    [SerializeField] private float unspoolingResistanceCoef;
    [SerializeField] private float reelForceMultiplier;
    [SerializeField] private float reelControlDistanceThreshold;
    [SerializeField] private RectTransform reelSprite;
    [SerializeField] private Rigidbody2D reelBody;

    public enum Position { Center = 0, Edge = -1, Up = 1, Right = 2, Down = 3, Left = 4 }

    [ReadOnlyInInspector] public Vector2 reelMovement;
    [ReadOnlyInInspector] public float reelTensionPercentage;
    [ReadOnlyInInspector] public bool isReelApplyingTension = false;
    [ReadOnlyInInspector] public float rotationVelocity = 0f;   // deg/s
    [ReadOnlyInInspector] public float reelVelocity = 0; // positive clockwise, negative counter-clockwise
    [ReadOnlyInInspector] public float currentReelRadius;
    [ReadOnlyInInspector] public float timeElapsed = 0f;
    [ReadOnlyInInspector] public Position latestPosition = Position.Center;
    private Quaternion previousRotation;
    [ReadOnlyInInspector] public float currentAngle;
    private float previousAngle;
    [ReadOnlyInInspector] public float deltaAngle;
    //[ReadOnlyInInspector] public float deltaTurn;

    [Header("Line")]
    [SerializeField] private float lineBrakeTensionPercentage;
    [SerializeField] private float maxLine;
    [SerializeField] private float lineBrakeForceMultiplier;
    [SerializeField] private DistanceJoint2D lineJoint;
    [SerializeField] private LineRenderer line;

    [SerializeField] public FloatVariable lineSlack;
    [SerializeField] public Vector2Variable lineTension;

    public event Action<float> onSlackChanged;
    public event Action<float> onTensionChanged;

    [ReadOnlyInInspector] public float lineBrakePressure = 0f;
    [ReadOnlyInInspector] public float lineLoad = 0f;
    [ReadOnlyInInspector] public float lineRemaining;
    [ReadOnlyInInspector] public float lineOut = 0f;
    //[ReadOnlyInInspector] public float lineSlack = 0f;
    [ReadOnlyInInspector] public float deltaLine;
    [ReadOnlyInInspector] public float lineTensionCoef = 0f;

    [Header("Rod")]
    [SerializeField] private RodTipPhysics rodTip;
    /*[SerializeField]*/ private Rigidbody2D rodTipBody;
    //[SerializeField] private Vector2Variable rodTipForceVector;

    [ReadOnlyInInspector] public Vector2 rodTipVelocity;

    [Header("Lure")]
    [SerializeField] private LurePhysics lure;
    /*[SerializeField]*/ private Rigidbody2D lureBody;

    [ReadOnlyInInspector] public Vector2 lureVelocity;
    [ReadOnlyInInspector] public float lureToRodAngle;  //testing
    [ReadOnlyInInspector] public float tensionStrength; //testing
    [ReadOnlyInInspector] public float lureStrength; //testing
    [ReadOnlyInInspector] public float rodToLureAngle;  //testing
    [ReadOnlyInInspector] public float unspoolStrength; //testing

    private PlayerControls controls;
    //private Queue<float> allVelocities;

    void Awake()
    {
        this.controls = new PlayerControls();

        //this.allVelocities = new Queue<float>(4);

        if(rodTip != null)
        {
            this.rodTipBody = rodTip.GetComponent<Rigidbody2D>();
        }

        if(lure != null)
        {
            this.lureBody = lure.GetComponent<Rigidbody2D>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.controls.Fishing.ReelMovement.performed += context => reelMovement = context.ReadValue<Vector2>();
        this.controls.Fishing.ReelMovement.canceled += context => reelMovement = Vector2.zero;

        this.controls.Fishing.LineBrake.performed += context => lineBrakePressure = context.ReadValue<float>();
        this.controls.Fishing.LineBrake.canceled += context => lineBrakePressure = 0f;

        //this.currentReelRadius = maxReelRadius;
        this.currentReelRadius = minReelRadius; //testing
        this.lineOut = lineJoint.distance;
        this.lineRemaining = maxLine - lineOut;

        this.reelTensionPercentage = 1f - lineBrakeTensionPercentage;
    }

    void OnValidate()
    {
        this.reelTensionPercentage = 1f - lineBrakeTensionPercentage;
    }

    void OnDrawGizmos()
    //void OnDrawGizmosSelected()
    {
        //DrawForceArrow();
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

        Slack();
    }

    #region Update

    private void ReelInput()
    {
        this.isReelApplyingTension = false;

        if (reelMovement.magnitude == 0)
        {
            reelBody.angularVelocity = 0;
            return;
        }

        Position position = GetPosition(reelMovement, reelControlDistanceThreshold);
        //this.latestPosition = position;

        //int posVal = (int)position;
        //int latestPosVal = (int)latestPosition;

        //int diff = posVal - latestPosVal;

        if (position == Position.Center)
        {
            reelBody.angularVelocity = 0;
            this.reelVelocity = 0f;
            this.latestPosition = position;
            //this.timeElapsed = 0f;
            return;
        }

        this.currentAngle = Mathf.Atan2(reelMovement.x, reelMovement.y) * Mathf.Rad2Deg;

        // To prevent reel from jumping to a new orientation
        if (latestPosition == Position.Center)
        {
            this.latestPosition = position;
            this.previousAngle = currentAngle;
            return;
        }

        this.latestPosition = position;

        this.isReelApplyingTension = true;

        //this.deltaAngle = currentAngle - previousAngle;
        this.deltaAngle = Mathf.DeltaAngle(previousAngle, currentAngle);

        //this.deltaLine = deltaAngle * currentReelRadius;
        this.deltaLine = deltaAngle * spoolingSpeed;
        this.deltaLine = Mathf.Clamp(deltaLine, -lineOut, lineRemaining);

        AdjustLineOut();

        RotateReel(currentAngle, Time.deltaTime);

        //if (position == Position.Edge || diff == 0)
        //{
        //    timeElapsed += Time.deltaTime;
        //    return;
        //}

        ////if (diff != 0)
        ////{
        ////    Debug.Log($"{diff} = {posVal} - {latestPosVal}");   //debug
        ////}

        ////float minTimeElapsed = 1 / Time.deltaTime;

        //if (diff == 1 || diff == -3)
        //{
        //    // Moving clockwise
        //    timeElapsed = Mathf.Clamp(timeElapsed, Time.deltaTime, float.MaxValue);
        //    //this.reelVelocity = reelSpeed / timeElapsed;

        //    float velocity = reelSpeed / timeElapsed;
        //    allVelocities.Enqueue(velocity);
        //}

        //if (diff == -1 || diff == 3)
        //{
        //    // Moving counter-clockwise
        //    timeElapsed = Mathf.Clamp(timeElapsed, Time.deltaTime, float.MaxValue);
        //    //this.reelVelocity = -reelSpeed / timeElapsed;

        //    float velocity = -reelSpeed / timeElapsed;
        //    allVelocities.Enqueue(velocity);
        //}

        //this.latestPosition = position;
        //this.timeElapsed = 0f;
    }

    private void Slack()
    {
        float distance = Vector2.Distance(rodTipBody.transform.position, lureBody.transform.position);

        float slack = Mathf.Clamp(lineOut - distance, 0, maxLine);

        if(slack == lineSlack.runtimeValue)
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
        //if(rodTipForceVector == null)
        if (rodTip.forceVector == null)
        {
            return;
        }

        this.lineTensionCoef = 0f;

        ReelPhysics();

        LineBrakePhysics();

        //Tension();

        //this.tensionStrength = this.lineTension.runtimeValue.magnitude;

        ReelUnspooling();

        //if (allVelocities.Count == 0)
        //{
        //    return;
        //}

        //// Get average velocity
        //int totalVelocities = Mathf.Clamp(maxVelocitySamples, 1, allVelocities.Count);

        //float sum = 0;
        //for(int i = 0; i < totalVelocities; i++)
        //{
        //    float velocity = allVelocities.Dequeue();
        //    sum += velocity;
        //}

        //float avgVelocity = sum / totalVelocities;
        //this.reelVelocity = avgVelocity;
        //this.reelVelocity = reelBody.angularVelocity;
        //if (reelVelocity <= 0)
        //{
        //    return;
        //}

        //// Reel in lure
        //Vector2 lureForce = (rodTip.position - lureBody.transform.position)
        //    * forceMultiplier * reelVelocity * Time.fixedDeltaTime;
        ////lureBody.AddForce(lureForce);

        //ForceManager manager = lureBody.GetComponent<ForceManager>();
        //if(manager == null)
        //{
        //    return;
        //}

        //manager.AddForce(lureForce);
    }

    #region FixedUpdate

    private void ReelPhysics()
    {
        if(isReelApplyingTension == false)
        {
            return;
        }

        this.lineTensionCoef += reelTensionPercentage;
    }

    private void LineBrakePhysics()
    {
        this.lineTensionCoef += lineBrakeTensionPercentage * lineBrakePressure;

        //this.rodTipVelocity = rodTipBody.velocity;

        ////float rodtipSpeed = rodTipVelocity.magnitude;
        ////if (rodtipSpeed < 0.01f)
        ////{
        ////    return;
        ////}

        ////line.enabled = brakePressure > 1 - brakePressureThreshold;

        //float distance = Vector2.Distance(lureBody.transform.position, transform.position);

        //Vector2 appliedVelocity = rodTipVelocity * lineBrakePressure;
        //Vector2 appliedForce = appliedVelocity * lineBrakeForceMultiplier;
        ////if (distance > 0)
        ////{
        ////    lureForce /= distance;
        ////}

        //ForceManager forces = lureBody.GetComponent<ForceManager>();
        //if (forces == null)
        //{
        //    return;
        //}

        //forces.AddForce(appliedForce);

        ////lureBody.AddForce(appliedForce);
        //lureBody.gravityScale = 1 - lineBrakePressure;

        //this.lureVelocity = lureBody.velocity;
    }

    private void Tension()
    {
        if(lineSlack.runtimeValue > 0)
        {
            return;
        }

        // Apply rod tip force to lure along line based on tension
        //Vector2 rodTipForce = rodTipForceVector.runtimeValue;
        Vector2 rodTipForce = rodTip.forceVector.runtimeValue;

        // Get angle between rod tip force and vector from lure to rod tip
        Vector2 lureVector = (lureBody.transform.position - rodTipBody.transform.position).normalized;

        float angle = Vector2.Angle(lureVector, rodTipForce);
        this.lureToRodAngle = angle;    //testing

        if (angle <= -90 || angle >= 90)
        {
            this.lineTension.runtimeValue = Vector2.zero;
            return;
        }

        // Adjust force based on tension applied
        float tensionStrength = lineTensionCoef * rodTipForce.magnitude;
        float tensionComponent = tensionStrength;

        if (angle != 0)
        {
            tensionComponent *= Mathf.Cos(angle * Mathf.Deg2Rad);
        }

        Vector2 tensionForce = lureVector * tensionComponent;
        this.lineTension.runtimeValue = tensionForce;
    }

    private void ReelUnspooling()
    {
        if(lineTensionCoef == 1)
        {
            return;
        }

        // TODO: line should unspool from lure's gravity and other forces when reel has no pressure applied
        // Unspool rate should increase as less line remains

        // If net force component away from rod tip towards lure is positive then unspool line
        Vector2 lureNetForce = lure.netForce.runtimeValue;
        this.lureStrength = lureNetForce.magnitude;

        Vector2 lureDirection = (rodTipBody.transform.position - lureBody.transform.position).normalized;

        float angle = Vector2.Angle(lureDirection, lureNetForce);
        this.rodToLureAngle = angle;    //testing

        if (angle <= -90 || angle >= 90)
        {
            return;
        }

        // Adjust force based on tension applied
        float unspoolStrength = (1 - lineTensionCoef) * lureNetForce.magnitude;
        float unspoolComponent = unspoolStrength;

        if (angle != 0)
        {
            unspoolComponent *= Mathf.Cos(angle * Mathf.Deg2Rad);
        }

        this.unspoolStrength = unspoolComponent;

        if(unspoolComponent <= 0)
        {
            return;
        }

        this.deltaLine += unspoolComponent * spoolingSpeed;
        this.deltaLine = Mathf.Clamp(deltaLine, -lineOut, lineRemaining);

        AdjustLineOut();

        // Spin reel as it spools out
        this.deltaAngle = deltaLine / spoolingSpeed;

        RotateReel(previousAngle + deltaAngle, Time.fixedDeltaTime);
    }

    #endregion FixedUpdate

    private void AdjustLineOut()
    {
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

    private void RotateReel(float angle, float timeElapsed)
    {
        Quaternion currentRotation = Quaternion.Euler(0f, 0f, -deltaAngle);
        reelSprite.rotation *= currentRotation;

        //Quaternion rotationDiff = currentRotation * Quaternion.Inverse(previousRotation);
        //Quaternion up = Quaternion.LookRotation(Vector2.up);
        //Quaternion normalizedRotation = up * rotationDiff;
        //this.deltaTurn = 

        this.rotationVelocity = deltaAngle / timeElapsed;
        reelBody.rotation -= deltaAngle;
        //reelBody.angularVelocity = -rotationVelocity; // tends to skip
        //reelBody.AddTorque(rotationVelocity);

        this.previousRotation = currentRotation;
        this.previousAngle = angle;
    }

    private Position GetPosition(Vector2 position, float threshold)
    {
        float y = position.y;
        float x = position.x;

        float h = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));

        if(h < 1 - threshold)
        {
            return Position.Center;
        }

        float absY = Mathf.Abs(y);
        if (1 - absY < threshold)
        {
            y = y / absY;
        }
        if (absY < threshold)
        {
            y = 0;
        }

        float absX = Mathf.Abs(x);
        if (1 - absX < threshold)
        {
            x = x / absX;
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
}