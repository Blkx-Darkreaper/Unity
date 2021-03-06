﻿using UnityEngine;
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
        public Player player;

        public Profile(string username, int avatarId)
        {
            this.Username = username;
            this.AvatarId = avatarId;
            this.IsCurrentProfile = true;
            this.Ranking = new Rank();
        }

        [JsonConstructor]
        public Profile(string username, int avatarId, bool isCurrentProfile, Rank ranking)
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
            this.Ranking = ranking;
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

        public void ResetRanking()
        {
            this.Ranking = new Rank();
        }

        private string GetDefaultUsername()
        {
            string defaultUsername = GlobalAssets.DefaultUsername;
            return defaultUsername;
        }

        public virtual void DropPlayer()
        {
            // Transfer player inventory to team
            player.buildMode.currentInventory.TransferAllTo(player.currentTeam.sharedInventory);

            //Network.RemoveRPCs(Player);
            //Network.DestroyPlayerObjects(Player);
            this.player = null;
        }

        public virtual void DrawPlayerAvatar()
        {
            float height = Menu.Attributes.TextHeight;
            float x = Menu.Attributes.TextHeight;
            float y = Screen.height - x - Menu.Attributes.Padding;
            DrawPlayerAvatar(x, y, height);
        }

        public virtual void DrawPlayerAvatar(float x, float y, float height)
        {
            Texture2D avatar = GlobalAssets.GetAvatar(AvatarId);
            if (avatar == null)
            {
                return;
            }

            GUI.DrawTexture(new Rect(x, y, height, height), avatar);
        }
    }
}