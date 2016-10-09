using UnityEngine;
using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using RTS;

public class SaveManager : Manager {

    public static void SaveGame(string filename)
    {
        SetJsonSerializerIgnoreNulls();

        string username = GameManager.activeInstance.currentPlayerAccount.username;
        string path = defaultSaveFolderName + Path.DirectorySeparatorChar + username;
        CreateDirectory(path);

        string fullPath = path + Path.DirectorySeparatorChar + filename + ".json";
        using (StreamWriter stream = new StreamWriter(fullPath))
        {
            using (JsonWriter writer = new JsonTextWriter(stream))
            {
                writer.WriteStartObject();
                SaveGameDetails(writer);
                writer.WriteEndObject();
            }
        }
    }

    public static void CreatePlayerSaveFolder(string username)
    {
        string path = defaultSaveFolderName + Path.DirectorySeparatorChar + username;
        CreateDirectory(path);
    }

    public static void SavePlayerAccounts()
    {
        SetJsonSerializerIgnoreNulls();

        string path = defaultSaveFolderName + Path.DirectorySeparatorChar + JsonProperties.GAME_DETAILS + ".json";
        using (StreamWriter stream = new StreamWriter(path))
        {
            using (JsonWriter writer = new JsonTextWriter(stream))
            {
                writer.WriteStartObject();

                writer.WritePropertyName(JsonProperties.GAME_DETAILS);
                writer.WriteStartArray();
                foreach (PlayerAccount account in GameManager.activeInstance.allPlayerAccounts.Values)
                {
                    account.Save(writer);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
        }
    }

    public static void SaveString(JsonWriter writer, string propertyName, string entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        bool isClone = entry.Contains("(");
        if (isClone == true)
        {
            entry = entry.Substring(0, entry.IndexOf("("));
        }

        writer.WriteValue(entry);
    }

    public static void SaveBoolean(JsonWriter writer, string propertyName, bool entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteValue(entry);
    }

    public static void SaveInt(JsonWriter writer, string propertyName, int entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteValue(entry);
    }

    public static void SaveFloat(JsonWriter writer, string propertyName, float entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteValue(entry);
    }

    public static void SaveColour(JsonWriter writer, string propertyName, Color entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteStartObject();

        writer.WritePropertyName("r");
        writer.WriteValue(entry.r);

        writer.WritePropertyName("g");
        writer.WriteValue(entry.g);

        writer.WritePropertyName("b");
        writer.WriteValue(entry.b);

        writer.WritePropertyName("a");
        writer.WriteValue(entry.a);

        writer.WriteEndObject();
    }

    public static void SaveVector(JsonWriter writer, string propertyName, Vector3 entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteStartObject();

        writer.WritePropertyName("x");
        writer.WriteValue(entry.x);

        writer.WritePropertyName("y");
        writer.WriteValue(entry.y);

        writer.WritePropertyName("z");
        writer.WriteValue(entry.z);

        writer.WriteEndObject();
    }

    public static void SaveQuaternion(JsonWriter writer, string propertyName, Quaternion entry)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteStartObject();

        writer.WritePropertyName("x");
        writer.WriteValue(entry.x);

        writer.WritePropertyName("y");
        writer.WriteValue(entry.y);

        writer.WritePropertyName("z");
        writer.WriteValue(entry.z);

        writer.WritePropertyName("w");
        writer.WriteValue(entry.w);

        writer.WriteEndObject();
    }

    public static void SaveStringArray(JsonWriter writer, string propertyName, string[] allEntries)
    {
        if (writer == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        writer.WriteStartArray();
        foreach (string entry in allEntries)
        {
            writer.WriteValue(entry);
        }
        writer.WriteEndArray();
    }

    private static void SaveGameDetails(JsonWriter writer)
    {
		if (writer == null)
		{
			return;
		}

        SaveEnvironment(writer);
        SaveTerrain(writer);
        SaveCamera(writer);
        SaveResources(writer);
        SavePlayers(writer);
    }

    private static void SavePlayers(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        GameObject[] playerGameObjects = GameObject.FindGameObjectsWithTag(Tags.PLAYER);
        if (playerGameObjects.Length == 0)
        {
            return;
        }

        writer.WritePropertyName(JsonProperties.PLAYERS);

        writer.WriteStartArray();
        foreach (GameObject gameObject in playerGameObjects)
        {
            PlayerController player = gameObject.GetComponent<PlayerController>();
            if (player == null)
            {
                continue;
            }

            player.Save(writer);
        }
        writer.WriteEndArray();
    }

    private static void SaveResources(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        GameObject[] resourceGameObjects = GameObject.FindGameObjectsWithTag(Tags.RESOURCE);
        if (resourceGameObjects.Length == 0)
        {
            return;
        }

        writer.WritePropertyName(JsonProperties.RESOURCES);

        writer.WriteStartArray();
        foreach (GameObject gameObject in resourceGameObjects)
        {
            ResourceController resource = gameObject.GetComponent<ResourceController>();
            if (resource == null)
            {
                continue;
            }

            resource.Save(writer);
        }
        writer.WriteEndArray();
    }

    private static void SaveCamera(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        PersistentEntity camera = GameObject.FindGameObjectWithTag(Tags.MAIN_CAMERA).GetComponent<PersistentEntity>();
        if (camera != null)
        {
            writer.WritePropertyName(JsonProperties.CAMERA);
            camera.Save(writer);
        }
    }

    private static void SaveTerrain(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        PersistentEntity ground = GameObject.FindGameObjectWithTag(Tags.GROUND).GetComponent<PersistentEntity>();
        if (ground != null)
        {
            writer.WritePropertyName(JsonProperties.GROUND);
            ground.Save(writer);
        }
    }

    private static void SaveEnvironment(JsonWriter writer)
    {
        if (writer == null)
        {
            return;
        }

        PersistentEntity sun = GameObject.FindGameObjectWithTag(Tags.SUN).GetComponent<PersistentEntity>();
        if (sun != null)
        {
            writer.WritePropertyName(JsonProperties.SUN);
            sun.Save(writer);
        }
    }
}