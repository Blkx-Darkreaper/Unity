using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class GameManager : Manager
    {
        public static GameManager Singleton = null;
        //protected bool isLoading = false;
        protected VictoryCondition[] victoryConditions;
        public Dictionary<string, Profile> AllPlayerAccounts = new Dictionary<string, Profile>();
        public string CurrentGameName { get; protected set; }
        public string CurrentLevelName { get; protected set; }
        public Level[] CurrentLevels { get; protected set; }
        public Color DefaultColour;
        public int BaseMask;
        public int GroundMask;
        public int AirMask;
        public int EffectsMask;
        protected int nextEntityId = 0;
        protected Dictionary<int, Entity> allGameEntities;
        public int MaxEntities = 1000;

        private void Awake()
        {
            if (Singleton == null)
            {
                DontDestroyOnLoad(gameObject);
                Singleton = this;
            }
            if (Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            this.BaseMask = LayerMask.GetMask(Layers.BASE);
            this.GroundMask = LayerMask.GetMask(Layers.GROUND);
            this.AirMask = LayerMask.GetMask(Layers.AIR);
            this.EffectsMask = LayerMask.GetMask(Layers.EFFECTS);

            this.CurrentLevels = new Level[2];
            this.allGameEntities = new Dictionary<int, Entity>();

            GameObject[] levels = GameObject.FindGameObjectsWithTag(Tags.LEVEL);
            if (levels == null)
            {
                return;
            }

            for (int i = 0; i < levels.Length; i++)
            {
                this.CurrentLevels[i] = levels[i].GetComponent<Level>();
            }
        }

        protected void Start()
        {

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
                winner.MenuManager.ShowMenu(resultsScreen);
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

        public void LoadGame(string gameToLoad, string levelToLoad)
        {
            CurrentGameName = gameToLoad;
            LoadLevel(levelToLoad);
        }

        public void LoadLevel(string levelToLoad)
        {
            CurrentLevelName = levelToLoad;
            //isLoading = true;
        }

        public void ExitGame()
        {
            CurrentGameName = string.Empty;
            CurrentLevelName = Scenes.MainMenu;
        }

        public void GameOver()
        {
        }

        public string GetProperName(string name)
        {
            int index = name.IndexOf("(Clone)");
            if (index < 0)
            {
                return name;
            }

            string properName = name.Substring(0, index);
            return properName;
        }

        public void RegisterEntity(Entity spawnedItem)
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
                    RemoveEntity(spawnedItem);
                    Debug.Log(string.Format("Entity limit reached"));
                    return;
                }

            } while (idInUse == true);

            spawnedItem.EntityId = id;
            allGameEntities.Add(id, spawnedItem);
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

        public void RemoveEntity(GameObject gameObjectToRemove)
        {
            Entity entityToRemove = gameObjectToRemove.GetComponent<Entity>();
            if (entityToRemove == null)
            {
                return;
            }

            RemoveEntity(entityToRemove);
        }

        public void RemoveEntity(Entity entityToRemove)
        {
            int id = entityToRemove.EntityId;

            Entity toCheck = allGameEntities[id];
            if (toCheck != entityToRemove)
            {
                Debug.Log(string.Format("{0} entity does not match registered entity with id: {1}", entityToRemove.name, id));
                return;
            }

            allGameEntities.Remove(id);
            Destroy(entityToRemove.gameObject);
        }

        public int GetNextUniqueId()
        {
            int id = Singleton.nextEntityId;
            Singleton.nextEntityId++;
            if (Singleton.nextEntityId >= int.MaxValue)
            {
                Singleton.nextEntityId = 0;
            }

            return id;
        }

        public static string[] GetAllUsernames(Dictionary<string, Profile> playerAccounts)
        {
            int count = playerAccounts.Count;
            string[] allUsernames = new string[count];
            playerAccounts.Keys.CopyTo(allUsernames, 0);

            return allUsernames;
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
            string[] allUsernames = GameManager.GetAllUsernames(AllPlayerAccounts);
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
            GameObject entity = GlobalAssets.GetStructurePrefab(name);
            if (entity == null)
            {
                entity = GlobalAssets.GetVehiclePrefab(name);
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