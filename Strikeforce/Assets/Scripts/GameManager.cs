using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class GameManager : Manager
    {
        public static GameManager singleton = null;
        [HideInInspector]
        public int baseMask;
        [HideInInspector]
        public int groundMask;
        [HideInInspector]
        public int airMask;
        [HideInInspector]
        public int effectsMask;
        public GameObject tilePrefab;
        public GameObject checkpointPrefab;

        private void Awake()
        {
            if (singleton == null)
            {
                DontDestroyOnLoad(gameObject);
                singleton = this;
            }
            if (singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            this.baseMask = LayerMask.GetMask(Layers.BASE);
            this.groundMask = LayerMask.GetMask(Layers.GROUND);
            this.airMask = LayerMask.GetMask(Layers.AIR);
            this.effectsMask = LayerMask.GetMask(Layers.EFFECTS);
        }

        public string GetProperName(string name)
        {
            int index = name.IndexOf("(Clone)");
            if (index < 0)
            {
                return name;
            }

            string properName = name.Substring(0, index);
            return properName;
        }

        public Texture2D GetBuildIcon(string name)
        {
            GameObject entity = GlobalAssets.GetStructurePrefab(name);
            if (entity == null)
            {
                entity = GlobalAssets.GetVehiclePrefab(name);
            }
            if (entity == null)
            {
                return null;
            }

            Selectable controller = entity.GetComponent<Selectable>();
            Texture2D buildImage = controller.buildImage;
            return buildImage;
        }

        public static Rect CalculateSelectionBox(Bounds selectionBounds, Rect playingArea)
        {
            List<Vector3> corners = GetBoundsCorners(selectionBounds);

            // Determine the bounds on screen for the selection bounds
            Bounds screenBounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Count; i++)
            {
                screenBounds.Encapsulate(corners[i]);
            }

            // Screen coordinates start in the bottom left corner, rather than the top left corner
            float selectionBoxTop = playingArea.height - (screenBounds.center.y + screenBounds.extents.y);
            float selectionBoxLeft = screenBounds.center.x - screenBounds.extents.x;
            float selectionBoxWidth = 2 * screenBounds.extents.x;
            float selectionBoxHeight = 2 * screenBounds.extents.y;

            Rect selectionBox = new Rect(selectionBoxLeft, selectionBoxTop, selectionBoxWidth, selectionBoxHeight);
            return selectionBox;
        }

        public static List<Vector3> GetBoundsCorners(Bounds bounds)
        {
            float[] centers = new float[3];
            centers[0] = bounds.center.x;
            centers[1] = bounds.center.y;
            centers[2] = bounds.center.z;

            float[] extents = new float[3];
            extents[0] = bounds.extents.x;
            extents[1] = bounds.extents.y;
            extents[2] = bounds.extents.z;

            // Determine the screen coordinates for the corners of the selection bounds
            List<Vector3> corners = new List<Vector3>();
            float[][] edges = new float[centers.Length][];
            for (int i = 0; i < centers.Length; i++)
            {
                edges[i] = new float[] {
                    centers[i] - extents[i],
                    centers[i] + extents[i]
                };
            }

            foreach (float x in edges[0])
            {
                foreach (float y in edges[1])
                {
                    foreach (float z in edges[2])
                    {
                        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(x, y, z)));
                    }
                }
            }

            return corners;
        }
    }
}