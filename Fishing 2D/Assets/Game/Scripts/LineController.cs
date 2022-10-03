using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineController : MonoBehaviour
{
    [SerializeField] private Transform rodTip;
    [SerializeField] private Transform lureKnot;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int numPoints = 10;
    [SerializeField] private float limitSlope = 10;
    [SerializeField] private PlayerReel reel;
    //[SerializeField] private FloatVariable lineSlack;

    private LineRenderer line;

    void Awake()
    {
        this.line = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        if(reel == null)
        {
            return;
        }

        line.enabled = true;

        reel.onSlackChanged += OnCurvatureChanged;
    }

    void OnDisable()
    {
        line.enabled = false;

        if (reel == null)
        {
            return;
        }

        reel.onSlackChanged -= OnCurvatureChanged;
    }

    public void OnCurvatureChanged(float slack)
    {
        UpdateControlPoint(slack);
        //Draw(slack);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.line.startWidth = lineWidth;
        this.line.endWidth = lineWidth;
    }

    // Update is called once per frame
    void Update()
    {
        //if(lineSlack == null)
        if(reel.lineSlack == null)
        {
            return;
        }

        //float slack = lineSlack.runtimeValue;
        float slack = reel.lineSlack.runtimeValue;

        //UpdateControlPoint(slack);
        Draw(slack);
    }

    #region Update

    private void UpdateControlPoint(float slack)
    {
        if (slack <= 0)
        {
            //this.controlPoint.position = Vector2.zero;
            transform.position = Vector2.zero;
            return;
        }

        //if (curvature == 1f)
        //{
        //    this.controlPoint = cornerPoint;
        //    return;
        //}

        Vector2 delta = lureKnot.position - rodTip.position;
        float deltaX = Mathf.Abs(delta.x);
        float deltaY = Mathf.Abs(delta.y);

        float oppositeAngle = Mathf.Atan(deltaY / deltaX);
        float quarterHypotenuse = 0.25f * Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

        float d = Mathf.Cos(oppositeAngle) * quarterHypotenuse;

        Vector2 cornerPoint = new Vector2(rodTip.position.x, lureKnot.position.y);
        //Vector2 intersectPoint = new Vector2(rodTip.position.x + d, lure.position.y + d);
        Vector2 intersectPoint = new Vector2(cornerPoint.x + 0.25f * deltaX, cornerPoint.y + 0.75f * deltaY);

        float a = rodTip.position.x;
        float b = intersectPoint.y + limitSlope / (intersectPoint.x - a);

        float controlY = intersectPoint.y - slack;
        float controlX = -limitSlope / (controlY - b) + a;

        //this.controlPoint.position = new Vector2(controlX, controlY);
        transform.position = new Vector2(controlX, controlY);
    }

    private void Draw(float slack)
    {
        Vector3[] allPoints = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            Vector3 bezierPoint = BezierCurve.CalculateLinearBezierPoint(t, rodTip.position, lureKnot.position);

            if (slack > 0)
            {
                //bezierPoint = CalculateQuadraticBezierPoint(t, rodTip.position, controlPoint.position, lure.position);
                bezierPoint = BezierCurve.CalculateQuadraticBezierPoint(t, rodTip.position, transform.position, lureKnot.position);
            }

            allPoints[i] = bezierPoint;
        }

        line.positionCount = numPoints;
        line.SetPositions(allPoints);
    }

    #endregion Update
}