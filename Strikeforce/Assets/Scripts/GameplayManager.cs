using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class GameplayManager : Manager
    {
        public static GameplayManager singleton = null;
        public string gameName { get; protected set; }
        public string levelName;
        public Color defaultColour;
        protected VictoryCondition[] victoryConditions;
        public Dictionary<string, Profile> allProfiles = new Dictionary<string, Profile>();
        public Level[] allLevels { get; protected set; }
        public Team[] allTeams { get; protected set; }
        protected GameMenu gameMenu;
        protected TeamSelectionMenu teamMenu;
        public bool isGameInProgress { get; protected set; }
        public float elapsedGameTime { get; protected set; }

        private void Awake()
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

            this.isGameInProgress = false;

            this.gameMenu = FindObjectOfType<GameMenu>();
            this.teamMenu = FindObjectOfType<TeamSelectionMenu>();

            this.allLevels = new Level[2];

            GameObject[] allLevels = GameObject.FindGameObjectsWithTag(Tags.LEVEL);
            if (allLevels == null)
            {
                return;
            }

            this.allTeams = new Team[allLevels.Length];
            for (int i = 0; i < allLevels.Length; i++)
            {
                Level level = allLevels[i].GetComponent<Level>();
                level.LoadMap(levelName);

                Team team = allLevels[i].GetComponent<Team>();
                team.Init(i, level);

                this.allLevels[i] = level;
                this.allTeams[i] = team;
            }
        }

        protected void Start()
        {
            if (teamMenu == null)
            {
                throw new InvalidOperationException("Could not find Team Selection Menu");
            }

            MenuManager.Singleton.HideLoadingScreenDelayed();
            MenuManager.Singleton.ShowMenu(teamMenu);
        }

        protected void Update()
        {
            if (isGameInProgress == false)
            {
                return;
            }

            this.elapsedGameTime += Time.deltaTime;

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
                ResultsMenu resultsScreen = winner.player.GetComponent<ResultsMenu>();
                if (resultsScreen == null)
                {
                    return;
                }

                resultsScreen.Victory(winCondition);
                resultsScreen.enabled = true;
                Time.timeScale = 0f;
                Cursor.visible = true;
                //winner.MenuManager.ShowMenu(resultsScreen);
                winner.player.buildMode.hud.enabled = false;
                winner.player.raidMode.hud.enabled = false;
            }
        }

        public virtual Team GetOtherTeam(Team team)
        {
            Team otherTeam = team == allTeams[0] ? allTeams[1] : allTeams[0];
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
            int rank = playerAccount.Ranking.Grade;

            int teamMembers = teamToCheck.totalMembers;
            int totalTeamRank = teamToCheck.totalRank;

            Team otherTeam = GetOtherTeam(teamToCheck);
            int otherTeamMembers = otherTeam.totalMembers;
            int otherTotalTeamRank = otherTeam.totalRank;

            float threshold = 3 * (otherTeamMembers - teamMembers) + 0.5f * (otherTotalTeamRank - totalTeamRank) / rank + 1;

            int difference = rank + totalTeamRank - otherTotalTeamRank;
            if (difference > threshold)
            {
                return false;
            }

            return true;
        }

        [Command]
        public void CmdJoinTeam(string username, int teamIndex)
        {
            if (isGameInProgress == true)
            {
                return;
            }

            bool validUser = allProfiles.ContainsKey(username);
            if(validUser == false)
            {
                Debug.Log(string.Format("Invalid username {0}. Unable to join team {1}"));
                return;
            }

            Profile playerAccount = allProfiles[username];

            if(teamIndex > allTeams.Length - 1)
            {
                Debug.Log(string.Format("Invalid team index {0}. Only {1} teams available. Unable to join"));
                return;
            }

            Team teamToJoin = allTeams[teamIndex];

            teamToJoin.AddPlayer(playerAccount);

            MenuManager.Singleton.SetLoadingScreenActive(true);
        }

        [Command]
        public void CmdStartGame()
        {
            if (isGameInProgress == true)
            {
                return;
            }

            Team teamA = allTeams[0];
            int teamAPlayers = teamA.totalMembers;

            Team teamB = allTeams[1];
            int teamBPlayers = teamB.totalMembers;

            teamA.ResetRaidCountdown(teamAPlayers, teamBPlayers);
            teamB.ResetRaidCountdown(teamBPlayers, teamAPlayers);

            this.isGameInProgress = true;
            MenuManager.Singleton.SetCurrentMenu(gameMenu);
            MenuManager.Singleton.CurrentMenu.HideMenu();

            foreach (Profile playerAccount in allProfiles.Values)
            {
                Player player = playerAccount.player;
                if (player == null)
                {
                    continue;
                }

                player.StartGame();
            }
        }

        [Command]
        public void CmdLoadGame(string gameToLoad, string levelToLoad)
        {
            this.gameName = gameToLoad;
            CmdLoadLevel(levelToLoad);
        }

        [Command]
        public void CmdLoadLevel(string levelToLoad)
        {
            levelName = levelToLoad;
            //isLoading = true;
        }

        public void ExitGame()
        {
            this.gameName = string.Empty;
            this.levelName = Scenes.MatchLobby;
        }

        [Command]
        public void CmdCompleteRaid(string username, float damageInflictedDuringRaid)
        {
            bool validUser = allProfiles.ContainsKey(username);
            if (validUser == false)
            {
                Debug.Log(string.Format("Invalid username {0}. Unable to join team {1}"));
                return;
            }

            Profile playerAccount = allProfiles[username];

            // Clear checkpoint
            playerAccount.player.raidMode.previousCheckpoint = null;

            Team playerTeam = playerAccount.player.currentTeam;

            playerTeam.CompleteRaid(playerAccount, damageInflictedDuringRaid);

            if (playerTeam.raidWindowRemaining > 0)
            {
                return;
            }
            if (playerTeam.areMembersCurrentlyRaiding == true)
            {
                return;
            }

            // Close Raid window
            int playerTeamMembers = playerTeam.totalMembers;
            Team otherTeam = GetOtherTeam(playerTeam);
            int otherTeamMembers = otherTeam.totalMembers;

            playerTeam.ResetRaidCountdown(playerTeamMembers, otherTeamMembers, playerTeam.totalDamageInflictedDuringRaid, elapsedGameTime);
        }

        public void GameOver()
        {
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
            bool accountExists = allProfiles.ContainsKey(username);
            if (accountExists == false)
            {
                return null;
            }

            Profile account = allProfiles[username];
            return account;
        }

        public void AddProfile(Profile accountToAdd)
        {
            string username = accountToAdd.Username;

            bool usernameConflict = allProfiles.ContainsKey(username);
            if (usernameConflict == true)
            {
                Debug.Log(string.Format("User {0} has already joined", username));
                return;
            }

            allProfiles.Add(username, accountToAdd);
        }

    }
}