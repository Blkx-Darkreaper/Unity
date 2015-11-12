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
    }

    public static void Load(JsonTextReader reader, GameManager gameManager)
    {
        string propertyName = string.Empty, username = string.Empty;
        int avatarId = -1;

        while (reader.Read())
        {
            if (reader.Value == null)
            {
                if (reader.TokenType != JsonToken.EndObject)
                {
                    continue;
                }

                gameManager.AddPlayerAccount(username, avatarId);
                return;
            }

            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = (string)reader.Value;
                continue;
            }

            switch (propertyName)
            {
                case USERNAME_PROPERTY:
                    username = (string)reader.Value;
                    break;

                case AVATAR_PROPERTY:
                    avatarId = (int)(System.Int64)reader.Value;
                    break;
            }
        }
    }

}