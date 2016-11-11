using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class PlayerMove : NetworkBehaviour
    {
        public override void OnStartLocalPlayer()
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }

        void Update()
        {
            if (isLocalPlayer == false)
            {
                return;
            }

            var x = Input.GetAxis("Horizontal") * 0.1f;
            var z = Input.GetAxis("Vertical") * 0.1f;

            transform.Translate(x, 0, z);
        }
    }
}