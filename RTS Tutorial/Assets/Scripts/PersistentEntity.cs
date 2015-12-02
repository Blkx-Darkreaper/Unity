using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using RTS;

public class PersistentEntity : MonoBehaviour {

    public string entityName;
    public int entityId { get; set; }
    protected bool isLoadedFromSave = false;
    protected struct Properties
    {
        public const string POSITION = "Position";
        public const string ROTATION = "Rotation";
        public const string SCALE = "Scale";
    }

    protected virtual void Awake()
    {
        GameManager.activeInstance.RegisterGameEntity(this);
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
        SaveManager.SaveVector(writer, Properties.POSITION, transform.position);
        SaveManager.SaveQuaternion(writer, Properties.ROTATION, transform.rotation);
        SaveManager.SaveVector(writer, Properties.SCALE, transform.localScale); // Last property to save
    }

    protected virtual void SaveEnd(JsonWriter writer)
    {
        writer.WriteEndObject();

        Debug.Log(string.Format("Saved entity {0}, ", entityId, entityName));
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

        PersistentEntity entity = gameObject.GetComponent<PersistentEntity>();
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

			if(reader.TokenType != JsonToken.PropertyName) {
				continue;
			}
//            if (reader.TokenType != JsonToken.StartObject || reader.TokenType != JsonToken.StartArray)
//            {
//                continue;
//            }
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
            case Properties.POSITION:
                transform.position = LoadManager.LoadVector(reader);
                break;

            case Properties.ROTATION:
                transform.rotation = LoadManager.LoadQuaternion(reader);
                break;

            case Properties.SCALE:
                transform.localScale = LoadManager.LoadVector(reader);
                loadCompleted = true;   // Last property to load
                break;
        }

        return loadCompleted;
    }

    protected virtual void LoadEnd(bool loadingComplete)
    {
        if (loadingComplete == false)
        {
            Debug.Log(string.Format("Failed to load {0}", entityName));
            GameManager.activeInstance.DestroyGameEntity(gameObject);
            return;
        }

        isLoadedFromSave = true;
        Debug.Log(string.Format("Loaded {0}", entityName));
    }
}