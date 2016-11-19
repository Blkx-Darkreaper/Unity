using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Strikeforce
{
    public class MatchmakingMenu : Menu
    {
        public const string HOST_GAME = "Host Game";
        public const string JOIN_GAME = "Join Game";
        protected NetworkManager networkManager;

        protected override void Awake()
        {
            base.Awake();

            networkManager = NetworkManager.singleton;
            if (networkManager == null)
            {
                Debug.Log(string.Format("NetworkManager failed to load."));
            }
        }

        protected override void SetButtonNames()
        {
            buttonNames = new string[] { HOST_GAME, JOIN_GAME };
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
            }
        }

        protected virtual void HostGame()
        {
            Resume();

            networkManager.StartHost();
        }

        protected virtual void JoinGame()
        {
            Resume();

            //networkManager.StartClient();
        }
    }
}