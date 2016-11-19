using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class GameManager : Manager
    {
        public static GameManager ActiveInstance = null;
        protected bool isLoading = false;
        public GameObject[] Structures;
        private Dictionary<string, GameObject> allStructures;
        public GameObject[] Units;
        private Dictionary<string, GameObject> allUnits;
        public GameObject[] Entities;
        private Dictionary<string, GameObject> allEntities;
        public GameObject Player;
        protected VictoryCondition[] victoryConditions;
        public bool IsMenuOpen { get; set; }
        public Dictionary<string, Profile> AllPlayerAccounts = new Dictionary<string, Profile>();
        public Profile CurrentPlayerAccount { get; protected set; }
        public string CurrentGameName { get; protected set; }
        public string CurrentLevelName { get; protected set; }
        public Color DefaultColour;
        protected int nextId = 0;
        protected Dictionary<int, Entity> allGameEntities;
        public int MaxEntities = 1000;

        private void Awake()
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

            allGameEntities = new Dictionary<int, Entity>();

            InitStructures();
            InitUnits();
            InitEntities();
        }

        protected void OnLevelWasLoaded()
        {
            if (isLoading == false)
            {
                return;
            }

            isLoading = false;
            LoadDetails();
            Time.timeScale = 1f;
            IsMenuOpen = false;
        }

        protected void Update()
        {
            if (victoryConditions == null)
            {
                return;
            }

            foreach (VictoryCondition winCondition in victoryConditions)
            {
                bool gameFinished = winCondition.IsGameOver();
                if (gameFinished == false)
                {
                    continue;
                }

                Debug.Log(string.Format("Win condition {0} met"));
                Profile winner = winCondition.GetWinningPlayer();
                EndGameMenu resultsScreen = winner.Player.PlayerHud.GetComponent<EndGameMenu>();
                if (resultsScreen == null)
                {
                    return;
                }

                resultsScreen.Victory(winCondition);
                resultsScreen.enabled = true;
                Time.timeScale = 0f;
                Cursor.visible = true;
                IsMenuOpen = true;
                winner.Player.PlayerHud.enabled = false;
            }
        }

        protected void LoadDetails()
        {
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(Tags.PLAYER);
            List<Profile> activePlayers = new List<Profile>();
            foreach (GameObject gameObject in playerObjects)
            {
                Profile player = gameObject.GetComponent<Profile>();
                if (player == null)
                {
                    continue;
                }

                activePlayers.Add(player);
            }

            victoryConditions = GameObject.FindObjectsOfType(typeof(VictoryCondition)) as VictoryCondition[];
            if (victoryConditions == null)
            {
                return;
            }

            foreach (VictoryCondition winCondition in victoryConditions)
            {
                winCondition.activePlayers = activePlayers.ToArray();
            }
        }

        private void InitStructures()
        {
            InitCollection<Structure>(ref allStructures, Structures, "Structure");
        }

        private void InitUnits()
        {
            InitCollection<Selectable>(ref allUnits, Units, "Unit");
        }

        private void InitEntities()
        {
            InitCollection<Entity>(ref allEntities, Entities, "Entity");
        }

        private void InitCollection<T>(ref Dictionary<string, GameObject> collection, GameObject[] items, string className)
        where T : Entity
        {
            collection = new Dictionary<string,
            GameObject>();
            foreach (GameObject gameObject in items)
            {
                T controller = gameObject.GetComponent<T>();
                if (controller == null)
                {
                    Debug.Log(string.Format("{0} gameobject has no controller", gameObject.name));
                    continue;
                }

                SetEntityName(gameObject, controller);
                string name = controller.name;
                if (name.Equals(string.Empty) == true)
                {
                    Debug.Log(string.Format("{0} {1} has no name", className, gameObject.ToString()));
                    continue;
                }

                collection.Add(name, gameObject);
            }
        }

        public void LoadGame(string gameToLoad, string levelToLoad)
        {
            CurrentGameName = gameToLoad;
            LoadLevel(levelToLoad);
        }

        public void LoadLevel(string levelToLoad)
        {
            CurrentLevelName = levelToLoad;
            isLoading = true;
        }

        public void ExitGame()
        {
            CurrentGameName = string.Empty;
            CurrentLevelName = Levels.MainMenu;
        }

        public void GameOver()
        {
        }

        public string GetProperName(string cloneName)
        {
            int index = cloneName.IndexOf("(Clone)");
            if (index < 0)
            {
                return cloneName;
            }

            string properName = cloneName.Substring(0, index);
            return properName;
        }

        public void RegisterGameEntity(Entity spawnedEntity)
        {
            int id;
            bool idInUse = true;
            do
            {
                id = GetNextUniqueId();
                idInUse = allGameEntities.ContainsKey(id);

                int count = allGameEntities.Count;
                if (count > MaxEntities)
                {
                    DestroyGameEntity(spawnedEntity);
                    Debug.Log(string.Format("Entity limit reached"));
                    return;
                }

            } while (idInUse == true);

            spawnedEntity.entityId = id;
            allGameEntities.Add(id, spawnedEntity);
        }

        public Entity GetGameEntityById(int id)
        {
            bool entityExists = allGameEntities.ContainsKey(id);
            if (entityExists == false)
            {
                return null;
            }

            Entity entity = allGameEntities[id];
            return entity;
        }

        public void DestroyGameEntity(GameObject gameObjectToDestroy)
        {
            Entity entityToDestroy = gameObjectToDestroy.GetComponent<Entity>();
            if (entityToDestroy == null)
            {
                return;
            }

            DestroyGameEntity(entityToDestroy);
        }

        public void DestroyGameEntity(Entity entityToDestroy)
        {
            int id = entityToDestroy.entityId;

            Entity toCheck = allGameEntities[id];
            if (toCheck != entityToDestroy)
            {
                Debug.Log(string.Format("{0} entity does not match registered entity with id: {1}", entityToDestroy.name, id));
                return;
            }

            allGameEntities.Remove(id);
            Destroy(entityToDestroy.gameObject);
        }

        public int GetNextUniqueId()
        {
            int id = ActiveInstance.nextId;
            ActiveInstance.nextId++;
            if (ActiveInstance.nextId >= int.MaxValue)
            {
                ActiveInstance.nextId = 0;
            }

            return id;
        }

        private void SetEntityName(GameObject gameObject, Entity entity)
        {
            if (entity.name == null)
            {
                entity.name = string.Empty;
            }

            if (entity.name.Equals(string.Empty) == false)
            {
                return;
            }

            entity.name = gameObject.name;
            if (entity.name.Equals(string.Empty) == false)
            {
                return;
            }

            entity.name = gameObject.tag;
        }

        public GameObject GetPrefab(string name)
        {
            GameObject entity = GetStructurePrefab(name);
            if (entity != null)
            {
                return entity;
            }

            entity = GetUnitPrefab(name);
            if (entity != null)
            {
                return entity;
            }

            entity = GetEntityPrefab(name);
            return entity;
        }

        public GameObject GetStructurePrefab(string name)
        {
            bool exists = allStructures.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject structure = allStructures[name];
            return structure;
        }

        public GameObject GetUnitPrefab(string name)
        {
            bool exists = allUnits.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject unit = allUnits[name];
            return unit;
        }

        public GameObject GetEntityPrefab(string name)
        {
            bool exists = allEntities.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            GameObject entity = allEntities[name];
            return entity;
        }

        public GameObject GetPlayerPrefab()
        {
            return Player;
        }

        public string[] GetAllPlayerNames() {
            return GlobalAssets.GetAllUsernames(AllPlayerAccounts);
        }

        public Profile GetPlayerAccount(string username)
        {
            bool accountExists = AllPlayerAccounts.ContainsKey(username);
            if (accountExists == false)
            {
                return null;
            }

            Profile account = AllPlayerAccounts[username];
            return account;
        }

        public void SetCurrentPlayerAccount(string username, int avatarId)
        {
            bool playerExists = AllPlayerAccounts.ContainsKey(username);
            if (playerExists == false)
            {
                AddPlayerAccount(username, avatarId);
            }

            CurrentPlayerAccount = AllPlayerAccounts[username];
        }

        public void AddPlayerAccount(string username, int avatarId)
        {
            AddPlayerAccount(username, avatarId, false);
        }

        public void AddPlayerAccount(string username, int avatarId, bool isSelectedProfile)
        {
            bool usernameConflict = AllPlayerAccounts.ContainsKey(username);
            if (usernameConflict == true)
            {
                Debug.Log(string.Format("Username {0} is already taken", username));
                return;
            }

            Profile accountToAdd = new Profile(username, avatarId, isSelectedProfile);
            AllPlayerAccounts.Add(username, accountToAdd);
        }

        private void VerifyAccounts()
        {
            string[] allUsernames = GetAllPlayerNames();
            int count = AllPlayerAccounts.Count;
            if (allUsernames.Length != count)
            {
                Debug.Log(string.Format("{0} usernames but {1} entries", allUsernames.Length, count));
            }

            foreach (string username in allUsernames)
            {
                Profile account = GetPlayerAccount(username);
                if (account == null)
                {
                    Debug.Log(string.Format("No account for {0}", username));
                    continue;
                }
                Debug.Log(string.Format("Username: {0}, Account Username: {1}, Account avatar id: {2}", username, account.Username, account.AvatarId));
            }
        }

        public Texture2D GetBuildIcon(string name)
        {
            GameObject entity = GetStructurePrefab(name);
            if (entity == null)
            {
                entity = GetUnitPrefab(name);
            }
            if (entity == null)
            {
                return null;
            }

            Selectable controller = entity.GetComponent<Selectable>();
            Texture2D buildImage = controller.BuildImage;
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
                edges[i] = new float[] {
					centers[i] - extents[i],
					centers[i] + extents[i]
				};
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