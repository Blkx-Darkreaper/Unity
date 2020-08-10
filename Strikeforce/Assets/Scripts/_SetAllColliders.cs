using UnityEngine;

namespace Strikeforce
{
    public class _SetAllColliders : MonoBehaviour
    {
        public void SetColliders(bool enabled)
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in allColliders)
            {
                collider.enabled = enabled;
            }
        }
    }
}