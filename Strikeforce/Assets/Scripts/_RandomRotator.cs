﻿using UnityEngine;
using System.Collections;

public class _RandomRotator : MonoBehaviour
{
    public float tumble;

    public void Start()
    {
        var asteroid = GetComponent<Rigidbody>();
        asteroid.angularVelocity = Random.insideUnitSphere * tumble;
    }
}
