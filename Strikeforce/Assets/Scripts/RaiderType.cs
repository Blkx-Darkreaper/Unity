using UnityEngine;
using System;

namespace Strikeforce
{
    public class RaiderType : ScriptableObject
    {
        public float StartingSpeed;
        public float MaxSpeed;
        public float MaxEnergy;
        public Vector3[] AllFiringPoints;
    }
}