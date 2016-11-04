using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Entity : NetworkBehaviour
    {
        public int entityId { get; set; }
        public static string nameProperty { get { return EntityProperties.NAME; } }
        protected bool isLoadedFromSave = false;
        public float MoveSpeed, TurnSpeed;
        protected bool isMoving, isTurning;
        private Vector3 currentWaypoint;
        private Quaternion targetHeading;
        private GameObject targetEntityGameObject;
        protected struct UnitProperties
        {
            public const string IS_MOVING = "IsMoving";
            public const string IS_TURNING = "IsTurning";
            public const string WAYPOINT = "Waypoint";
            public const string TARGET_HEADING = "TargetHeading";
            public const string TARGET_ID = "TargetId";
        }
        protected struct Properties
        {
            public const string POSITION = "Position";
            public const string ROTATION = "Rotation";
            public const string SCALE = "Scale";
        }
        public Rect playingArea { get; set; }

        protected virtual void Awake()
        {
            GameManager.ActiveInstance.RegisterGameEntity(this);

            playingArea = new Rect(0f, 0f, 0f, 0f);
        }

        public void SetColliders(bool enabled)
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in allColliders)
            {
                collider.enabled = enabled;
            }
        }

        public virtual void SetWaypoint(Vector3 destination)
        {
            currentWaypoint = destination;
            isTurning = true;
            isMoving = false;
            targetEntityGameObject = null;
        }

        public virtual void SetWaypoint(Entity target)
        {
            if (target == null)
            {
                return;
            }

            SetWaypoint(target.transform.position);
            targetEntityGameObject = target.gameObject;
        }

        public void Save(JsonWriter writer)
        {
            if (writer == null)
            {
                return;
            }

            SaveStart(writer);
            SaveDetails(writer);
            SaveEnd(writer);
        }

        protected virtual void SaveStart(JsonWriter writer)
        {
            writer.WriteStartObject();
        }

        protected virtual void SaveDetails(JsonWriter writer)
        {
            SaveManager.SaveString(writer, EntityProperties.NAME, name);

            SaveManager.SaveVector(writer, Properties.POSITION, transform.position);
            SaveManager.SaveQuaternion(writer, Properties.ROTATION, transform.rotation);
            SaveManager.SaveVector(writer, Properties.SCALE, transform.localScale);

            SaveManager.SaveInt(writer, EntityProperties.ID, entityId); // Last property to save
            //SaveManager.SaveString(writer, EntityProperties.MESHES, 
        }

        protected virtual void SaveEnd(JsonWriter writer)
        {
            writer.WriteEndObject();

            Debug.Log(string.Format("Saved entity {0}, ", entityId, name));
        }

        public static void Load(JsonReader reader, GameObject gameObject)
        {
            if (reader == null)
            {
                return;
            }
            if (gameObject == null)
            {
                return;
            }

            Entity entity = gameObject.GetComponent<Entity>();
            if (entity == null)
            {
                return;
            }

            bool loadingComplete = false;

            while (reader.Read() == true)
            {
                if (reader.Value == null)
                {
                    if (reader.TokenType != JsonToken.EndObject)
                    {
                        continue;
                    }

                    entity.LoadEnd(loadingComplete);
                    return;
                }

                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }
                //            if (reader.TokenType != JsonToken.StartObject || reader.TokenType != JsonToken.StartArray)
                //            {
                //                continue;
                //            }

                string propertyName = LoadManager.LoadString(reader);
                reader.Read();

                loadingComplete = entity.LoadDetails(reader, propertyName);
            }

            entity.LoadEnd(loadingComplete);
        }

        public void Load(JsonReader reader)
        {
            if (reader == null)
            {
                return;
            }

            bool loadingComplete = false;

            while (reader.Read() == true)
            {
                if (reader.Value == null)
                {
                    if (reader.TokenType != JsonToken.EndObject)
                    {
                        continue;
                    }

                    LoadEnd(loadingComplete);
                    return;
                }

                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propertyName = LoadManager.LoadString(reader);
                reader.Read();

                loadingComplete = LoadDetails(reader, propertyName);
            }

            LoadEnd(loadingComplete);
        }

        protected virtual bool LoadDetails(JsonReader reader, string propertyName)
        {
            // Properties must be loaded in the order they were saved for loadCompleted to work properly
            bool loadCompleted = false;

            switch (propertyName)
            {
                case EntityProperties.NAME:
                    name = LoadManager.LoadString(reader);
                    break;

                case Properties.POSITION:
                    transform.position = LoadManager.LoadVector(reader);
                    break;

                case Properties.ROTATION:
                    transform.rotation = LoadManager.LoadQuaternion(reader);
                    break;

                case Properties.SCALE:
                    transform.localScale = LoadManager.LoadVector(reader);
                    break;

                case EntityProperties.ID:
                    this.entityId = LoadManager.LoadInt(reader);
                    loadCompleted = true;   // Last property to load
                    break;
            }

            return loadCompleted;
        }

        protected virtual void LoadEnd(bool loadingComplete)
        {
            if (loadingComplete == false)
            {
                Debug.Log(string.Format("Failed to load {0}", name));
                GameManager.ActiveInstance.DestroyGameEntity(gameObject);
                return;
            }

            isLoadedFromSave = true;
            Debug.Log(string.Format("Loaded {0}", name));
        }
    }
}