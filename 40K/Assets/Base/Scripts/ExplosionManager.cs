using System;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager singleton = null;
    //public List<GameObject> allExplosionPrefabs;
    protected Dictionary<string, ExplosionPool> allExplosionPools;

    public void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }

        if (this != singleton)
        {
            Destroy(this);
            return;
        }

        this.allExplosionPools = new Dictionary<string, ExplosionPool>();
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        ExplosionPool[] allPools = GetComponents<ExplosionPool>();
        foreach (ExplosionPool pool in allPools)
        {
            GameObject prefab = pool.explosionPrefab;
            allExplosionPools.Add(prefab.name, pool);
        }
    }

    public Explosion GetExplosionByName(string name)
    {
        if (allExplosionPools.ContainsKey(name) == false)
        {
            return null;
        }

        ExplosionPool pool = allExplosionPools[name];
        Explosion explosion = pool.GetNextObject().GetComponent<Explosion>();
        return explosion;
    }

    //public void ReleaseExplosion(Explosion explosion)
    //{
    //    string name = explosion.name.Replace("(Clone)", "");
    //    if (allExplosionPools.ContainsKey(name) == false)
    //    {
    //DestroyObject(explosion);
    //        return;
    //    }

    //    ExplosionPool pool = allExplosionPools[name];
    //    pool.ReleaseObject(explosion);
    //}

    //public void OnValidate()
    //{
    //    int totalPrefabs = allExplosionPrefabs.Count;
    //    int totalPools = GetComponents<ExplosionPool>().Length;

    //    if (totalPrefabs == totalPools)
    //    {
    //        return;
    //    }

    //    foreach (GameObject prefab in allExplosionPrefabs)
    //    {
    //        bool hasPool = allExplosionPools.ContainsKey(prefab.name);
    //        if (hasPool == true)
    //        {
    //            continue;
    //        }

    //        ExplosionPool pool = gameObject.AddComponent<ExplosionPool>();
    //        pool.explosionPrefab = prefab;

    //        allExplosionPools.Add(prefab.name, pool);
    //    }
    //}
}

[Serializable]
public class ExplosionPool : PoolManager<Explosion>
{
    public GameObject explosionPrefab;

    public override Explosion CreateObject()
    {
        GameObject gameObject = Instantiate(explosionPrefab, transform);
        gameObject.SetActive(false);

        Explosion explosion = gameObject.GetComponent<Explosion>();
        return explosion;
    }

    public override void DestroyObject(Explosion explosion)
    {
        Destroy(explosion.gameObject);
    }

    public override bool IsObjectAvailable(Explosion explosion)
    {
        return !explosion.gameObject.activeInHierarchy;
    }

    protected override void CleanUpObject(Explosion explosion)
    {
        explosion.gameObject.SetActive(false);
    }
}