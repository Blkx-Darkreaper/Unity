using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DisableByBoundary : MonoBehaviour
{
    public void OnTriggerExit(Collider other)
    {
        Debug.Log($"{other.name} has left the bounds of {name}");
        other.gameObject.SetActive(false);
    }

    public void OnTriggerExit(Collider2D other)
    {
        Debug.Log($"{other.name} has left the bounds of {name}");
        other.gameObject.SetActive(false);
    }
}