using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class RallyPoint : MonoBehaviour
    {
        public void Enable()
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = true;
            }
        }

        public void Disable()
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
        }
    }
}
