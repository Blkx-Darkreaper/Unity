using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class GameManager : Manager
    {
        public static GameManager Singleton = null;
        protected VictoryCondition[] victoryConditions;
        public Dictionary<string, Profile> AllProfiles = new Dictionary<string, Profile>();
        public string GameName { get; protected set; }
        public string LevelName;
        public Level[] AllLevels { get; protected set; }
        public Team[] AllTeams { get; protected set; }
        public bool IsGameInProgress { get; protected set; }
        public float ElapsedGameTime { get; protected set; }
        public Color DefaultColour;
        [HideInInspector]
        public int BaseMask;
        [HideInInspector]
        public int GroundMask;
        [HideInInspector]
        public int AirMask;
        [HideInInspector]
        public int EffectsMask;
        public GameObject TilePrefab;
        public GameObject CheckpointPrefab;
        protected int nextEntityId = 0;
        protected Dictionary<int, Entity> allGameEntities;
        public int MaxEntities = 1000;
        protected TeamSelectionMenu teamMenu;
        protected GameMenu gameMenu;

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

            this.IsGameInProgress = false;

            this.BaseMask = LayerMask.GetMask(Layers.BASE);
            this.GroundMask = LayerMask.GetMask(Layers.GROUND);
            this.AirMask = LayerMask.GetMask(Layers.AIR);
            this.EffectsMask = LayerMask.GetMask(Layers.EFFECTS);

            this.teamMenu = FindObjectOfType<TeamSelectionMenu>();
            this.gameMenu = FindObjectOfType<GameMenu>();

            this.AllLevels = new Level[2];
            this.allGameEntities = new Dictionary<int, Entity>();

            GameObject[] allLevels = GameObject.FindGameObjectsWithTag(Tags.LEVEL);
            if (allLevels == null)
            {
                return;
            }

            this.AllTeams = new Team[allLevels.Length];
            for (int i = 0; i < allLevels.Length; i++)
            {
                Level level = allLevels[i].GetComponent<Level>();
                level.LoadMap(LevelName);

                Team team = allLevels[i].GetComponent<Team>();
                team.SetHomeBase(level);

                this.AllLevels[i] = level;
                this.AllTeams[i] = team;
            }
        }

        protected void Start()
        {
            if(teamMenu == null)
            {
                throw new InvalidOperationException("Could not find Team Selection Menu");
            }

            MenuManager.Singleton.HideLoadingScreenDelayed();
            MenuManager.Singleton.ShowMenu(teamMenu);
        }

        protected void Update()
        {
            if(IsGameInProgress == false)
            {
                return;
            }

            this.ElapsedGameTime += Time.deltaTime;

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
                ResultsMenu resultsScreen = winner.Player.GetComponent<ResultsMenu>();
                if (resultsScreen == null)
                {
                    return;
                }

                resultsScreen.Victory(winCondition);
                resultsScreen.enabled = true;
                Time.timeScale = 0f;
                Cursor.visible = true;
                //winner.MenuManager.ShowMenu(resultsScreen);
                winner.Player.BuildHud.enabled = false;
                winner.Player.RaidHud.enabled = false;
            }
        }

        public virtual Team GetOtherTeam(Team team)
        {
            Team otherTeam = team == AllTeams[0] ? AllTeams[1] : AllTeams[0];
            return otherTeam;
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

        public bool CanJoinTeam(Profile playerAccount, Team teamToCheck)
        {
            int rank = 0;
            if(playerAccount.Ranking != null)
            {
                rank = playerAccount.Ranking.Grade;
            }

            int teamMembers = teamToCheck.TotalMembers;
            int totalTeamRank = teamToCheck.TotalRank;

            Team otherTeam = GetOtherTeam(teamToCheck);
            int otherTeamMembers = otherTeam.TotalMembers;
            int otherTotalTeamRank = otherTeam.TotalRank;

            float threshold = 3 * (otherTeamMembers - teamMembers) + 0.5f * (otherTotalTeamRank - totalTeamRank) / rank;

            int difference = rank + totalTeamRank - otherTotalTeamRank;
            if(difference > threshold)
            {
                return false;
            }

            return true;
        }

        public void JoinTeam(Profile playerAccount, Team teamToJoin)
        {
            if(IsGameInProgress == true)
            {
                return;
            }

            teamToJoin.AddPlayer(playerAccount);
        }

        public void StartGame()
        {
            if(IsGameInProgress == true)
            {
                return;
            }

            Team teamA = AllTeams[0];
            int teamAPlayers = teamA.TotalMembers;

            Team teamB = AllTeams[1];
            int teamBPlayers = teamB.TotalMembers;

            teamA.ResetRaidCountdown(teamAPlayers, teamBPlayers);
            teamB.ResetRaidCountdown(teamBPlayers, teamAPlayers);

            this.IsGameInProgress = true;

            foreach(Profile playerAccount in AllProfiles.Values)
            {
                Player player = playerAccount.Player;
                if(player == null)
                {
                    continue;
                }

                player.StartGame();
            }
        }

        public void LoadGame(string gameToLoad, string levelToLoad)
        {
            GameName = gameToLoad;
            LoadLevel(levelToLoad);
        }

        public void LoadLevel(string levelToLoad)
        {
            LevelName = levelToLoad;
            //isLoading = true;
        }

        public void ExitGame()
        {
            GameName = string.Empty;
            LevelName = Scenes.MainMenu;
        }

        public void CompleteRaid(Profile playerAccount, float damageInflictedDuringRaid)
        {
            // Clear checkpoint
            playerAccount.Player.PreviousCheckpoint = null;

            Team playerTeam = playerAccount.Player.CurrentTeam;

            playerTeam.CompleteRaid(playerAccount, damageInflictedDuringRaid);

            if (playerTeam.RaidWindowRemaining > 0)
            {
                return;
            }
            if (playerTeam.AreMembersCurrentlyRaiding == true)
            {
                return;
            }

            // Close Raid window
            int playerTeamMembers = playerTeam.TotalMembers;
            Team otherTeam = GetOtherTeam(playerTeam);
            int otherTeamMembers = otherTeam.TotalMembers;

            playerTeam.ResetRaidCountdown(playerTeamMembers, otherTeamMembers, playerTeam.TotalDamageInflictedDuringRaid, ElapsedGameTime);
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
            bool accountExists = AllProfiles.ContainsKey(username);
            if (accountExists == false)
            {
                return null;
            }

            Profile account = AllProfiles[username];
            return account;
        }

        public void AddProfile(Profile accountToAdd)
        {
            string username = accountToAdd.Username;

            bool usernameConflict = AllProfiles.ContainsKey(username);
            if (usernameConflict == true)
            {
                Debug.Log(string.Format("User {0} has already joined", username));
                return;
            }

            AllProfiles.Add(username, accountToAdd);
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