﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace Strikeforce
{
    public class MatchmakingMenu : Menu
    {
        protected const string HOST_GAME = "HostGame";
        protected const string JOIN_GAME = "JoinGame";
        public string hostGameText = "Host Game";
        protected string joinGameText = "Join Game";
        protected NetworkManager networkManager;
        protected ServerManager serverManager;

        protected override void Update()
        {
            base.Update();

            Button joinGameButton = allButtons[JOIN_GAME];
            joinGameButton.enabled = false;

            if (serverManager == null)
            {
                return;
            }
            if (serverManager.broadcastsReceived.Count == 0)
            {
                return;
            }

            joinGameButton.enabled = true;
        }

        protected override void Init()
        {
            base.Init();

            this.networkManager = NetworkManager.singleton;
            if (networkManager == null)
            {
                Debug.Log(string.Format("NetworkManager failed to load."));
            }

            this.serverManager = networkManager.gameObject.GetComponent<ServerManager>() as ServerManager;
            if (serverManager == null)
            {
                Debug.Log(string.Format("ServerManager failed to load."));
            }
        }

        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { HOST_GAME, JOIN_GAME, BACK };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { hostGameText, joinGameText, BACK };
        }

        protected override string[] GetMenuButtonNamesToAdd()
        {
            return new string[] { HOST_GAME, JOIN_GAME };
        }

        protected override void HandleButtonPress(string buttonName)
        {
            switch (buttonName)
            {
                case HOST_GAME:
                    HostGame();
                    break;

                case JOIN_GAME:
                    JoinGame();
                    break;

                case BACK:
                    Back();
                    break;
            }
        }

        protected void HostGame()
        {
            LoadGame();

            //serverManager.StartAsServer();
            networkManager.StartHost();
        }

        protected void JoinGame()
        {
            LoadGame();

            networkManager.StartClient();
        }

        protected virtual void LoadGame()
        {
            MenuManager.Singleton.ShowLoadingScreen();
        }
    }
}