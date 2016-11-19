using UnityEngine;

namespace Strikeforce
{
    public class Raider : Aircraft
    {
        protected override void DestroyEntity()
        {
            base.DestroyEntity();

            Debug.Log(string.Format("You have been shot down."));
        }
    }
}