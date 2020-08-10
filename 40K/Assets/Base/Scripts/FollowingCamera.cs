using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera))]
public class FollowingCamera : MonoBehaviour
{
    public GameObject target;
    public Rect deadzone;
    [ReadOnlyInInspector]
    public Vector3 smoothPos;

    public Rect[] limits;

    protected Camera followCam;
    protected Vector3 currentVelocity;

    public void Awake()
    {
        this.smoothPos = target.transform.position;
        this.smoothPos.z = transform.position.z;
        this.currentVelocity = Vector3.zero;

        this.followCam = GetComponentInChildren<Camera>();
        if (!followCam.orthographic)
        {
            Debug.LogError("Following camera script requires an orthographic camera");
            Destroy(this);
        }
    }

    public void Update()
    {
        float localX = target.transform.position.x - transform.position.x;
        float localY = target.transform.position.y - transform.position.y;

        if (localX < deadzone.xMin)
        {
            smoothPos.x += localX - deadzone.xMin;
        }
        else if (localX > deadzone.xMax)
        {
            smoothPos.x += localX - deadzone.xMax;
        }

        if (localY < deadzone.yMin)
        {
            smoothPos.y += localY - deadzone.yMin;
        }
        else if (localY > deadzone.yMax)
        {
            smoothPos.y += localY - deadzone.yMax;
        }

        Rect camWorldRect = new Rect();
        camWorldRect.min = new Vector2(smoothPos.x - followCam.aspect * followCam.orthographicSize, smoothPos.y - followCam.orthographicSize);
        camWorldRect.max = new Vector2(smoothPos.x + followCam.aspect * followCam.orthographicSize, smoothPos.y + followCam.orthographicSize);

        for (int i = 0; i < limits.Length; ++i)
        {
            if (limits[i].Contains(target.transform.position))
            {
                Vector3 localOffsetMin = limits[i].min + camWorldRect.size * 0.5f;
                Vector3 localOffsetMax = limits[i].max - camWorldRect.size * 0.5f;

                localOffsetMin.z = localOffsetMax.z = smoothPos.z;

                smoothPos = Vector3.Max(smoothPos, localOffsetMin);
                smoothPos = Vector3.Min(smoothPos, localOffsetMax);
                break;
            }
        }

        Vector3 current = transform.position;
        current.x = smoothPos.x; // we don't smooth horizontal movement

        transform.position = Vector3.SmoothDamp(current, smoothPos, ref currentVelocity, 0.1f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FollowingCamera))]
public class FollowingCameraEditor : Editor
{
    public void OnSceneGUI()
    {
        FollowingCamera cam = target as FollowingCamera;

        Vector3[] vert =
        {
            cam.transform.position + new Vector3(cam.deadzone.xMin, cam.deadzone.yMin, 0),
            cam.transform.position + new Vector3(cam.deadzone.xMax, cam.deadzone.yMin, 0),
            cam.transform.position + new Vector3(cam.deadzone.xMax, cam.deadzone.yMax, 0),
            cam.transform.position + new Vector3(cam.deadzone.xMin, cam.deadzone.yMax, 0)
        };

        Color transp = new Color(0, 0, 0, 0);
        Handles.DrawSolidRectangleWithOutline(vert, transp, Color.red);

        for (int i = 0; i < cam.limits.Length; ++i)
        {
            Vector3[] vertLimit =
           {
                new Vector3(cam.limits[i].xMin, cam.limits[i].yMin, 0),
                new Vector3(cam.limits[i].xMax, cam.limits[i].yMin, 0),
                new Vector3(cam.limits[i].xMax, cam.limits[i].yMax, 0),
                new Vector3(cam.limits[i].xMin, cam.limits[i].yMax, 0)
            };

            Handles.DrawSolidRectangleWithOutline(vertLimit, transp, Color.green);
        }
    }
}
#endif