using UnityEngine;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Checkpoint : MonoBehaviour
    {
        public Vector2 Location;
        public Size Size;

        protected void OnTriggerEnter(Collider other)
        {
            Raider raider = other.gameObject.GetComponent<Raider>();
            if(raider == null)
            {
                return;
            }

            raider.Owner.PreviousCheckpoint = this;
        }
    }
}