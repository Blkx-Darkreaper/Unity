using System;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class ServerManager : NetworkDiscovery
    {
        protected void Awake()
        {
            this.showGUI = false;

            Initialize();
            StartAsClient();
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
 	         base.OnReceivedBroadcast(fromAddress, data);

             NetworkManager.singleton.networkAddress = fromAddress;
        }
    }
}