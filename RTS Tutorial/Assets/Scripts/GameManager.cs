using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS
{
    public class GameManager : MonoBehaviour
    {
        public GameObject[] structures;
        private Dictionary<string, GameObject> allStructures;
        public GameObject[] units;
        private Dictionary<string, GameObject> allUnits;
        public GameObject[] entities;
        private Dictionary<string, GameObject> allEntities;
        public GameObject player;

        public static GameManager activeInstance = null;

        private void Awake()
        {
            if (activeInstance == null)
            {
                DontDestroyOnLoad(gameObject);
                activeInstance = this;
            }
            if (activeInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitStructures();
            InitUnits();
            InitEntities();
        }

        private void InitStructures()
        {
            allStructures = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in structures)
            {
                StructureController controller = gameObject.GetComponent<StructureController>();
                string name = controller.entityName;
                if (name == null)
                {
                    name = gameObject.name;
                    controller.entityName = name;
                }
                if (name.Equals(string.Empty) == true)
                {
                    name = gameObject.name;
                    Debug.Log(string.Format("Structure {0} has no name", gameObject.ToString()));
                    continue;
                }

                allStructures.Add(name, gameObject);
            }
        }

        private void InitUnits()
        {
            allUnits = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in units)
            {
                UnitController controller = gameObject.GetComponent<UnitController>();
                string name = controller.entityName;
                if (name == null)
                {
                    name = gameObject.name;
                    controller.entityName = name;
                }
                if (name.Equals(string.Empty) == true)
                {
                    name = gameObject.name;
                    Debug.Log(string.Format("Unit {0} has no name", gameObject.ToString()));
                    continue;
                }

                allUnits.Add(name, gameObject);
            }
        }

        private void InitEntities()
        {
            allEntities = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in entities)
            {
                EntityController controller = gameObject.GetComponent<EntityController>();
                string name = controller.entityName;
                if (name == null)
                {
                    name = gameObject.name;
                    controller.entityName = name;
                }
                if (name.Equals(string.Empty) == true)
                {
                    name = gameObject.name;
                    Debug.Log(string.Format("Entity {0} has no name", gameObject.ToString()));
                    continue;
                }

                allEntities.Add(name, gameObject);
            }
        }

        public GameObject GetStructure(string name)
        {
            bool exists = allStructures.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject structure = allStructures[name];
            return structure;
        }

        public GameObject GetUnit(string name)
        {
            bool exists = allUnits.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject unit = allUnits[name];
            return unit;
        }

        public GameObject GetEntity(string name)
        {
            bool exists = allEntities.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject entity = allEntities[name];
            return entity;
        }

        public GameObject GetPlayer()
        {
            return player;
        }

        public Texture2D GetBuildIcon(string name)
        {
            GameObject entity = GetStructure(name);
            if (entity == null)
            {
                entity = GetUnit(name);
            }
            if (entity == null)
            {
                return null;
            }

            EntityController controller = entity.GetComponent<EntityController>();
            Texture2D buildImage = controller.buildImage;
            return buildImage;
        }

        public static Rect CalculateSelectionBox(Bounds selectionBounds, Rect playingArea)
        {
            float[] centers = new float[3];
            centers[0] = selectionBounds.center.x;
            centers[1] = selectionBounds.center.y;
            centers[2] = selectionBounds.center.z;

            float[] extents = new float[3];
            extents[0] = selectionBounds.extents.x;
            extents[1] = selectionBounds.extents.y;
            extents[2] = selectionBounds.extents.z;

            // Determine the screen coordinates for the corners of the selection bounds
            List<Vector3> corners = new List<Vector3>();
            float[][] edges = new float[centers.Length][];
            for (int i = 0; i < centers.Length; i++)
            {
                edges[i] = new float[] { centers[i] - extents[i], centers[i] + extents[i] };
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
    }
}