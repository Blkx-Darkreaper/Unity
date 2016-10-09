using UnityEngine;
using System;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class PlayerAccount
{
    private const string USERNAME_PROPERTY = "Username";
    private const string AVATAR_PROPERTY = "Avatar";
    public string username { get; protected set; }
    public int avatarId { get; protected set; }

    public PlayerAccount(string username, int avatarId)
    {
        bool noUsername = username.Equals(string.Empty);
        if (noUsername == true)
        {
            string defaultUsername = GetDefaultUsername();
            if (defaultUsername != null)
            {
                username = defaultUsername;
            }
        }

        this.username = username;
        this.avatarId = avatarId;
    }

    private string GetDefaultUsername()
    {
        if (GameManager.activeInstance == null)
        {
            return null;
        }

        return GameManager.activeInstance.defaultUsername;
    }

    public void Save(JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(USERNAME_PROPERTY);
        writer.WriteValue(username);
        writer.WritePropertyName(AVATAR_PROPERTY);
        writer.WriteValue(avatarId);

        writer.WriteEndObject();

        Debug.Log(string.Format("Saved {0}'s account", username));
    }

    public static void Load(JsonReader reader)
    {
        string propertyName = string.Empty, username = string.Empty;
        int avatarId = -1;

        while (reader.Read() == true)
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndObject)
                {
                    continue;
                }

				GameManager.activeInstance.AddPlayerAccount(username, avatarId);
                Debug.Log(string.Format("Loaded {0}'s account", username));
                return;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = LoadManager.LoadString(reader);
                continue;
            }

            switch (propertyName)
            {
                case USERNAME_PROPERTY:
                    username = LoadManager.LoadString(reader);
                    break;

                case AVATAR_PROPERTY:
                    avatarId = LoadManager.LoadInt(reader);
                    break;
            }
        }
    }

}