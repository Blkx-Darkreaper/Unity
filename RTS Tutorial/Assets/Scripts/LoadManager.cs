using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RTS;
using Newtonsoft.Json;

public class LoadManager : Manager {

    public static void LoadGame(string filename, string levelName)
    {
        string username = GameManager.activeInstance.currentPlayerAccount.username;
        string path = defaultSaveFolderName + Path.DirectorySeparatorChar + username + Path.DirectorySeparatorChar + filename + ".json";
        bool fileExists = File.Exists(path);
        if (fileExists == false)
        {
            Debug.Log(string.Format("Unabled to find {0}. Loading failed", path));
            return;
        }

        string input;
        using (StreamReader stream = new StreamReader(path))
        {
            input = stream.ReadToEnd();
        }
        if (input == null)
        {
            return;
        }

        Debug.Log(string.Format("Loading level {0} from save file {1}", levelName, filename));

        using (JsonTextReader reader = new JsonTextReader(new StringReader(input)))
        {
            while (reader.Read() == true)
            {
                if (reader.Value == null)
                {
                    continue;
                }
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propertyName = LoadString(reader);
                switch (propertyName)
                {
                    case JsonProperties.SUN:
                        LoadEnvironment(reader);
                        break;

                    case JsonProperties.GROUND:
                        LoadTerrain(reader);
                        break;

                    case JsonProperties.CAMERA:
                        LoadCamera(reader);
                        break;

                    case JsonProperties.RESOURCES:
                        LoadResources(reader);
                        break;

                    case JsonProperties.PLAYERS:
                        LoadPlayers(reader);
                        break;
                }
            }
        }
    }

    public static void LoadGameDetails()
    {
        GameManager.activeInstance.allPlayerAccounts.Clear();

        string path = defaultSaveFolderName + Path.DirectorySeparatorChar + JsonProperties.GAME_DETAILS + ".json";

        bool fileNotFound = !File.Exists(path);
        if (fileNotFound == true)
        {
            return;
        }

        string input;
        using (StreamReader stream = new StreamReader(path))
        {
            input = stream.ReadToEnd();
        }

        if (input == null)
        {
            return;
        }

        using (JsonTextReader reader = new JsonTextReader(new StringReader(input)))
        {
            while (reader.Read() == true)
            {
                if (reader.Value == null)
                {
                    continue;
                }
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string propertyName = LoadString(reader);
                switch (propertyName)
                {
					case JsonProperties.GAME_DETAILS:					
						LoadPlayerAccounts(reader);
                        break;
                }
            }
        }
    }

	public static void LoadPlayerAccounts(JsonReader reader) {
		while (reader.Read() == true)
		{
			if (reader.TokenType == JsonToken.StartObject)
			{
                PlayerAccount.Load(reader);
			}
			
			if (reader.TokenType == JsonToken.EndArray)
			{
				return;
			}
		}
	}

    public static string LoadString(JsonReader reader)
    {
        string value = string.Empty;
        if (reader == null)
        {
            return value;
        }

        value = (string)reader.Value;
        return value;
    }

    public static bool LoadBoolean(JsonReader reader)
    {
        bool value = false;
        if (reader == null)
        {
            return value;
        }

        value = (bool)reader.Value;
        return value;
    }

    public static int LoadInt(JsonReader reader)
    {
        int value = 0;
        if (reader == null)
        {
            return value;
        }

        value = (int)(Int64)reader.Value;
        return value;
    }

    public static float LoadFloat(JsonReader reader)
    {
        float value = 0f;
        if (reader == null)
        {
            return value;
        }

        value = (float)Convert.ToDouble(reader.Value);
        return value;
    }

    public static Color LoadColour(JsonReader reader)
    {
        Color colour = Color.white;
        if (reader == null)
        {
            return colour;
        }

        string propertyName = string.Empty;
        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndObject)
                {
                    continue;
                }

