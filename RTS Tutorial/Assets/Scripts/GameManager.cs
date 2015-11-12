using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RTS
{
    public class GameManager : MonoBehaviour
    {
        public GameObject[] structures;
        private Dictionary<string, GameObject> allStructures;
        public GameObject[] units;
        private Dictionary<string, GameObject> allUnits;
        public GameObject[] entities;
        private Dictionary<string, GameObject> allEntities;
        public GameObject player;
        public bool isMenuOpen { get; set; }
        private Dictionary<string, PlayerAccount> allPlayerAccounts = new Dictionary<string, PlayerAccount>();
        public PlayerAccount currentPlayerAccount { get; protected set; }
        public string defaultUsername = "NewPlayer";
        public string defaultSaveFolderName = "SavedGames";

        public struct JsonProperties
        {
            public const string PLAYERS = "Players";
        }

        public static GameManager activeInstance = null;

        private void Awake()
        {
            if (activeInstance == null)
            {
                DontDestroyOnLoad(gameObject);
                activeInstance = this;
            }
            if (activeInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitStructures();
            InitUnits();
            InitEntities();

            Load();
        }

        private void InitStructures()
        {
            allStructures = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in structures)
            {
                StructureController controller = gameObject.GetComponent<StructureController>();
                string name = controller.entityName;
                if (name == null)
                {
                    name = gameObject.name;
                    controller.entityName = name;
                }
                if (name.Equals(string.Empty) == true)
                {
                    name = gameObject.name;
                    Debug.Log(string.Format("Structure {0} has no name", gameObject.ToString()));
                    continue;
                }

                allStructures.Add(name, gameObject);
            }
        }

        private void InitUnits()
        {
            allUnits = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in units)
            {
                UnitController controller = gameObject.GetComponent<UnitController>();
                string name = controller.entityName;
                if (name == null)
                {
                    name = gameObject.name;
                    controller.entityName = name;
                }
                if (name.Equals(string.Empty) == true)
                {
                    name = gameObject.name;
                    Debug.Log(string.Format("Unit {0} has no name", gameObject.ToString()));
                    continue;
                }

                allUnits.Add(name, gameObject);
            }
        }

        private void InitEntities()
        {
            allEntities = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in entities)
            {
                EntityController controller = gameObject.GetComponent<EntityController>();
                string name = controller.entityName;
                if (name == null)
                {
                    name = gameObject.name;
                    controller.entityName = name;
                }
                if (name.Equals(string.Empty) == true)
                {
                    name = gameObject.name;
                    Debug.Log(string.Format("Entity {0} has no name", gameObject.ToString()));
                    continue;
                }

                allEntities.Add(name, gameObject);
            }
        }

        public GameObject GetStructure(string name)
        {
            bool exists = allStructures.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject structure = allStructures[name];
            return structure;
        }

        public GameObject GetUnit(string name)
        {
            bool exists = allUnits.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject unit = allUnits[name];
            return unit;
        }

        public GameObject GetEntity(string name)
        {
            bool exists = allEntities.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject entity = allEntities[name];
            return entity;
        }

        public GameObject GetPlayer()
        {
            return player;
        }

        public string[] GetAllUsernames()
        {
            int count = allPlayerAccounts.Count;
            string[] allUsernames = new string[count];
            allPlayerAccounts.Keys.CopyTo(allUsernames, 0);

            return allUsernames;
        }

        public PlayerAccount GetPlayerAccount(string username)
        {
            bool accountExists = allPlayerAccounts.ContainsKey(username);
            if (accountExists == false)
            {
                return null;
            }

            PlayerAccount account = allPlayerAccounts[username];
            return account;
        }

        public void SetCurrentPlayerAccount(string username, int avatarId)
        {
            bool playerExists = allPlayerAccounts.ContainsKey(username);
            if (playerExists == false)
            {
                AddPlayerAccount(username, avatarId);

                CreatePlayerSaveFolder(username);
                Save();
            }

            currentPlayerAccount = allPlayerAccounts[username];
        }

        public void AddPlayerAccount(string username, int avatarId)
        {
            bool usernameConflict = allPlayerAccounts.ContainsKey(username);
            if (usernameConflict == true)
            {
                Debug.Log(string.Format("Username {0} is already taken", username));
                return;
            }

            PlayerAccount accountToAdd = new PlayerAccount(username, avatarId);
            allPlayerAccounts.Add(username, accountToAdd);
        }

        private void VerifyAccounts()
        {
            string[] allUsernames = GetAllUsernames();
            int count = allPlayerAccounts.Count;
            if (allUsernames.Length != count)
            {
                Debug.Log(string.Format("{0} usernames but {1} entries", allUsernames.Length, count));
            }

            foreach (string username in allUsernames)
            {
                PlayerAccount account = GetPlayerAccount(username);
                if (account == null)
                {
                    Debug.Log(string.Format("No account for {0}", username));
                    continue;
                }
                Debug.Log(string.Format("Username: {0}, Account Username: {1}, Account avatar id: {2}",
                                         username, account.username, account.avatarId));
            }
        }

        private void CreatePlayerSaveFolder(string username)
        {
            string path = defaultSaveFolderName + Path.DirectorySeparatorChar + username;
            Directory.CreateDirectory(path);
        }

        public void Save()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            string path = defaultSaveFolderName + Path.DirectorySeparatorChar + JsonProperties.PLAYERS + ".json";
            using (StreamWriter stream = new StreamWriter(path))
            {
                using (JsonWriter writer = new JsonTextWriter(stream))
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName(JsonProperties.PLAYERS);
                    writer.WriteStartArray();
                    foreach (PlayerAccount account in allPlayerAccounts.Values)
                    {
                        account.Save(writer);
                    }
                    writer.WriteEndArray();

                    writer.WriteEndObject();
                }
            }
        }

        public void Load()
        {
            allPlayerAccounts.Clear();

            string path = defaultSaveFolderName + Path.DirectorySeparatorChar + JsonProperties.PLAYERS + ".json";

            bool fileNotFound = ! File.Exists(path);
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
                while (reader.Read())
                {
                    if (reader.Value == null)
                    {
                        continue;
                    }
                    if (reader.TokenType != JsonToken.PropertyName)
                    {
                        continue;
                    }

                    string value = (string)reader.Value;
                    switch (value)
                    {
                        case JsonProperties.PLAYERS:
                            LoadPlayers(reader);
                            break;
                    }
                }
            }
        }

        public void LoadPlayers(JsonTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    PlayerAccount.Load(reader, this);
                }

                if (reader.TokenType == JsonToken.EndArray)
                {
                    return;
                }
            }
        }

        public Texture2D GetBuildIcon(string name)
        {
            GameObject entity = GetStructure(name);
            if (entity == null)
            {
                entity = GetUnit(name);
            }
            if (entity == null)
            {
                return null;
            }

            EntityController controller = entity.GetComponent<EntityController>();
            Texture2D buildImage = controller.buildImage;
            return buildImage;
        }

        public static Rect CalculateSelectionBox(Bounds selectionBounds, Rect playingArea)
        {
            List<Vector3> corners = GetBoundsCorners(selectionBounds);

            // Determine the bounds on screen for the selection bounds
            Bounds screenBounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Count; i++)
            {
                screenBounds.Encapsulate(corners[i]);
            }

            // Screen coordinates start in the bottom left corner, rather than the top left corner
            float selectionBoxTop = playingArea.height - (screenBounds.center.y + screenBounds.extents.y);
            float selectionBoxLeft = screenBounds.center.x - screenBounds.extents.x;
            float selectionBoxWidth = 2 * screenBounds.extents.x;
            float selectionBoxHeight = 2 * screenBounds.extents.y;

            Rect selectionBox = new Rect(selectionBoxLeft, selectionBoxTop, selectionBoxWidth, selectionBoxHeight);
            return selectionBox;
        }

        public static List<Vector3> GetBoundsCorners(Bounds bounds)
        {
            float[] centers = new float[3];
            centers[0] = bounds.center.x;
            centers[1] = bounds.center.y;
            centers[2] = bounds.center.z;

            float[] extents = new float[3];
            extents[0] = bounds.extents.x;
            extents[1] = bounds.extents.y;
            extents[2] = bounds.extents.z;

            // Determine the screen coordinates for the corners of the selection bounds
            List<Vector3> corners = new List<Vector3>();
            float[][] edges = new float[centers.Length][];
            for (int i = 0; i < centers.Length; i++)
            {
                edges[i] = new float[] { centers[i] - extents[i], centers[i] + extents[i] };
            }

            foreach (float x in edges[0])
            {
                foreach (float y in edges[1])
                {
                    foreach (float z in edges[2])
                    {
                        corners.Add(Camera.main.WorldToScreenPoint(new Vector3(x, y, z)));
                    }
                }
            }

            return corners;
        }
    }
}