using UnityEngine;
using System.Collections;

public class _DestroyByBoundary : MonoBehaviour {

    public void OnTriggerExit(Collider other)
    {
        GameObject entity = other.gameObject;
        if (other.transform.parent != null)
        {
            entity = other.transform.parent.gameObject;
        }

        Debug.Log(string.Format("{0} has left the boundary", entity.name));
        Destroy(other.gameObject);
    }
}
