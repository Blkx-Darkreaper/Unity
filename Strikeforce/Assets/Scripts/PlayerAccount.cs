using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class PlayerAccount : NetworkBehaviour
    {
        private const string USERNAME_PROPERTY = "Username";
        private const string AVATAR_PROPERTY = "Avatar";
        public string Username { get; protected set; }
        public int AvatarId { get; protected set; }
        public Player player;

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

            this.Username = username;
            this.AvatarId = avatarId;
        }

        private string GetDefaultUsername()
        {
            if (GameManager.ActiveInstance == null)
            {
                return null;
            }

            return GameManager.ActiveInstance.DefaultUsername;
        }

        public void Save(JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(USERNAME_PROPERTY);
            writer.WriteValue(Username);
            writer.WritePropertyName(AVATAR_PROPERTY);
            writer.WriteValue(AvatarId);

            writer.WriteEndObject();

            Debug.Log(string.Format("Saved {0}'s account", Username));
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

                    GameManager.ActiveInstance.AddPlayerAccount(username, avatarId);
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
}