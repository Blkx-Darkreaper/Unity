using UnityEngine;
using System.Collections;

public class _DestroyByTime : MonoBehaviour
{
    public float lifetime;

    public void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
