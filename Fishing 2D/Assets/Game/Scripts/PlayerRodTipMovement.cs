using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRodTipMovement : MonoBehaviour
{
    [SerializeField] private BezierCurve rodTipPath;
    [SerializeField] private float gizmoRadius = 0.2f;
    [SerializeField] private float rodTipInitSpeed = 0.5f;
    [SerializeField] private float rodTipAccel = 1.5f;
    //[SerializeField] private PlayerInput playerInput;

    private PlayerControls controls;
    private Rigidbody2D rodTipBody;

    [ReadOnlyInInspector] public Vector2 rodTipMovement;
    [ReadOnlyInInspector] [Range(0, 1)] public float t;
    [ReadOnlyInInspector] public int currentDirection;
    [ReadOnlyInInspector] public int previousDirection;
    [ReadOnlyInInspector] public float rodTipSpeed = 0f;
    [ReadOnlyInInspector] public Vector2 nextPoint;

    void Awake()
    {
        this.controls = new PlayerControls();

        this.rodTipBody = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.controls.Fishing.RodtipMovement.performed += context => rodTipMovement = context.ReadValue<Vector2>();
        this.controls.Fishing.RodtipMovement.canceled += context => rodTipMovement = Vector2.zero;

        Vector2[] allPoints = rodTipPath.GetAllPoints();
        if (allPoints.Length < 2 || allPoints.Length > 4)
        {
            return;
        }

        Vector2 startPoint = BezierCurve.CalculateBezierPoint(t, allPoints);
        transform.position = startPoint;
    }

    //void OnDrawGizmos()
    void OnDrawGizmosSelected()
    {
        if (rodTipPath == null)
        {
            return;
        }

        Vector2[] allPoints = rodTipPath.GetAllPoints();
        if (allPoints.Length < 2 || allPoints.Length > 4)
        {
            return;
        }

        Vector2 startPoint = BezierCurve.CalculateBezierPoint(t, allPoints);
        Gizmos.DrawWireSphere(startPoint, gizmoRadius);
    }

    void OnEnable()
    {
        controls.Fishing.RodtipMovement.Enable();
    }

    void OnDisable()
    {
        controls.Fishing.RodtipMovement.Disable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Vector2 movement = new Vector2(rodtipMovement.x, rodtipMovement.y) * Time.deltaTime;  //testing
        //transform.Translate(movement, Space.World);   //testing

        Vector2[] allPoints = rodTipPath.GetAllPoints();
        if (allPoints.Length < 2 || allPoints.Length > 4)
        {
            return;
        }

        this.previousDirection = this.currentDirection;
        this.currentDirection = (int)Mathf.RoundToInt(rodTipMovement.y);

        if(currentDirection != previousDirection || currentDirection == 0 || t == 0 || t == 1)
        {
            rodTipSpeed = rodTipInitSpeed;
            rodTipBody.velocity = Vector2.zero;
        } else
        {
            rodTipSpeed += rodTipAccel * Time.fixedDeltaTime;
        }

        if(currentDirection == 0)
        {
            return;
        }

        this.t += Time.fixedDeltaTime * rodTipMovement.y * rodTipSpeed;
        this.t = Mathf.Clamp(this.t, 0f, 1f);

        this.nextPoint = BezierCurve.CalculateBezierPoint(t, allPoints);
        //transform.position = nextPoint;
        //rodTipBody.MovePosition(nextPoint);

        Vector2 currentPoint = new(transform.position.x, transform.position.y);
        Vector2 velocity = (nextPoint - currentPoint) / Time.fixedDeltaTime;
        rodTipBody.velocity = velocity;
    }
}