﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class ProfileManager : Manager
    {
        public static ProfileManager singleton = null;
        public string ProfileSaveFile = @"profiles.json";
        public Dictionary<string, Profile> AllProfiles = new Dictionary<string, Profile>();
        public Profile CurrentProfile { get; protected set; }

        public void Awake()
        {
            if (singleton == null)
            {
                DontDestroyOnLoad(gameObject);
                singleton = this;
            }
            if (singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadProfiles();
        }

        public void LoadProfiles()
        {
            CurrentProfile = null;

            string fullPath = string.Format("{0}/{1}", Application.dataPath, ProfileSaveFile);

            // Check if file exists
            if (System.IO.File.Exists(fullPath) == false)
            {
                // if no file is found create one
                System.IO.File.WriteAllText(fullPath, string.Empty);
                return;
            }

            string json = GlobalAssets.ReadTextFile(fullPath);
            if (json.Equals(string.Empty) == true)
            {
                return;
            }

            Profile[] loadedProfiles = DeserializeAccounts(json);
            foreach (Profile profile in loadedProfiles)
            {
                string username = profile.Username;
                AllProfiles.Add(username, profile);

                if (profile.IsCurrentProfile == false)
                {
                    continue;
                }

                SetCurrentProfile(profile);
            }
        }

        public virtual void SaveProfiles()
        {
            string fullPath = string.Format("{0}/{1}", Application.dataPath, ProfileSaveFile);
            string json = SerializeAccounts(new List<Profile>(AllProfiles.Values).ToArray());
            if (json.Equals(string.Empty) == true)
            {
                return;
            }

            GlobalAssets.WriteTextFile(fullPath, json);
        }

        protected string SerializeAccounts(Profile[] accounts)
        {
            string json = JsonConvert.SerializeObject(accounts);
            return json;
        }

        protected Profile[] DeserializeAccounts(string json)
        {
            Profile[] accounts = JsonConvert.DeserializeObject<Profile[]>(json);
            return accounts;
        }

        public string[] GetAllUsernames()
        {
            string[] usernames = GameplayManager.GetAllUsernames(AllProfiles);
            return usernames;
        }

        public bool HasProfile(string username) {
            return AllProfiles.ContainsKey(username);
        }

        public Profile SwitchToProfile(string username, int avatarId)
        {
            Profile profile;

            bool profileExists = AllProfiles.ContainsKey(username);
            if (profileExists == false)
            {
                profile = CreateProfile(username, avatarId);
            }
            else
            {
                profile = AllProfiles[username];
            }

            SetCurrentProfile(profile);

            return profile;
        }

        public Profile GetProfile(string username)
        {
            bool profileExists = AllProfiles.ContainsKey(username);
            if (profileExists == false)
            {
                return null;
            }
            
            Profile profile = AllProfiles[username];
            return profile;
        }

        protected void SetCurrentProfile(Profile profile)
        {
            Profile.SetCurrentProfile(CurrentProfile, profile);
            CurrentProfile = profile;
            SaveProfiles();

            UserInput userInput = GetComponent<UserInput>();
            if (userInput == null)
            {
                return;
            }

            userInput.profile = profile;

            if(profile.Ranking != null)
            {
                return;
            }

            profile.ResetRanking();
        }

        public Profile CreateProfile(string username, int avatarId)
        {
            Rank ranking = new Rank();
            Profile profile = new Profile(username, avatarId, false, ranking);
            AllProfiles.Add(username, profile);

            SaveProfiles();

            return profile;
        }
    }
}