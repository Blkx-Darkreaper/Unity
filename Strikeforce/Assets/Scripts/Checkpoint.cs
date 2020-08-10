using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class Checkpoint : MonoBehaviour
    {
        public Vector2 location;
        public Size size;
        public bool isFinalCheckpoint { get; protected set; }

        protected void OnTriggerEnter(Collider other)
        {
            Raider raider = other.gameObject.GetComponent<Raider>();
            if (raider == null)
            {
                return;
            }

            if (isFinalCheckpoint == true)
            {
                raider.currentOwner.EndLevelRaid();
                return;
            }

            bool isBuggingOut = CheckIsBuggingOut(raider.currentOwner);
            if (isBuggingOut == true)
            {
                raider.currentOwner.BugOut();
                return;
            }

            raider.currentOwner.raidMode.previousCheckpoint = this;
        }

        protected bool CheckIsBuggingOut(Player player)
        {
            Checkpoint previousCheckpoint = player.raidMode.previousCheckpoint;
            if (previousCheckpoint == this)
            {
                return true;
            }

            return false;
        }
    }
}