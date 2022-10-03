using System.Collections;
using UnityEngine;

    public class PlayerRodTipPosition : MonoBehaviour
    {
    [SerializeField] private BezierCurve rodTipPath;
    [SerializeField] private float gizmoRadius = 0.2f;
    [SerializeField] private float rodTipSpeed = 0.5f;
    [SerializeField] [Range(0, 1)] private float rodTipPathNeutralTPoint;

    private PlayerControls controls;
    private Rigidbody2D rodTipBody;

    [ReadOnlyInInspector] public Vector2 rodTipMovement;
    [ReadOnlyInInspector] [Range(0, 1)] public float targetT;
    [ReadOnlyInInspector] [Range(0, 1)] public float currentT;
    [ReadOnlyInInspector] [Range(0, 1)] public float previousT;

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

        Vector2 startPoint = BezierCurve.CalculateBezierPoint(rodTipPathNeutralTPoint, allPoints);
        transform.position = startPoint;

        this.targetT = rodTipPathNeutralTPoint;
    }

    //void OnDrawGizmos()
    void OnDrawGizmosSelected()
    {
        if(rodTipPath == null)
        {
            return;
        }

        Vector2[] allPoints = rodTipPath.GetAllPoints();
        if (allPoints.Length < 2 || allPoints.Length > 4)
        {
            return;
        }

        Gizmos.color = Color.green; //testing

        Vector2 startPoint = BezierCurve.CalculateBezierPoint(targetT, allPoints);
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
        Vector2[] allPoints = rodTipPath.GetAllPoints();
        if (allPoints.Length < 2 || allPoints.Length > 4)
        {
            return;
        }

        this.targetT = (rodTipMovement.y + 1) * rodTipPathNeutralTPoint;
        if(rodTipMovement.y > 0)
        {
            this.targetT = (1 - rodTipPathNeutralTPoint) * rodTipMovement.y + rodTipPathNeutralTPoint;
        }

        //Vector2 nextIntendedPoint = BezierCurve.CalculateBezierPoint(t, allPoints);

        float deltaT = targetT - previousT;

        float maxDeltaT = rodTipSpeed * Time.fixedDeltaTime;
        float limitedDeltaT = Mathf.Clamp(deltaT, -maxDeltaT, maxDeltaT);
        this.currentT = limitedDeltaT + previousT;

        Vector2 nextPoint = BezierCurve.CalculateBezierPoint(currentT, allPoints);

        Vector2 currentPoint = new(transform.position.x, transform.position.y);

        //Vector2 previousPoint = BezierCurve.CalculateBezierPoint(previousT, allPoints);

        Vector2 velocity = (nextPoint - currentPoint) / Time.fixedDeltaTime;
        rodTipBody.velocity = velocity;

        this.previousT = this.currentT;
    }
}