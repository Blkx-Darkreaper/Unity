using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace Strikeforce
{
    public class MatchmakingMenu : Menu
    {
        public const string HOST_GAME = "Host Game";
        public const string JOIN_GAME = "Join Game";
        protected NetworkManager networkManager;
        protected ServerManager serverManager;
        public GameMenu gameMenu;

        protected override void Awake()
        {
            // Hookup back button first
            Button backButton = GlobalAssets.GetChildComponentWithTag<Button>(gameObject, Tags.BUTTON);
            AddButtonHandler(backButton, BACK);

            base.Awake();

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

        protected override void SetButtonNames()
        {
            this.buttonNames = new string[] { HOST_GAME, JOIN_GAME, BACK };
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
            MenuManager.Singleton.SetLoadingScreenActive(true);
            MenuManager.Singleton.ShowMenu(gameMenu);
        }
    }
}