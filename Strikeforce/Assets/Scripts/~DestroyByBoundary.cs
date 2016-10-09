using UnityEngine;
using System.Collections;

public class DestroyByBoundary : MonoBehaviour {

    public void OnTriggerExit(Collider other)
    {
        Debug.Log(string.Format("{0} has left the boundary", other.name));
        Destroy(other.gameObject);
    }
}
