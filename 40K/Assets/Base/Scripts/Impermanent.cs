using UnityEngine;

public abstract class Impermanent : MonoBehaviour
{
    [ReadOnlyInInspector]
    public float lifetime = -1; // sec

    public virtual void OnEnable()
    {
        if (lifetime == -1)
        {
            return;
        }

        Invoke("Disable", lifetime);
    }

    public virtual void OnDisable()
    {
        CancelInvoke();

        Disable();
    }

    public abstract void Disable();

    public abstract void Destroy();
}