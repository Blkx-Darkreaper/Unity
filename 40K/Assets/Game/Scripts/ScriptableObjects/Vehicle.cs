using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "vehicle", menuName = "40K/Unit/Vehicle", order = 2)]
public class Vehicle : Unit
{
    public struct Armour
    {
        public int front;
    }

    public Armour armour;
}