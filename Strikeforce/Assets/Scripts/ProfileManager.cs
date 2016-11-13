using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class ProfileManager : Manager
    {
        public static ProfileManager ActiveInstance = null;
        public string ProfileSaveFile = @"profiles.json";
        public Dictionary<string, Profile> AllProfiles = new Dictionary<string, Profile>();
        public Profile CurrentProfile { get; protected set; }

        public void Awake()
        {
            if (ActiveInstance == null)
            {
                DontDestroyOnLoad(gameObject);
                ActiveInstance = this;
            }
            if (ActiveInstance != this)
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
            string[] usernames = GlobalAssets.GetAllUsernames(AllProfiles);
            return usernames;
        }

        public bool HasProfile(string username) {
            return AllProfiles.ContainsKey(username);
        }

        public Profile GetProfile(string username, int avatarId)
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

        protected void SetCurrentProfile(Profile profile)
        {
            Profile.SetCurrentProfile(CurrentProfile, profile);
            CurrentProfile = profile;

            UserInput userInput = GetComponent<UserInput>();
            if (userInput == null)
            {
                return;
            }

            userInput.profile = profile;
        }

        public Profile CreateProfile(string username, int avatarId)
        {
            Profile profile = new Profile(username, avatarId);
            AllProfiles.Add(username, profile);

            SaveProfiles();

            return profile;
        }
    }
}