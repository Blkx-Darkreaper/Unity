using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Color = UnityEngine.Color;

namespace Strikeforce
{
    public class Player : Entity
    {
        public int playerId { get { return this.playerControllerId; } }
        public bool isNPC;
        protected PlayerCamera playerCamera;
        public PlayerBuildMode buildMode { get; protected set; }
        public PlayerRaidMode raidMode { get; protected set; }
        public Color colour { get; protected set; }
        public Team currentTeam { get; set; }
        protected bool isInBuildMode = true;
        protected bool hasControl = false;

        protected override void Awake()
        {
            Profile profile = ProfileManager.singleton.CurrentProfile;
            if (profile != null)
            {
                profile.player = this;
            }

            this.currentTeam = null;

            this.playerCamera = GetComponent<PlayerCamera>();
            this.buildMode = GetComponent<PlayerBuildMode>();
            this.raidMode = GetComponent<PlayerRaidMode>();
        }

        protected void Start()
        {
            Profile profile = ProfileManager.singleton.CurrentProfile;
            if (profile == null)
            {
                return;
            }

            GameplayManager.singleton.AddProfile(profile);
        }

        public override void OnStartLocalPlayer()
        {
            //CurrentRaider.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        public void StartGame()
        {
            this.currentLevel = currentTeam.homeBase;

            SetCursors();
            //SpawnRaider();  // Testing

            buildMode.hud.gameObject.SetActive(true);

            MenuManager.Singleton.HideLoadingScreenDelayed();

            this.hasControl = true;
        }

        protected void SetCursors()
        {
            // Get initial sector and spawnpoint
            Sector spawnSector = currentLevel.GetNextAvailableSector();
            Spawnpoint spawnpoint = spawnSector.Spawn;

            spawnSector.SetOwnership(this);
            playerCamera.SetOverheadCameraPosition(spawnpoint.Location);

            // Get the build cursor
            this.buildMode.buildCursor.Bounds = currentLevel.bounds;

            //this.BuyCursor = cursors[1];
            //this.BuyCursor.Bounds = BuildHud.Bounds;

            this.buildMode.buildCursor.transform.position = new Vector3(spawnpoint.Location.x, spawnpoint.Location.y, spawnpoint.Location.z);
            this.buildMode.buildCursor.transform.parent = currentLevel.gameObject.transform;   // Make position relative to base
        }

        public void StartLevelRaid(RaiderLoadout loadout)
        {
            this.hasControl = false;

            ToggleBuildRaidModes();

            // Get spawn point from enemy level
            Team enemyTeam = GameplayManager.singleton.GetOtherTeam(currentTeam);
            Level enemyLevel = enemyTeam.homeBase;

            // Fade out
            MenuManager.Singleton.ShowLoadingScreen();

            // get level starting point
            Vector3 spawnLocation = enemyLevel.GetRaiderSpawnLocation();

            // Spawn raider
            raidMode.LaunchRaider(loadout, enemyLevel, spawnLocation, ref playerCamera);

            // Fade in
            MenuManager.Singleton.HideLoadingScreenDelayed();

            this.hasControl = true;
        }

        public void RestartRaidFromCheckpoint(RaiderLoadout loadout)
        {
            this.hasControl = false;

            ToggleBuildRaidModes();

            //Fade out
            MenuManager.Singleton.ShowLoadingScreen();

            // Get enemy base
            Team enemyTeam = GameplayManager.singleton.GetOtherTeam(currentTeam);
            Level enemyLevel = enemyTeam.homeBase;

            // Get checkpoint starting point
            Vector2 checkpointLocation = this.raidMode.previousCheckpoint.location;
            int altitude = currentLevel.raiderAltitude;
            Vector3 spawnLocation = new Vector3(checkpointLocation.x, altitude, checkpointLocation.y - 5);

            // Spawn raider
            raidMode.LaunchRaider(loadout, enemyLevel, spawnLocation, ref playerCamera);

            // Fade in
            MenuManager.Singleton.HideLoadingScreenDelayed();

            this.hasControl = true;
        }

        protected void SpawnRaider()	// Testing
        {
            SetBuildMode(false);

            // Get spawn point from enemy level
            Team enemyTeam = GameplayManager.singleton.GetOtherTeam(currentTeam);
            Level enemyLevel = enemyTeam.homeBase;

            raidMode.SpawnRaider(enemyLevel, ref playerCamera);

            this.hasControl = true;
        }

        public void BugOut()
        {
            // Disable Raider control
            this.hasControl = false;

            // Lock camera position
            playerCamera.SetMainCameraVelocity(0f, 0f, 0f);

            // Fade out
            MenuManager.Singleton.ShowLoadingScreen();

            // Despawn raider from map
            raidMode.DespawnRaider();

            // Return control to Build cursor
            ToggleBuildRaidModes();

            // Fade in
            MenuManager.Singleton.HideLoadingScreenDelayed();

            this.hasControl = true;
        }

        public void EndLevelRaid()
        {
            BugOut();

            // Remove last checkpoint
            this.raidMode.previousCheckpoint = null;
        }

        public void LeftStick(float x, float y, float z)
        {
            if (hasControl == false)
            {
                return;
            }

            MovePlayer(x, y, z);
        }

        public void RightStick(float x, float y, float z)
        {
            if (hasControl == false)
            {
                return;
            }

            if (isInBuildMode == false)
            {
                return;
            }

            //BuyCursor.Move(x, z);
        }

        public void DPad(float x, int y, float z)
        {
            if (hasControl)
            {
                return;
            }

            if (isInBuildMode == false)
            {
                return;
            }

            if (z == 1)
            {
                buildMode.DpadUp();
            }

            if (z == -1)
            {
                buildMode.DpadDown();
            }
        }

        protected void MovePlayer(float x, float y, float z)
        {
            if (isInBuildMode == true)
            {
                this.buildMode.buildCursor.Move(x, z);
            }
            else
            {
                x *= 0.3f;
                z *= 0.3f;

                raidMode.currentRaider.Move(x, z);
            }

            playerCamera.MoveMainCamera(x, y, z);
            //SetMainCameraPosition(cameraPosition);
        }

        public void RespondToKeyEvent(KeyEvent keyEvent)
        {
            if (isLocalPlayer == false)
            {
                return;
            }

            ActionKey key = keyEvent.Key;

            if (keyEvent.IsComplete == false)
            {
                Debug.Log(string.Format("{0} key {1} at {2}", key.ToString(), keyEvent.EventType.ToString(), Time.time.ToString()));
            }
            else
            {
                Debug.Log(string.Format("{0} key released at {1}", key.ToString(), Time.time.ToString()));
            }

            if (hasControl == false)
            {
                if (key != ActionKey.Menu)
                {
                    return;
                }
            }

            switch (key)
            {
                case ActionKey.Action1:
                    if (isInBuildMode == true)
                    {
                        this.buildMode.Action1();
                    }
                    break;

                case ActionKey.Action2:
                    if (isInBuildMode == true)
                    {
                        this.buildMode.Action2();
                    }
                    else
                    {
                        this.raidMode.SetIsBoosting(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.Special1:
                    if (isInBuildMode == true)
                    {
                        this.buildMode.Special1(keyEvent);
                    }
                    else
                    {
                        this.raidMode.SetSpecialFiring(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.Special2:
                    if (isInBuildMode == true)
                    {
                        buildMode.Special2();
                    }
                    else
                    {
                        raidMode.SetEquipmentActive(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.DUp:
                    if (isInBuildMode == true)
                    {
                        buildMode.DpadUp();
                    }
                    break;

                case ActionKey.DDown:
                    if (isInBuildMode == true)
                    {
                        buildMode.DpadDown();
                    }
                    break;

                case ActionKey.RightTrigger:
                    if (isInBuildMode == true)
                    {
                    }
                    else
                    {
                        this.raidMode.SetPrimaryFiring(!keyEvent.IsComplete);
                    }
                    break;

                case ActionKey.LeftTrigger:
                    if (isInBuildMode == true)
                    {
                    }
                    else
                    {
                        this.raidMode.SetSecondaryFiring(!keyEvent.IsComplete);
                    }
                    break;
            }
        }

        protected void SetBuildMode(bool buildMode)
        {
            this.isInBuildMode = buildMode;
            this.buildMode.hud.enabled = buildMode;
            this.raidMode.hud.enabled = !buildMode;
        }

        protected void ToggleBuildRaidModes()
        {
            this.isInBuildMode = !isInBuildMode;
            this.buildMode.hud.enabled = isInBuildMode;
            this.raidMode.hud.enabled = !isInBuildMode;
        }
    }
}