using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Profile
    {
        [JsonIgnore]
        private const string USERNAME_PROPERTY = "Username";
        [JsonIgnore]
        private const string AVATAR_PROPERTY = "Avatar";
        public string Username { get; set; }
        public int AvatarId { get; protected set; }
        public Rank Ranking { get; protected set; }
        public bool IsCurrentProfile { get; protected set; }
        [JsonIgnore]
        public Player Player;
        public MenuManager MenuManager;

        [JsonConstructor]
        public Profile(string username, int avatarId, bool isCurrentProfile)
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
			this.IsCurrentProfile = isCurrentProfile;
            this.MenuManager = GetComponent<MenuManager>();
        }

        public static void SetCurrentProfile(Profile previousProfile, Profile currentProfile)
        {
            currentProfile.IsCurrentProfile = true;

            if (previousProfile == null)
            {
                return;
            }

            previousProfile.IsCurrentProfile = false;
        }

        private string GetDefaultUsername()
        {
            string defaultUsername = GlobalAssets.DefaultUsername;
            return defaultUsername;
        }
    }
}