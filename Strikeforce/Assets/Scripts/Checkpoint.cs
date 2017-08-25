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
                raider.Owner.EndLevelRaid();
            }

            bool isBuggingOut = CheckIsBuggingOut(raider.Owner);
            if(isBuggingOut == true)
            {
                raider.Owner.BugOut();
                return;
            }

            raider.Owner.PreviousCheckpoint = this;
        }

        protected bool CheckIsBuggingOut(Player player)
        {
            Checkpoint previousCheckpoint = player.PreviousCheckpoint;
            if(previousCheckpoint == this)
            {
                return true;
            }

            return false;
        }
    }
}