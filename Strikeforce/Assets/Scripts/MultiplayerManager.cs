using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class MultiplayerManager : Manager
    {
        public GameObject Prefab;
        NetworkClient client;

        // Create a client and connect to the server port
        public void SetupClient()
        {
            ClientScene.RegisterPrefab(Prefab);

            client = new NetworkClient();

            client.RegisterHandler(MsgType.Connect, OnConnected);
            client.Connect("127.0.0.1", 4444);
        }

        protected void OnConnected(NetworkMessage message)
        {
            Debug.Log("Client connected");
        }
    }
}