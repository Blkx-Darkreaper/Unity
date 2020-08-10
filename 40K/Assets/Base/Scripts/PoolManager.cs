using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PoolManager<T> : MonoBehaviour
{
    //public static PoolManager<T> singleton = null;

    public int poolSize = 5;
    public int maxPoolSize = 100;
    public bool canGrow = true;
    public int historySize = 5;
    [ReadOnlyInInspector]
    public float avgInUse = 0;
    [ReadOnlyInInspector]
    public int totalInUse = 0;
    //public T pooledObjectPrefab;
    [ReadOnlyInInspector]
    [SerializeField]
    protected bool isPoolActive = false;

    protected List<T> allPooledObjects = null;
    [SerializeField]
    protected Queue<int> useHistory;

    public void Awake()
    {
        //    if (singleton == null)
        //    {
        //        singleton = this;
        //    }

        //    if (this != singleton)
        //    {
        //        Destroy(this);
        //        return;
        //    }

        //    DontDestroyOnLoad(gameObject);

        this.useHistory = new Queue<int>(historySize);
    }

    public void Start()
    {
        this.allPooledObjects = new List<T>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            T obj = CreateObject();
            allPooledObjects.Add(obj);
        }
    }

    //public void Update()
    //{
    //    if(isPoolActive == false)
    //    {
    //        return;
    //    }

    //    ResizePool();
    //}

    public virtual T GetNextObject()
    {
        this.isPoolActive = true;

        T pooledObject = default(T);

        foreach (T obj in allPooledObjects)
        {
            if (IsObjectAvailable(obj) == false)
            {
                continue;
            }

            pooledObject = obj;
            break;
        }

        if (pooledObject == null && canGrow == true)
        {
            pooledObject = CreateObject();
            poolSize++;
        }

        if (pooledObject == null)
        {
            return pooledObject;
        }

        CleanUpObject(pooledObject);

        //totalInUse++;
        //UpdateAverageInUse();

        return pooledObject;
    }

    //public void ReleaseObject(T obj)
    //{
    //    CleanUpObject(obj);

    //    if (isPoolActive == false)
    //    {
    //        return;
    //    }

    //    totalInUse--;
    //    UpdateAverageInUse();
    //}

    public abstract bool IsObjectAvailable(T obj);

    public abstract T CreateObject();

    public abstract void DestroyObject(T obj);

    protected abstract void CleanUpObject(T obj);

    protected virtual void UpdateAverageInUse()
    {
        if(allPooledObjects == null)
        {
            return;
        }

        this.useHistory.Enqueue(totalInUse);

        while (useHistory.Count > historySize)
        {
            this.useHistory.Dequeue();
        }

        int total = 0;
        foreach (int count in useHistory)
        {
            total += count;
        }

        this.avgInUse = total / (float)useHistory.Count;
    }

    public virtual void ResizePool()
    {
        this.poolSize = allPooledObjects.Count;

        if (canGrow != true)
        {
            return;
        }

        // Don't resize without sufficient data points
        if (useHistory.Count < historySize)
        {
            return;
        }

        if (poolSize > 2 * avgInUse)
        {
            DecreasePoolSize(poolSize / 2);
        }

        if (poolSize < 2 * avgInUse)
        {
            IncreasePoolSize(poolSize * 2);
        }
    }

    public virtual void IncreasePoolSize(int size)
    {
        if (canGrow != true)
        {
            return;
        }

        if (size > maxPoolSize)
        {
            size = maxPoolSize;
        }

        if (size == poolSize)
        {
            return;
        }

        int numbertoAdd = size - poolSize;
        for (int i = 0; i < numbertoAdd; i++)
        {
            T obj = CreateObject();
            allPooledObjects.Add(obj);
        }

        this.poolSize = size;
    }

    public virtual void DecreasePoolSize(int size)
    {
        if (canGrow != true)
        {
            return;
        }

        if (size == poolSize)
        {
            return;
        }

        int numberToRemove = poolSize - size;
        int totalRemoved = 0;

        for (int i = allPooledObjects.Count - 1; i >= 0; i--)
        {
            if (totalRemoved == numberToRemove)
            {
                break;
            }

            T obj = allPooledObjects[i];

            bool canBeRemoved = IsObjectAvailable(obj);
            if (canBeRemoved == false)
            {
                continue;
            }

            allPooledObjects.RemoveAt(i);
            DestroyObject(obj);
            totalRemoved++;
        }
    }
}