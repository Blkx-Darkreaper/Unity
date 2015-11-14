using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class PersistentEntity : MonoBehaviour {

    public string propertyName = string.Empty;
    protected struct Properties
    {
        public const string POSITION = "Position";
        public const string ROTATION = "Rotation";
        public const string SCALE = "Scale";
    }

    public virtual void SavePropertyName(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);
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
        SaveManager.SaveVector(writer, Properties.SCALE, transform.localScale);
    }

    protected virtual void SaveEnd(JsonWriter writer)
    {
        writer.WriteEndObject();
    }
}