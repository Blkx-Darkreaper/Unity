using UnityEngine;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Checkpoint : MonoBehaviour
    {
        public Vector2 Location;
        public Size Size;
        public bool IsFinalCheckpoint { get; protected set; }

        protected void OnTriggerEnter(Collider other)
        {
            Raider raider = other.gameObject.GetComponent<Raider>();
            if(raider == null)
            {
                return;
            }

            if(IsFinalCheckpoint == true)
            {
                raider.Owner.EndOfLevel();
            }

            raider.Owner.PreviousCheckpoint = this;
        }
    }
}