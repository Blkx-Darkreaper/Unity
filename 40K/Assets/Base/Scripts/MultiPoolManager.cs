using System.Collections.Generic;
using UnityEngine;

public abstract class MultiPoolManager<T, T2> : MonoBehaviour
    where T : GameObjectPoolManager<T2>
    where T2 : Component
{
    protected Dictionary<string, T> allPools;

    public abstract void Awake();

    public virtual void Start()
    {
        T[] allPools = GetComponents<T>();
        foreach (T pool in allPools)
        {
            GameObject prefab = pool.prefab;
            this.allPools.Add(prefab.name, pool);
        }
    }

    public T2 GetPrefabByName(string name)
    {
        if (allPools.ContainsKey(name) == false)
        {
            return null;
        }

        T pool = allPools[name];
        T2 prefab = pool.GetNextObject().GetComponent<T2>();
        return prefab;
    }
}