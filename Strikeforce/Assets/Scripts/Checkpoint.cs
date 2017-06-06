using UnityEngine;
using System.Drawing;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Checkpoint : MonoBehaviour
    {
        public Vector2 Location;
        public Size Size;

        [JsonConstructor]
        public Checkpoint(Point location, Size size)
        {
            this.Location = new Vector2(location.X, location.Y);
            this.Size = size;
        }

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