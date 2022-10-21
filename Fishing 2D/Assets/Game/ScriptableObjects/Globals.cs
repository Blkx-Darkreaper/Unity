using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : ScriptableObject
{
    public const float FLOAT_PRECISION = 0.01f;

    public static float FloorFloatToPrecision(float num)
    {
        if (Mathf.Abs(num) < Globals.FLOAT_PRECISION)
        {
            return 0f;
        }

        return num;
    }

    public struct Debug
    {
        public static void DrawCustomLine(Vector2 start, Vector2 end, int lineThickness)
        {
            DrawCustomLine(start, end, lineThickness, Color.white);
        }

        public static void DrawCustomLine(Vector2 start, Vector2 end, int lineThickness, Color colour, float duration = 0f, bool depthTest = true)
        {
            int totalLines = Mathf.Clamp(lineThickness, 1, 100);

            if(totalLines == 1)
            {
                UnityEngine.Debug.DrawLine(start, end, colour, duration, depthTest);
                return;
            }

            Camera camera = Camera.current;
            if (camera == null)
            {
                UnityEngine.Debug.LogError("Camera.current is null");
                return;
            }

            //Vector3 lineDirection = (end - start).normalized; // line direction
            //Vector3 cameraDirection = (camera.transform.position - (Vector3)start).normalized; // direction to camera
            //Vector3 normal = Vector3.Cross(lineDirection, cameraDirection); // normal vector

            //for (int i = 0; i < totalLines; i++)
            //{
            //    Vector2 lineOffset = normal * lineThickness * ((float)i / (totalLines - 1) - 0.5f);
            //    //Gizmos.DrawLine(start + lineOffset, end + lineOffset);
            //    UnityEngine.Debug.DrawLine(start + lineOffset, end + lineOffset, colour, duration, depthTest);
            //}

            Vector2 WorldUnitsInCamera;
            WorldUnitsInCamera.y = camera.orthographicSize * 2;
            WorldUnitsInCamera.x = WorldUnitsInCamera.y * Screen.width / Screen.height;

            float pixelsPerUnit = Mathf.Min(Screen.width / WorldUnitsInCamera.x,
                Screen.height / WorldUnitsInCamera.y);

            Vector2 lineDirection = (end - start).normalized;
            Vector2 orthogonal = Quaternion.Euler(0, 0, 90) * lineDirection;

            for (int i = 0; i < totalLines; i++)
            {
                int amount = Mathf.CeilToInt(i / 2f);
                int distance = (i % 2) * amount - ((i - 1) % 2) * amount;

                Vector2 lineOffset = orthogonal.normalized * distance / pixelsPerUnit;
                UnityEngine.Debug.DrawLine(start + lineOffset, end + lineOffset, colour, duration, depthTest);
            }
        }
    }
}