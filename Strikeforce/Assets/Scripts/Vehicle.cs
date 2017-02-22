using UnityEngine;
using UnityEngine.Networking;
using System.Drawing;

namespace Strikeforce
{
    public class Vehicle : Destructible
    {
        public int RepairCost = 1;
        public int MaxSpeed = 5;
        public int Acceleration = 1;
        public int Range = 50;
        [SyncVar]
        protected float distanceTravelled;
        public int MaxFuelCapacity = 0;
        [SyncVar]
        protected float fuelRemaining;
        public int FuelConsumption = 0;

        protected override void Awake()
        {
            base.Awake();

            this.distanceTravelled = 0f;
            this.fuelRemaining = 0f;
        }

        public virtual void Move(float x, float z)
        {
            if (isServer == false)
            {
                return;
            }

            if (x == 0f)
            {
                if (z == 0f)
                {
                    return;
                }
            }

            Vector3 currentPosition = transform.position;

            CurrentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            transform.Translate(x, 0, z);
        }
    }
}