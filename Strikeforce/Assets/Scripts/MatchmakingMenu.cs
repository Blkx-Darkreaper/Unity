using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace Strikeforce
{
    public class MatchmakingMenu : Menu
    {
        protected string hostGameButtonName { get; set; }
        public const string HOST_GAME = "Host Game";
        protected string joinGameButtonName { get; set; }
        public const string JOIN_GAME = "Join Game";
        protected string backButtonName { get; set; }
        protected NetworkManager networkManager;
        protected ServerManager serverManager;

        protected override void Update()
        {
            base.Update();

            Button joinGameButton = allButtons[joinGameButtonName];
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
            this.hostGameButtonName = AllButtonNames[0];
            this.joinGameButtonName = AllButtonNames[1];
            this.backButtonName = AllButtonNames[2];

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

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { HOST_GAME, JOIN_GAME, BACK };
        }

        protected override string[] GetMenuButtonNamesToAdd()
        {
            return new string[] { hostGameButtonName, joinGameButtonName };
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
            MenuManager.Singleton.SetLoadingScreenActive(true);
        }
    }
}