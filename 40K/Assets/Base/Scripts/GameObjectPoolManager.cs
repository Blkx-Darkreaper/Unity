using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameObjectPoolManager<T> : PoolManager<T>
    where T : Component
{
    public GameObject prefab;
}