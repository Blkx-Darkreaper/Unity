using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Manager {
	public GameObject Prefab;
	NetworkClient client;

	// Create a client and connect to the server port
	public void SetupClient() {
		ClientScene.RegisterPrefab(Prefab);
		
		client = new NetworkClient();

		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.Connect("127.0.0.1", 4444);
	}
}