                return colour;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = LoadString(reader);
                continue;
            }

            switch (propertyName)
            {
                case "r":
                    colour.r = LoadFloat(reader);
                    break;

                case "g":
                    colour.g = LoadFloat(reader);
                    break;

                case "b":
                    colour.b = LoadFloat(reader);
                    break;

                case "a":
                    colour.a = LoadFloat(reader);
                    break;
            }
        }

        return colour;
    }

    public static Vector3 LoadVector(JsonReader reader)
    {
        Vector3 vector = Vector3.zero;
        if (reader == null)
        {
            return vector;
        }

        string propertyName = string.Empty;
        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndObject)
                {
                    continue;
                }

                return vector;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
				propertyName = LoadString(reader);
                continue;
            }

            switch (propertyName)
            {
                case "x":
					vector.x = (float)Convert.ToDouble(reader.Value);
                    break;

                case "y":
					vector.y = (float)Convert.ToDouble(reader.Value);
				    break;

                case "z":
                    vector.z = (float)Convert.ToDouble(reader.Value);
                    break;
            }
        }

        return vector;
    }

    public static Quaternion LoadQuaternion(JsonReader reader)
    {
        Quaternion quaternion = Quaternion.identity;
        if (reader == null)
        {
            return quaternion;
        }

        string propertyName = string.Empty;
        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndObject)
                {
                    continue;
                }

                return quaternion;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
				propertyName = LoadString(reader);
                continue;
            }
            
            switch (propertyName)
            {
                case "x":
                    quaternion.x = (float)Convert.ToDouble(reader.Value);
                    break;

                case "y":
                    quaternion.y = (float)Convert.ToDouble(reader.Value);
                    break;

                case "z":
                    quaternion.z = (float)Convert.ToDouble(reader.Value);
                    break;

                case "w":
                    quaternion.w = (float)Convert.ToDouble(reader.Value);
                    break;
            }
        }

        return quaternion;
    }

    public static string[] LoadStringArray(JsonReader reader)
    {
        List<string> values = new List<string>();
        if (reader == null)
        {
            return values.ToArray();
        }

        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndArray)
                {
                    continue;
                }

                return values.ToArray();
            }

            string valueToAdd = LoadString(reader);
            values.Add(valueToAdd);
        }

        return values.ToArray();
    }

    private static void LoadPlayers(JsonReader reader)
    {
        if (reader == null)
        {
            return;
        }

        while (reader.Read() == true)
        {
            if (reader.TokenType == JsonToken.EndArray)
            {
                return;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                continue;
            }

            GameObject prefab = GameManager.activeInstance.GetPlayerPrefab();
            LoadEntity(reader, prefab);
        }
    }

    private static void LoadResources(JsonReader reader)
    {
        if (reader == null)
        {
            return;
        }

        string propertyName = string.Empty;
        string resourceName = string.Empty;

        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndArray)
                {
                    continue;
                }

                return;
            }

            if (reader.TokenType != JsonToken.PropertyName)
            {
                continue;
            }

            propertyName = LoadString(reader);
            reader.Read();

            bool isNameProperty = propertyName.Equals(ResourceController.nameProperty);
            if (isNameProperty == false)
            {
                continue;
            }

            resourceName = LoadString(reader);
            GameObject prefab = GameManager.activeInstance.GetResourcePrefab(resourceName);
            LoadEntity(reader, prefab);
        }
    }

    private static void LoadCamera(JsonReader reader)
    {
        if (reader == null)
        {
            return;
        }

        GameObject gameObjectToLoad = Camera.main.gameObject;
        PersistentEntity.Load(reader, gameObjectToLoad);
    }

    private static void LoadTerrain(JsonReader reader)
    {
        if (reader == null)
        {
            return;
        }

        LoadPrimitiveEntity(reader, JsonProperties.GROUND);
    }

    private static void LoadEnvironment(JsonReader reader)
    {
        if (reader == null)
        {
            return;
        }

        LoadPrimitiveEntity(reader, JsonProperties.SUN);
    }

    private static void LoadPrimitiveEntity(JsonReader reader, string entityName)
    {
        GameObject prefab = GameManager.activeInstance.GetEntityPrefab(entityName);
        if (prefab == null)
        {
            return;
        }

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Vector3 scale = Vector3.one;
        GameObject clone = (GameObject)GameObject.Instantiate(prefab, position, rotation);
        clone.name = GameManager.activeInstance.GetProperName(clone.name);
        clone.transform.localScale = scale;

        PersistentEntity entity = clone.GetComponent<PersistentEntity>();
        if (entity == null)
        {
            return;
        }

        entity.Load(reader);
    }

    private static void LoadEntity(JsonReader reader, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject clone = (GameObject)GameObject.Instantiate(prefab);
        clone.name = GameManager.activeInstance.GetProperName(clone.name);
        PersistentEntity entity = clone.GetComponent<PersistentEntity>();
        if (entity == null)
        {
            return;
        }

        entity.Load(reader);
    }
}