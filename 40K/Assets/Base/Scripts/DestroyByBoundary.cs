using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DestroyByBoundary : MonoBehaviour
{
    public void OnTriggerExit(Collider other)
    {
        Debug.Log($"{other.name} has left the bounds of {name}");

        Impermanent impermanent = other.GetComponent<Impermanent>();
        if (impermanent)
        {
            impermanent.Destroy();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider2D other)
    {
        Debug.Log($"{other.name} has left the bounds of {name}");

        Impermanent impermanent = other.GetComponent<Impermanent>();
        if (impermanent)
        {
            impermanent.Destroy();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}